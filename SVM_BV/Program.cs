using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Bagging;
using BV;

namespace SVM_BV
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string trainingFile = @"C:\coding\ml\data\diabetes\diabetes_libsvmformat_train.txt";
            string testingFile = @"C:\coding\ml\data\diabetes\diabetes_libsvmformat_test.txt";

            // Read in the training data
            List<string> trainingData = new List<string>();
            using (StreamReader sr = File.OpenText(trainingFile))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    if (!String.IsNullOrWhiteSpace(s))
                    {
                        trainingData.Add(s);
                    }
                }
            }


            Program.RunSvm(trainingData, testingFile, 0, 0);



        }

        private static List<int> RunSvm(List<string> trainingData, string testingFile, int kernel, int? randomSeed)
        {
            string sampledTrainingFile = @"C:\coding\svm\sampledTraining.txt";
            string sampledModelFile = @"C:\coding\svm\sampledModel.txt";
            string sampledResultsFile = @"C:\coding\svm\sampledResults.txt";

            
            Random random;
            if (randomSeed == null)
            {
                random = new Random();
            }
            else
            {
                random = new Random((int)randomSeed);
            }

            StringBuilder sampledTrainingData = new StringBuilder();

            for (int i = 0; i < trainingData.Count; i++)
            {
                int randomIndex = random.Next(0, trainingData.Count - 1);
                sampledTrainingData.AppendLine(trainingData.ElementAt(randomIndex));
            }

            File.WriteAllText(sampledTrainingFile, sampledTrainingData.ToString());

            string svmTrainExe = @"C:\tools\libsvm-3.21\svm-train.exe";
            string trainArgs = String.Format("-t {0} {1} {2}", kernel, sampledTrainingFile, sampledModelFile);
            Process.Start(svmTrainExe, trainArgs);

            Thread.Sleep(1000);

            string svmTestExe = @"C:\tools\libsvm-3.21\svm-predict.exe";
            string testArgs = String.Format("{0} {1} {2}", testingFile, sampledModelFile, sampledResultsFile);
            Process.Start(svmTestExe, testArgs);

            Thread.Sleep(1000);

            // Read in the predicted data
            List<int> predictedData = new List<int>();
            using (StreamReader sr = File.OpenText(sampledResultsFile))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    if (!String.IsNullOrWhiteSpace(s))
                    {
                        predictedData.Add(Int32.Parse(s));
                    }
                }
            }

            return predictedData;
        }
    }
}
