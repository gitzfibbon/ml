using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollaborativeFiltering
{
    public class Program
    {
        public static void Main(string[] args)
        {

            // Default training and testing files
            string trainingSetPath = @"C:\coding\ml\data\netflix_data\TrainingRatings.txt";
            string testingSetPath = @"C:\coding\ml\data\netflix_data\TestingRatings.txt";
            int maxPredictions = 40;
            bool logVerbose = false;

            if (args.Length >= 3)
            {
                trainingSetPath = args[0];
                testingSetPath = args[1];
                maxPredictions = Convert.ToInt32(args[2]);

                if (args.Length >= 4)
                {
                    logVerbose = args[3].StartsWith("v", StringComparison.InvariantCultureIgnoreCase);
                }
            }

            Log.LogAlwaysOn = true;
            Log.LogImportantOn = logVerbose;
            Log.LogVerboseOn = false;
            Log.LogAlways("");
            Log.LogAlways("Training Set Path is {0}", trainingSetPath);
            Log.LogAlways("Testing Set Path is {0}", testingSetPath);
            Log.LogAlways("Max Predictions is {0}", maxPredictions);
            Log.LogAlways("Verbose Logging is {0}", logVerbose);

            CF cf = new CF();
            cf.Initialize(trainingSetPath, testingSetPath);
            cf.PredictAll(maxPredictions <= 0 ? null : (int?)maxPredictions);
            Console.WriteLine();
            Console.WriteLine("Done. Press the anykey to continue.");
            Console.Read();
        }
    }
}
