using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaiveBayes
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            Trace.TraceInformation("");

            string trainingSetPath = ConfigurationManager.AppSettings["TrainingSetPath"];
            string testingSetPath = ConfigurationManager.AppSettings["TestingSetPath"];
            string laplaceSmoothing = ConfigurationManager.AppSettings["LaplaceSmoothing"];
            
            if (String.IsNullOrWhiteSpace(trainingSetPath))
            {
                trainingSetPath = "TrainingRatings.txt";
                Trace.TraceInformation("TrainingSetPath in config file is not set. Default to {0} in current directory.", trainingSetPath);
            }

            if (String.IsNullOrWhiteSpace(testingSetPath))
            {
                testingSetPath = "TestingRatings.txt";
                Trace.TraceInformation("TestingSetPath in config file is not set. Default to {0} in current directory.", testingSetPath);
            }

            if (String.IsNullOrWhiteSpace(laplaceSmoothing))
            {
                laplaceSmoothing = "1.0";
                Trace.TraceInformation("LaplaceSmoothing in config file is not set. Default to {0}.", laplaceSmoothing);
            }


            NB nb = new NB();
            nb.Train(trainingSetPath, Double.Parse(laplaceSmoothing));
            nb.Test(testingSetPath);

            Trace.TraceInformation("");

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Done. Press the anykey to continue.");
                Console.Read();
            }
        }
    }
}
