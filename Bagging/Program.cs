﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bagging
{
    public class Program
    {
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            Trace.TraceInformation("");

            string trainingSetPathConfig = ConfigurationManager.AppSettings["TrainingSetPath"];
            string testingSetPathConfig = ConfigurationManager.AppSettings["TestingSetPath"];
            string numberOfModelsConfig = ConfigurationManager.AppSettings["NumberOfModels"];
            string randomSeedConfig = ConfigurationManager.AppSettings["RandomSeed"];
            int? randomSeed = null;

            if (String.IsNullOrWhiteSpace(trainingSetPathConfig))
            {
                trainingSetPathConfig = "TrainingRatings.txt";
                Trace.TraceInformation("TrainingSetPath in config file is not set. Default to {0} in current directory.", trainingSetPathConfig);
            }

            if (String.IsNullOrWhiteSpace(testingSetPathConfig))
            {
                testingSetPathConfig = "TestingRatings.txt";
                Trace.TraceInformation("TestingSetPath in config file is not set. Default to {0} in current directory.", testingSetPathConfig);
            }

            if (String.IsNullOrWhiteSpace(numberOfModelsConfig))
            {
                numberOfModelsConfig = "1";
                Trace.TraceInformation("NumberOfModels in config file is not set. Default to {0}.", numberOfModelsConfig);
            }

            if (String.IsNullOrWhiteSpace(randomSeedConfig))
            {
                randomSeed = null;
                Trace.TraceInformation("RandomSeed in config file is not set. Default to null.");
            }
            else
            {
                randomSeed = Int32.Parse(randomSeedConfig);
            }

            Diabetes.Run(trainingSetPathConfig, testingSetPathConfig, Int32.Parse(numberOfModelsConfig), randomSeed);

            Trace.TraceInformation("");

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Done. Press enter to continue.");
                Console.Read();
            }
        }
    }
}
