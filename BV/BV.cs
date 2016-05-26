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
            bagging.TestNonBagging(testingInstances);

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
            bagging.Test(testingInstances);
        }
    }
}
