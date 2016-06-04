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
            Trace.Listeners.Add(new ConsoleTraceListener());

            string trainingFile = @"C:\coding\ml\data\diabetes\diabetes_libsvmformat_train.txt";
            string testingFile = @"C:\coding\ml\data\diabetes\diabetes_libsvmformat_test.txt";
            int bootstrapSamples = 10;
            int kernel = 0;
            int? randomSeed = null;

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

            // Read in the testing data
            List<int> testingTargetValues = new List<int>();
            using (StreamReader sr = File.OpenText(testingFile))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    if (!String.IsNullOrWhiteSpace(s))
                    {
                        int value = Int32.Parse(s.Split(' ')[0]);
                        // Map -1 to 0
                        testingTargetValues.Add(value == -1 ? 0 : value);
                    }
                }
            }

            // Stores the predictions for every sampled training set
            List<List<int>> allPredictions = new List<List<int>>();

            for (int i = 0; i < bootstrapSamples; i++)
            {
                List<int> predictions = Program.RunSvm(trainingData, testingFile, kernel, randomSeed);

                for (int k = 0; k < testingTargetValues.Count; k++)
                {
                    if (i == 0)
                    {
                        allPredictions.Add(new List<int>());
                    }

                    // Map -1 to 0
                    allPredictions[k].Add(predictions[k] == -1 ? 0 : predictions[k]);
                }
            }


            // Calculate bias and variance
            BiasVariance.biasvar(testingTargetValues, allPredictions, testingTargetValues.Count, bootstrapSamples);

            Trace.TraceInformation("");

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Done. Press enter to continue.");
                Console.Read();
            }
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

            //string svmTrainExe = @"C:\tools\libsvm-3.21\svm-train.exe";
            //string trainArgs = String.Format("-t {0} {1} {2}", kernel, sampledTrainingFile, sampledModelFile);
            //Process.Start(svmTrainExe, trainArgs);

            Process trainProcess = new Process();
            trainProcess.StartInfo.FileName = @"C:\tools\libsvm-3.21\svm-train.exe";
            trainProcess.StartInfo.Arguments = String.Format("-t {0} {1} {2}", kernel, sampledTrainingFile, sampledModelFile);
            trainProcess.StartInfo.UseShellExecute = false;
            //trainProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //trainProcess.StartInfo.CreateNoWindow = true;
            trainProcess.Start();
            trainProcess.WaitForExit();

            Thread.Sleep(1000);

            //string svmTestExe = @"C:\tools\libsvm-3.21\svm-predict.exe";
            //string testArgs = String.Format("{0} {1} {2}", testingFile, sampledModelFile, sampledResultsFile);
            //Process.Start(svmTestExe, testArgs);

            Process testProcess = new Process();
            testProcess.StartInfo.FileName = @"C:\tools\libsvm-3.21\svm-predict.exe";
            testProcess.StartInfo.Arguments = String.Format("{0} {1} {2}", testingFile, sampledModelFile, sampledResultsFile);
            testProcess.StartInfo.UseShellExecute = false;
            //testProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //testProcess.StartInfo.CreateNoWindow = true;
            testProcess.Start();
            testProcess.WaitForExit();

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
