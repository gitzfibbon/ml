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

            // Get the class for each test example
            int targetAttribute = testingInstances.numAttributes() - 1;
            List<int> classes = new List<int>();
            for (int i = 0; i < testingInstances.numInstances(); i++)
            {
                classes.Add((int)testingInstances.instance(i).value(targetAttribute));
            }

            // Get the prediction for every test example, for every set of instanes
            List<int> predictions = bagging.TestNonBagging(testingInstances);
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

            double loss = 0;
            double bias = 0;
            double var = 0;
            double varp = 0;
            double varn = 0;
            double varc = 0;

            BiasVariance.biasvar(classes, allPredictions, testingInstances.numInstances(), 1, ref loss, ref bias, ref var, ref varp, ref varn, ref varc);
        }

        public static void RunBagging(string trainingSetPath, string testingSetPath, int numberOfModels, int? randomSeed, int maxTreeDepth)
        {
            Trace.TraceInformation("Starting Bias-Variance for Bagging");
            Trace.TraceInformation("TrainingSetPath: {0}", trainingSetPath);
            Trace.TraceInformation("TestingSetPath: {0}", testingSetPath);
            Trace.TraceInformation("Models: {0}", numberOfModels);
            Trace.TraceInformation("Random Seed: {0}", randomSeed.ToString());
            Trace.TraceInformation("Max Tree Depth: {0}", maxTreeDepth);

            Instances trainingInstances = Diabetes.LoadData(trainingSetPath, Mode.Train);
            Bagging.Bagging bagging = new Bagging.Bagging();
            bagging.Train(trainingInstances, numberOfModels, randomSeed, maxTreeDepth);
            Instances testingInstances = Diabetes.LoadData(testingSetPath, Mode.Test);
            bagging.TestNonBagging(testingInstances);
            //bagging.Test(testingInstances);
        }
    }
}
