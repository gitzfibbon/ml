using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bagging;
using BV;

namespace SVM_BV
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string trainingFile = @"C:\coding\ml\data\diabetes\";
            string testingFile = @"C:\coding\ml\data\diabetes\";

            // Read in the training data
            List<string> trainingData = new List<string>();
            using (StreamReader sr = File.OpenText(trainingFile))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                { }
            }

        }

        private static List<int> RunSvm(string inputFile, int kernel, int? randomSeed)
        {
            string tempSampledTrainingData = @"C:\coding\svm\sampled.txt";
            int numTrainingInstances = 518;


            for (int i = 0; i < numTrainingInstances; i++ )
            { }

                return null;
        }
    }
}
