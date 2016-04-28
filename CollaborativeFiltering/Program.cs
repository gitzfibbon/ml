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

            if (args.Length >= 2)
            {
                trainingSetPath = args[0];
                testingSetPath = args[1];
            }

            Console.WriteLine("trainingSetPath is {0}", trainingSetPath);
            Console.WriteLine("testingSetPath is {0}", testingSetPath);

            CF cf = new CF();
            cf.LoadData(trainingSetPath, testingSetPath);
            var trainingData = cf.TrainingData;
            var testingData = cf.TestingData;
        }
    }
}
