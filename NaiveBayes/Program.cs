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

            string trainingSetPathConfig = ConfigurationManager.AppSettings["TrainingSetPath"];
            string testingSetPathConfig = ConfigurationManager.AppSettings["TestingSetPath"];
            string laplaceSmoothingConfig = ConfigurationManager.AppSettings["LaplaceSmoothing"];
            string useExtraFeaturesConfig = ConfigurationManager.AppSettings["UseExtraFeatures"];
            bool useExtraFeatures = false;
            
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

            if (String.IsNullOrWhiteSpace(laplaceSmoothingConfig))
            {
                laplaceSmoothingConfig = "1.0";
                Trace.TraceInformation("LaplaceSmoothing in config file is not set. Default to {0}.", laplaceSmoothingConfig);
            }

            if (String.IsNullOrWhiteSpace(useExtraFeaturesConfig))
            {
                useExtraFeatures = false;
                Trace.TraceInformation("UseExtraFeatures in config file is not set. Default to {0}.", useExtraFeatures);
            }
            else
            {
                useExtraFeatures = Boolean.Parse(useExtraFeaturesConfig);
            }

            NB nb = new NB(useExtraFeatures);
            nb.Train(trainingSetPathConfig, Double.Parse(laplaceSmoothingConfig));
            nb.Test(testingSetPathConfig);

            Trace.TraceInformation("");

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Done. Press the anykey to continue.");
                Console.Read();
            }
        }
    }
}
