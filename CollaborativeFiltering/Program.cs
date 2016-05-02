using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollaborativeFiltering
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.LogImportant("");

            // Parse the args from the config

            string trainingSetPath = ConfigurationManager.AppSettings["TrainingSetPath"];
            string testingSetPath = ConfigurationManager.AppSettings["TestingSetPath"];
            string movieTitlesPath = ConfigurationManager.AppSettings["MovieTitlesPath"];

            if (String.IsNullOrWhiteSpace(trainingSetPath))
            {
                trainingSetPath = "TrainingRatings.txt";
                Log.LogImportant("TrainingSetPath in config file is not set. Default to {0} in current directory.", trainingSetPath);
            }

            if (String.IsNullOrWhiteSpace(testingSetPath))
            {
                testingSetPath = "TestingRatings.txt";
                Log.LogImportant("TestingSetPath in config file is not set. Default to {0} in current directory.", testingSetPath);
            }

            if (String.IsNullOrWhiteSpace(movieTitlesPath))
            {
                movieTitlesPath = null;
                Log.LogImportant("MovieTitlesPath in config file is not set. Will not display movie titles.");
            }

            int maxPredictions = 0;
            if (String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["MaxPredictions"]))
            {
                Log.LogImportant("MaxPredictions in config file is not set. Predicting for all rows in the test data.");
            }
            else
            {
                maxPredictions = Convert.ToInt32(ConfigurationManager.AppSettings["MaxPredictions"]);
            }

            Log.LogImportantOn = true;
            Log.LogVerboseOn = true;
            Log.LogPedanticOn = false;

            Log.LogImportant("");
            Log.LogImportant("Training Set Path is {0}", trainingSetPath);
            Log.LogImportant("Testing Set Path is {0}", testingSetPath);
            Log.LogImportant("Movie Titles Path is {0}", movieTitlesPath);
            Log.LogImportant("Max Predictions is {0}", maxPredictions);

            CF cf = new CF();

            string predictOrRank = ConfigurationManager.AppSettings["PredictOrRank"];
            if (predictOrRank.Equals("predict", StringComparison.InvariantCultureIgnoreCase))
            {
                Log.LogImportant("Predicting ratings for the users");
                cf.Initialize(trainingSetPath, testingSetPath, movieTitlesPath); 
                cf.PredictAll(maxPredictions <= 0 ? null : (int?)maxPredictions);

            }
            else if (predictOrRank.Equals("rank", StringComparison.InvariantCultureIgnoreCase))
            {
                string rankForUser = ConfigurationManager.AppSettings["RankForUser"];
                if (String.IsNullOrWhiteSpace(rankForUser))
                {
                    Log.LogImportant("RankForUser in config file is not set. It must be set when 'rank' is specified.");
                    return;
                }

                int rankForUserId = Convert.ToInt32(rankForUser);
                Log.LogImportant("Ranking ratings for the user {0}", rankForUserId);
                cf.Initialize(trainingSetPath, testingSetPath, movieTitlesPath);

            }
            else
            {
                Log.LogImportant("PredictOrRank in config file is not set. It must be set to 'predict' or 'rank'.");
                return;
            }

            Log.LogImportant("");

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Done. Press the anykey to continue.");
                Console.Read();
            }
        }
    }
}
