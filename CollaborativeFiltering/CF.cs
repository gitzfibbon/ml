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


        // Implements 2.1 Eq. 1
        public double PredictVote(double userId, double itemId)
        {
            double a = userId;

            // Get the mean vote for a
            double mean_v_a = 0;

            // Iterate through each user in the collaborative filtering database
            double sum = 0; // summation from the equation
            double k = 0; // normalizing factor
            double w = 0; // weight w(a,i)
            double i = 0; // the other users we are iterating through
            double mean_v_i = 0; // the other users mean vote
            double v_ij = 0; // the other users rating for item j
            // loop

            w = this.GetWeight(a, i);
            k += Math.Abs(w);

            // get mean_v_i
            // get user i's rating for movie j

            sum += w * (v_ij - mean_v_i);


            // endloop

            double p_aj = mean_v_a - (k * sum);
            return p_aj;
        }

        // Implements 2.1 Eq. 2
        public double GetWeight(double activeUserId, double otherUserId)
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

            double weight = 0.0;
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

        public void LoadData(string trainingSetPath, string testingSetPath)
        {
            // Populate the other data structures of training data
            this.LoadData(trainingSetPath, out this.TrainingData);
            this.User_AverageRatings = new Dictionary<double, double>();
            this.User_Items = new Dictionary<double, Dictionary<double, double>>();
            this.Item_Users = new Dictionary<double, HashSet<double>>();

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

            Console.WriteLine(this.User_Items.Count());
            Console.WriteLine(this.Item_Users.Count());
            Console.WriteLine(this.User_AverageRatings.Count());

            this.LoadData(testingSetPath, out this.TestingData);
        }

        private void LoadData(string filePath, out List<double[]> data)
        {
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
        }

    }
}
