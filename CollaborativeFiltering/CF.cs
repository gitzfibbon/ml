using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
    //
    // MAE of around ~.69 and RMSE of around ~.88 are in the correct range
    //
    public class CF
    {
        private const char Delimiter = ',';
        private const int NumColumns = 3;
        private const int ItemIdColumn = 0;
        private const int UserIdColumn = 1;
        private const int RatingColumn = 2;

        // We will maintain several data structures to optimize speed over memory.
        public List<float[]> TrainingData; // all of the training data 3-tuples
        public List<float[]> TestingData; // all of the testing data 3-tuples
        //public Dictionary<float, float> User_AverageRatings; // Hashtable lookup by user for their average rating
        //public Dictionary<float, Dictionary<float, float>> User_Items; // Hashtable lookup by user all their rated items
        //public Dictionary<float, HashSet<float>> Item_Users; // Hashtable lookup by item which users rated them
        public ConcurrentDictionary<int, float> User_AverageRatings; // Hashtable lookup by user for their average rating
        public ConcurrentDictionary<int, ConcurrentDictionary<int, float>> User_Items; // Hashtable lookup by user all their rated items
        public ConcurrentDictionary<int, HashSet<int>> Item_Users; // Hashtable lookup by item which users rated them
        public ConcurrentDictionary<int, ConcurrentDictionary<int, float>> Correlations; // Stores the correlations between two users
        private const int MaxSizeOfCorrelations = 1000000;
        private int SizeOfCorrelations = 0;
        private int CorrelationReuseCount = 0;


        public void PredictAll(int? maxPredictions)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();


            float absoluteErrorSum = 0;
            float absoluteErrorSquaredSum = 0;
            int numPredictions = maxPredictions == null ? this.TestingData.Count() : (int)maxPredictions;
            int predictionCount = 0;
            Log.LogVerbose("predictionCount,i,difference,predicted,actual,user,item,minsElapsed,cachedWeightsCount");
            //Parallel.For(0, numPredictions, new ParallelOptions { MaxDegreeOfParallelism = 40 }, i =>
            Parallel.For(0, numPredictions, i =>
            {
                int userId = Convert.ToInt32(this.TestingData[i][CF.UserIdColumn]);
                int itemId = Convert.ToInt32(this.TestingData[i][CF.ItemIdColumn]);
                float actualRating = this.TestingData[i][CF.RatingColumn];
                float predictedRating = this.PredictVote(userId, itemId);
                float absoluteError = Math.Abs(predictedRating - actualRating);

                absoluteErrorSum += absoluteError;
                absoluteErrorSquaredSum += absoluteError * absoluteError;

                Log.LogVerbose("{0},{1},{2:0.00},{3:0.00},{4:0.00},{5},{6},{7:0.00},{8}",
                    predictionCount++, i, absoluteError, predictedRating, actualRating, userId, itemId, stopwatch.Elapsed.TotalMinutes, this.SizeOfCorrelations);
            });

            float mae = absoluteErrorSum / numPredictions;
            double rmse = Math.Sqrt(absoluteErrorSquaredSum / numPredictions);

            stopwatch.Stop();
            Log.LogImportant("");
            Log.LogImportant("Finished Predicting");
            Log.LogImportant("Number of Predictions: {0}", numPredictions);
            Log.LogImportant("Prediction Time in Minutes: {0:0.00}", stopwatch.Elapsed.TotalMinutes);
            Log.LogImportant("MAE: {0:0.0000}", mae);
            Log.LogImportant("RMSE: {0:0.0000}", rmse);
            Log.LogVerbose("Reused Correlation Calculations: {0}", this.CorrelationReuseCount);
        }

        // Implements 2.1 Eq. 1
        private float PredictVote(int userId, int itemId)
        {
            // Declare several variables that will be used over the summation in the equation
            int a = userId;
            int j = itemId;
            float w = 0; // weight w(a,i)
            float mean_v_i = 0; // the other users mean vote
            float v_ij = 0; // the other users rating for item j
            float sum = 0; // summation from the equation
            float k = 0; // normalizing factor

            // Iterate through each user in the collaborative filtering database
            foreach (int i in this.Item_Users[itemId])  // i is the other users we are iterating through
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

            float mean_v_a = this.User_AverageRatings[a]; // Mean vote for user a

            // If k is 0 then just return the active user's mean rating per https://catalyst.uw.edu/gopost/conversation/fsadeghi/961287#3264743
            if (k == 0)
            {
                return mean_v_a;
            }

            float p_aj = mean_v_a - (sum / k);

            if (float.IsNaN(p_aj))
            {
            }

            return p_aj;
        }

        // Implements 2.1 Eq. 2
        private float GetWeight(int activeUserId, int otherUserId)
        {
            // Rename the arguments to match the equations in the paper
            int a = activeUserId;
            int i = otherUserId;

            // Check if we've already calculated the correlation weight for these two users
            float storedWeight = this.LookupWeight(a, i);
            if (!float.IsNaN(storedWeight))
            {
                return storedWeight;
            }

            // Get all items common between both users
            List<int> commonItems = this.GetCommonItems(a, i);

            // Calculate activeUser (a) and otherUser (i) mean over the common items
            float sumA = 0;
            float sumI = 0;
            foreach (int commonItem in commonItems)
            {
                sumA += this.User_Items[a][commonItem];
                sumI += this.User_Items[i][commonItem];
            }
            float mean_v_a = sumA / commonItems.Count;
            float mean_v_i = sumI / commonItems.Count;

            // Iterate over all common items
            float A = 0;
            float I = 0;
            float A2 = 0;
            float I2 = 0;
            float numerator = 0;
            float denominator = 0;
            foreach (int commonItem in commonItems)
            {
                // Calculate A = v_aj - mean_v_a
                A = this.User_Items[a][commonItem] - mean_v_a;

                // Calculate I = v_ij - mean_v_i
                I = this.User_Items[i][commonItem] - mean_v_i;

                // Get the product A and I and of these two and add it to a running sum
                numerator += A * I;

                // Get the product of A^2 and I^2 and add to a running sum
                A2 += A * A;
                I2 += I * I;
            }

            denominator = Convert.ToSingle(Math.Sqrt(A2 * I2));

            float weight = numerator / denominator;

            if (float.IsNaN(weight))
            {
                Log.LogPedantic("Weight is NaN for users {0} and {1}. Setting it to 0.", a, i);
                weight = 0;
            }

            // Save the weight so we don't have to calculate it again
            if (this.SizeOfCorrelations < CF.MaxSizeOfCorrelations)
            {
                this.SaveWeight(a, i, weight);
            }

            // TODO: remove
            if (weight == 0)
            {
            }
            else if (weight < 0)
            {
            }

            return weight;
        }

        private float LookupWeight(int userId1, int userId2)
        {
            // The minUserId is always stored as the first key
            int minUserId = Math.Min(userId1, userId2);
            int maxUserId = Math.Max(userId1, userId2);

            // Check if the weight has already been calculated
            if (this.Correlations.ContainsKey(minUserId))
            {
                if (this.Correlations[minUserId].ContainsKey(maxUserId))
                {
                    // If we get here we already calculated the weight for these users
                    Log.LogPedantic("Found stored weight for users {0} and {1}", userId1, userId2);
                    CorrelationReuseCount++;
                    return this.Correlations[minUserId][maxUserId];
                }
            }

            return float.NaN;
        }

        private void SaveWeight(int userId1, int userId2, float weight)
        {
            // The minUserId is always stored as the first key
            int minUserId = Math.Min(userId1, userId2);
            int maxUserId = Math.Max(userId1, userId2);

            // Check if the user is already in the dictionary
            if (!this.Correlations.ContainsKey(minUserId))
            {
                this.Correlations.TryAdd(minUserId, new ConcurrentDictionary<int, float>());
            }

            this.Correlations[minUserId].TryAdd(maxUserId, weight);
            this.SizeOfCorrelations++;
        }

        private List<int> GetCommonItems(int userId1, int userId2)
        {
            List<int> commonItems = new List<int>();

            if (this.User_Items.ContainsKey(userId1) && this.User_Items.ContainsKey(userId2))
            {
                List<int> user1Items = this.User_Items[userId1].Keys.ToList();
                List<int> user2Items = this.User_Items[userId2].Keys.ToList();

                commonItems = user1Items.Intersect(user2Items).ToList();
            }

            return commonItems;
        }

        public void Initialize(string trainingSetPath, string testingSetPath)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Log.LogVerbose("Initializing the training data");

            // Populate the other data structures of training data
            this.LoadData(trainingSetPath, out this.TrainingData);
            this.User_AverageRatings = new ConcurrentDictionary<int, float>();
            this.User_Items = new ConcurrentDictionary<int, ConcurrentDictionary<int, float>>();
            this.Item_Users = new ConcurrentDictionary<int, HashSet<int>>();
            this.Correlations = new ConcurrentDictionary<int, ConcurrentDictionary<int, float>>();

            Log.LogVerbose("Pre-populating user-item and item-user lookup tables");

            for (int i = 0; i < this.TrainingData.Count(); i++)
            {
                int userId = Convert.ToInt32(this.TrainingData[i][CF.UserIdColumn]);
                int itemId = Convert.ToInt32(this.TrainingData[i][CF.ItemIdColumn]);
                float rating = this.TrainingData[i][CF.RatingColumn];

                // Users
                if (!this.User_Items.ContainsKey(userId))
                {
                    this.User_Items.TryAdd(userId, new ConcurrentDictionary<int, float>());
                }

                if (!this.User_Items[userId].ContainsKey(itemId))
                {
                    this.User_Items[userId].TryAdd(itemId, rating);
                }

                // Items
                if (!this.Item_Users.ContainsKey(itemId))
                {
                    this.Item_Users.TryAdd(itemId, new HashSet<int>());
                }

                if (!this.Item_Users[itemId].Contains(userId))
                {
                    this.Item_Users[itemId].Add(userId);
                }
            }

            Log.LogVerbose("Pre-calculating user average ratings");

            // Calculate average rating per user
            foreach (int userId in this.User_Items.Keys)
            {
                int numItems = this.User_Items[userId].Count();
                float sum = 0;

                foreach (float rating in this.User_Items[userId].Values)
                {
                    sum += rating;
                }

                float averageRating = sum / numItems;
                this.User_AverageRatings.TryAdd(userId, averageRating);

            }

            Log.LogVerbose("Finished initializing training data with {0} users, {1} items", this.User_Items.Count(), this.Item_Users.Count());

            Log.LogVerbose("Initializing the testing data");
            this.LoadData(testingSetPath, out this.TestingData);

            stopwatch.Stop();
            Log.LogVerbose("Finished initializing the testing data. Elapsed time {0} seconds.", stopwatch.Elapsed.TotalSeconds);

        }

        private void LoadData(string filePath, out List<float[]> data)
        {
            Log.LogVerbose("Loading from {0}", filePath);

            // Load the data into an array
            data = new List<float[]>();
            using (StreamReader sr = File.OpenText(filePath))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    string[] sParts = s.Split(CF.Delimiter);
                    float[] item = new float[3];
                    item[CF.ItemIdColumn] = float.Parse(sParts[CF.ItemIdColumn]);
                    item[CF.UserIdColumn] = float.Parse(sParts[CF.UserIdColumn]);
                    item[CF.RatingColumn] = float.Parse(sParts[CF.RatingColumn]);
                    data.Add(item);
                }
            }

            Log.LogVerbose("Done loading {0} items", data.Count());

        }

    }
}
