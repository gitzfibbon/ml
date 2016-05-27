using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bagging;
using weka.core;

namespace BV
{
    // Bias-Variance
    public class BV
    {
        public static void RunNonBagging(string trainingSetPath, string testingSetPath, int maxTreeDepth)
        {
            Trace.TraceInformation("Starting Bias-Variance for NonBagging");
            Trace.TraceInformation("TrainingSetPath: {0}", trainingSetPath);
            Trace.TraceInformation("TestingSetPath: {0}", testingSetPath);
            Trace.TraceInformation("Max Tree Depth: {0}", maxTreeDepth);

            // Train
            Instances trainingInstances = Diabetes.LoadData(trainingSetPath, Mode.Train);
            Bagging.Bagging bagging = new Bagging.Bagging();
            bagging.Train(trainingInstances, 0, null, maxTreeDepth);

            // Predict
            Instances testingInstances = Diabetes.LoadData(testingSetPath, Mode.Test);
            List<int> predictions = bagging.TestNonBagging(testingInstances);

            // Calculate Bias and Variance

            // Get the class for each test example
            int targetAttribute = testingInstances.numAttributes() - 1;
            List<int> classes = new List<int>();
            for (int i = 0; i < testingInstances.numInstances(); i++)
            {
                classes.Add((int)testingInstances.instance(i).value(targetAttribute));
            }

            // Get the prediction for every test example, for every set of instanes
            List<List<int>> allPredictions = new List<List<int>>();
            for (int j = 0; j < 1; j++)
            {
                for (int i = 0; i < testingInstances.numInstances(); i++)
                {
                    if (j == 0)
                    {
                        allPredictions.Add(new List<int>());
                    }

                    allPredictions[i].Add(predictions[i]);
                }
            }


            BiasVariance.biasvar(classes, allPredictions, testingInstances.numInstances(), 1);

        }

        public static void RunBagging(string trainingSetPath, string testingSetPath, int numberOfModels, int bootstrapSamples, int? randomSeed, int maxTreeDepth)
        {
            Trace.TraceInformation("Starting Bias-Variance for Bagging");
            Trace.TraceInformation("TrainingSetPath: {0}", trainingSetPath);
            Trace.TraceInformation("TestingSetPath: {0}", testingSetPath);
            Trace.TraceInformation("Models: {0}", numberOfModels);
            Trace.TraceInformation("Bootstrap Samples: {0}", bootstrapSamples);
            Trace.TraceInformation("Random Seed: {0}", randomSeed.ToString());
            Trace.TraceInformation("Max Tree Depth: {0}", maxTreeDepth);

            Instances trainingInstances = Diabetes.LoadData(trainingSetPath, Mode.Train);
            Instances testingInstances = Diabetes.LoadData(testingSetPath, Mode.Test);

            // Stores the predictions for every training set
            List<List<int>> allPredictions = new List<List<int>>();

            for (int i = 0; i < bootstrapSamples; i++)
            {
                Bagging.Bagging bagging = new Bagging.Bagging();
                bagging.Train(trainingInstances, numberOfModels, randomSeed, maxTreeDepth);
                List<int> predictions = bagging.Test(testingInstances);

                for (int k = 0; k < testingInstances.numInstances(); k++)
                {
                    if (i == 0)
                    {
                        allPredictions.Add(new List<int>());
                    }

                    allPredictions[k].Add(predictions[k]);
                }

            }

            // Calculate Bias and Variance

            // Get the class for each test example
            int targetAttribute = testingInstances.numAttributes() - 1;
            List<int> classes = new List<int>();
            for (int i = 0; i < testingInstances.numInstances(); i++)
            {
                classes.Add((int)testingInstances.instance(i).value(targetAttribute));
            }

            BiasVariance.biasvar(classes, allPredictions, testingInstances.numInstances(), bootstrapSamples);
        }
    }
}
