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
        public static void Run(string trainingSetPath, string testingSetPath, int numberOfModels, int? randomSeed)
        {
            Trace.TraceInformation("Starting Bias-Variance Program");
            Trace.TraceInformation("TrainingSetPath: {0}", trainingSetPath);
            Trace.TraceInformation("TestingSetPath: {0}", testingSetPath);
            Trace.TraceInformation("Models: {0}", numberOfModels);
            Trace.TraceInformation("Random Seed: {0}", randomSeed);

            Instances trainingInstances = Diabetes.LoadData(trainingSetPath, Mode.Train);
            Bagging.Bagging bagging = new Bagging.Bagging();
            bagging.Train(trainingInstances, numberOfModels, randomSeed);
            Instances testingInstances = Diabetes.LoadData(testingSetPath, Mode.Test);
            bagging.TestNonBagging(testingInstances);
            bagging.Test(testingInstances);
        }
    }
}
