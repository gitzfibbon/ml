using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollaborativeFiltering
{
    // Uses collaborative filtering defined in the paper:
    // Empirical Analysis of Predictive Algorithms for Collaborative Filtering
    // By John S. Breese, David Heckerman, Carl Kadie
    // http://courses.cs.washington.edu/courses/csep546/16sp/psetwww/2/algsweb.pdf 
    // 
    // Variable names will try to match the notation in the referenced paper.

    public class CF
    {
        private const char Delimiter = ',';
        private const int NumColumns = 3;
        private const int ItemIdColumn = 0;
        private const int UserIdColumn = 1;
        private const int RatingColumn = 2;

        // We will maintain several data structures to optimize speed over memory.
        public List<double[]> TrainingData; // all of the training data 3-tuples
        public List<double[]> TestingData; // all of the testing data 3-tuples
        public Dictionary<double, double> User_AverageRatings; // Hashtable lookup by user for their average rating
        public Dictionary<double, Dictionary<double, double>> User_Items; // Hashtable lookup by user all their rated items
        public Dictionary<double, HashSet<double>> Item_Users; // Hashtable lookup by item which users rated them
        public ConcurrentDictionary<double, ConcurrentDictionary<double, double>> Correlations; // Stores the correlations between two users


        public void PredictAll()
        {
            //for (int i=0; i < this.TestingData.Count(); i++)
            for (int i = 0; i < 1000; i++)
            {
                double userId = this.TestingData[i][CF.UserIdColumn];
                double itemId = this.TestingData[i][CF.ItemIdColumn];
                double actualRating = this.TestingData[i][CF.RatingColumn];
                double predictedRating = this.PredictVote(userId, itemId);
                Log.LogImportant("{0}. Predicted {1}, Actual {2} for user {3} item {4}",i, predictedRating, actualRating, userId, itemId);
            }
        }

        // Implements 2.1 Eq. 1
        public double PredictVote(double userId, double itemId)
        {
            // Declare several variables that will be used over the summation in the equation
            double a = userId;
            double j = itemId;
            double w = 0; // weight w(a,i)
            double mean_v_i = 0; // the other users mean vote
            double v_ij = 0; // the other users rating for item j
            double sum = 0; // summation from the equation
            double k = 0; // normalizing factor

            // Iterate through each user in the collaborative filtering database
            foreach (double i in this.Item_Users[itemId])  // i is the other users we are iterating through
            {
                // Check if this user already rated this item
                if (i == a)
                {
                    return this.User_Items[a][itemId];
                }

                if (!this.User_Items[i].ContainsKey(j))
                {
                    continue;
                }

                // get user i's rating for item j
                v_ij = this.User_Items[i][j];

                
                // get the weight
                w = this.GetWeight(a, i);

                // if the weight is 0 don't do any more calculations
                if (w == 0)
                {
                    continue;
                }

                // TODO: remove
                if (!this.User_AverageRatings.ContainsKey(i))
                {
                }

                // get the mean vote of user i
                mean_v_i = this.User_AverageRatings[i];

                // increment running totals
                k += Math.Abs(w);
                sum += w * (v_ij - mean_v_i);
            }

            // TODO: remove
            if (!this.User_AverageRatings.ContainsKey(a))
            {
            }

            double mean_v_a = this.User_AverageRatings[a]; // Mean vote for user a

            // If k is 0 then just return the active user's mean rating per https://catalyst.uw.edu/gopost/conversation/fsadeghi/961287#3264743
            if (k == 0)
            {
                return mean_v_a;
            }

            double p_aj = mean_v_a - (sum / k);

            if (Double.IsNaN(p_aj))
            {
            }

            return p_aj;
        }

        // Implements 2.1 Eq. 2
        private double GetWeight(double activeUserId, double otherUserId)
        {
            // Rename the arguments to match the equations in the paper
            double a = activeUserId;
            double i = otherUserId;

            // Check if we've already calculated the correlation weight for these two users
            double storedWeight = this.LookupWeight(a,i);
            if (!Double.IsNaN(storedWeight))
            {
                return storedWeight;
            }

            // Get all items common between both users
            List<double> commonItems = this.GetCommonItems(a, i);

            // Calculate activeUser (a) and otherUser (i) mean over the common items
            double sumA = 0;
            double sumI = 0;
            foreach (double commonItem in commonItems)
            {
                sumA += this.User_Items[a][commonItem];
                sumI += this.User_Items[i][commonItem];
            }
            double mean_v_a = sumA / commonItems.Count;
            double mean_v_i = sumI / commonItems.Count;

            // Iterate over all common items
            double A = 0;
            double I = 0;
            double A2 = 0;
            double I2 = 0;
            double numerator = 0;
            double denominator = 0;
            foreach (double commonItem in commonItems)
            {
                // Calculate A = v_aj - mean_v_a
                A = this.User_Items[a][commonItem] - mean_v_a;

                // Calculate I = v_ij - mean_v_i
                I = this.User_Items[i][commonItem] - mean_v_i;

                // Get the product A and I and of these two and add it to a running sum
                numerator += A * I;

                // Get the product of A^2 and I^2 and add to a running sum (DENOM_A and DENOM_I)
                A2 += A * A;
                I2 += I * I;
            }

            denominator = Math.Sqrt(A2 * I2);

            double weight = numerator / denominator;

            if (Double.IsNaN(weight))
            {
                Log.LogVerbose("Weight is NaN for users {0} and {1}. Setting it to 0.", a, i);
                weight = 0;
            }

            // Save the weight so we don't have to calculate it again
            this.SaveWeight(a, i, weight);

            // TODO: remove
            if (weight == 0)
            {
            }
            else if (weight < 0)
            {
            }

            return weight;
        }

        private double LookupWeight(double userId1, double userId2)
        {
            // The minUserId is always stored as the first key
            double minUserId = Math.Min(userId1, userId2);
            double maxUserId = Math.Max(userId1, userId2);

            // Check if the weight has already been calculated
            if (this.Correlations.ContainsKey(minUserId))
            {
                if (this.Correlations[minUserId].ContainsKey(maxUserId))
                {
                    // If we get here we already calculated the weight for these users
                    Log.LogVerbose("Found stored weight for users {0} and {1}", userId1, userId2);
                    return this.Correlations[minUserId][maxUserId];
                }
            }

            return Double.NaN;
        }

        private void SaveWeight(double userId1, double userId2, double weight)
        {
            // The minUserId is always stored as the first key
            double minUserId = Math.Min(userId1, userId2);
            double maxUserId = Math.Max(userId1, userId2);

            // Check if the user is already in the dictionary
            if (!this.Correlations.ContainsKey(minUserId))
            {
                this.Correlations.TryAdd(minUserId, new ConcurrentDictionary<double, double>());
            }

            this.Correlations[minUserId].TryAdd(maxUserId, weight);
        }

        private List<double> GetCommonItems(double userId1, double userId2)
        {
            List<double> commonItems = new List<double>();

            if (this.User_Items.ContainsKey(userId1) && this.User_Items.ContainsKey(userId2))
            {
                List<double> user1Items = this.User_Items[userId1].Keys.ToList();
                List<double> user2Items = this.User_Items[userId2].Keys.ToList();

                commonItems = user1Items.Intersect(user2Items).ToList();
            }

            return commonItems;
        }

        public void Initialize(string trainingSetPath, string testingSetPath)
        {
            Log.LogImportant("Initializing the training data");

            // Populate the other data structures of training data
            this.LoadData(trainingSetPath, out this.TrainingData);
            this.User_AverageRatings = new Dictionary<double, double>();
            this.User_Items = new Dictionary<double, Dictionary<double, double>>();
            this.Item_Users = new Dictionary<double, HashSet<double>>();
            this.Correlations = new ConcurrentDictionary<double, ConcurrentDictionary<double, double>>();

            Log.LogImportant("Pre-populating user-item and item-user lookup tables");

            for (int i = 0; i < this.TrainingData.Count(); i++)
            {
                double userId = this.TrainingData[i][CF.UserIdColumn];
                double itemId = this.TrainingData[i][CF.ItemIdColumn];
                double rating = this.TrainingData[i][CF.RatingColumn];

                // Users
                if (!this.User_Items.ContainsKey(userId))
                {
                    this.User_Items.Add(userId, new Dictionary<double, double>());
                }

                if (!this.User_Items[userId].ContainsKey(itemId))
                {
                    this.User_Items[userId].Add(itemId, rating);
                }

                // Items
                if (!this.Item_Users.ContainsKey(itemId))
                {
                    this.Item_Users.Add(itemId, new HashSet<double>());
                }

                if (!this.Item_Users[itemId].Contains(userId))
                {
                    this.Item_Users[itemId].Add(userId);
                }
            }

            Log.LogImportant("Pre-calculating user average ratings");

            // Calculate average rating per user
            foreach (double userId in this.User_Items.Keys)
            {
                double numItems = this.User_Items[userId].Count();
                double sum = 0;

                foreach (double rating in this.User_Items[userId].Values)
                {
                    sum += rating;
                }

                double averageRating = sum / numItems;
                this.User_AverageRatings.Add(userId, averageRating);

            }

            Log.LogImportant("Finished initializing training data with {0} users, {1} items", this.User_Items.Count(), this.Item_Users.Count());

            Log.LogImportant("Initializing the testing data");
            this.LoadData(testingSetPath, out this.TestingData);
            Log.LogImportant("Finished initializing the testing data");

        }

        private void LoadData(string filePath, out List<double[]> data)
        {
            Log.LogImportant("Loading from {0}", filePath);

            // Load the data into an array
            data = new List<double[]>();
            using (StreamReader sr = File.OpenText(filePath))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    string[] sParts = s.Split(CF.Delimiter);
                    double[] item = new double[3];
                    item[CF.ItemIdColumn] = Double.Parse(sParts[CF.ItemIdColumn]);
                    item[CF.UserIdColumn] = Double.Parse(sParts[CF.UserIdColumn]);
                    item[CF.RatingColumn] = Double.Parse(sParts[CF.RatingColumn]);
                    data.Add(item);
                }
            }

            Log.LogImportant("Done loading {0} items", data.Count());

        }

    }
}
