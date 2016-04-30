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
            Log.LogAlwaysOn = true;
            Log.LogImportantOn = true;
            Log.LogVerboseOn = false;

            // Default training and testing files
            string trainingSetPath = @"C:\coding\ml\data\netflix_data\TrainingRatings.txt";
            string testingSetPath = @"C:\coding\ml\data\netflix_data\TestingRatings.txt";

            if (args.Length >= 2)
            {
                trainingSetPath = args[0];
                testingSetPath = args[1];
            }

            Log.LogAlways("trainingSetPath is {0}", trainingSetPath);
            Log.LogAlways("testingSetPath is {0}", testingSetPath);

            CF cf = new CF();
            cf.Initialize(trainingSetPath, testingSetPath);
            cf.PredictAll();
            Console.WriteLine();
            Console.WriteLine("Done. Press the anykey to continue.");
            Console.Read();
        }
    }
}
