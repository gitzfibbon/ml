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
        public Dictionary<double, Dictionary<double, List<double>>> User_Common_Items; // Common Items between two users (Key is the lower of the two user Ids)

        public void PredictAll()
        {
            //for (int i=0; i < this.TestingData.Count(); i++)
            for (int i = 0; i < 5; i++)
            {
                double userId = this.TestingData[i][CF.UserIdColumn];
                double itemId = this.TestingData[i][CF.ItemIdColumn];
                double actualRating = this.TestingData[i][CF.RatingColumn];
                double predictedRating = this.PredictVote(userId, itemId);
                Log.LogVerbose("Predict {0}, Actual {1} for user {2} item {3}", predictedRating, actualRating, userId, itemId);
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
            foreach (double i in this.User_Items.Keys)  // i is the other users we are iterating through
            {
                // get the weight
                w = this.GetWeight(a, i);

                // if the weight is 0 don't do any more calculations
                if (w == 0)
                {
                    continue;
                }

                // get user i's rating for item j
                v_ij = this.User_Items[i][j];


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
            double p_aj = mean_v_a - (k * sum);
            return p_aj;
        }

        // Implements 2.1 Eq. 2
        private double GetWeight(double activeUserId, double otherUserId)
        {
            double a = activeUserId;
            double i = otherUserId;

            // Get all items common between both users

            // Calculate activeUser (a) mean over the common items

            // Calculate otherUser (i) mean over the common items

            // Iterate over all common items
            // loop

            // Calculate A = v_a - mean_a
            // Calculate I = v_i - mean_i
            // Get the product A and I and of these two and add it to a running sum (NUMER)
            // Get the A^2 and I^2 and add to a running sum (DENOM_A and DENOM_I)

            // endloop

            double weight = 0.5;
            // WEIGHT = NUMER / SQRT(DENOM_A * DENOM_I)

            // TODO: remove
            if (weight == 0)
            {
            }
            else if (weight < 0)
            {
            }

            return weight;
        }

        private List<double> GetCommonItems(double userId1, double userId2)
        {
            // The minUserId is always stored as the first key
            double minUserId = Math.Min(userId1, userId2);
            double maxUserId = Math.Max(userId1, userId2);

            // Check if the list has already been discovered and if not, add it to the data structure
            if (this.User_Common_Items.ContainsKey(minUserId))
            {
                if (this.User_Common_Items[minUserId].ContainsKey(maxUserId))
                {
                    // If we get here we already calculated common items for these users
                    return this.User_Common_Items[minUserId][maxUserId];
                }
                else
                {
                    // We need to add the common items and return
                    this.User_Common_Items[minUserId].Add(maxUserId, this.DetermineCommonItems(userId1, userId2));
                    return this.User_Common_Items[minUserId][maxUserId];
                }
            }

            // If we get this far, we need to add a new key to the dictionary for the minUserId and then add common items
            this.User_Common_Items.Add(minUserId, new Dictionary<double, List<double>>());
            this.User_Common_Items[minUserId].Add(maxUserId, this.DetermineCommonItems(userId1, userId2));
            return this.User_Common_Items[minUserId][maxUserId];

        }

        private List<double> DetermineCommonItems(double userId1, double userId2)
        {
            return null;
        }

        public void Initialize(string trainingSetPath, string testingSetPath)
        {
            Log.LogImportant("Initializing the training data");

            // Populate the other data structures of training data
            this.LoadData(trainingSetPath, out this.TrainingData);
            this.User_AverageRatings = new Dictionary<double, double>();
            this.User_Items = new Dictionary<double, Dictionary<double, double>>();
            this.Item_Users = new Dictionary<double, HashSet<double>>();
            this.User_Common_Items = new Dictionary<double, Dictionary<double, List<double>>>();

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
