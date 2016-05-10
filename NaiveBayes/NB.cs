using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaiveBayes
{
    public class NB
    {
        private const char Delimiter = ' ';

        private List<SpamRecord> TrainingData;

        public void Train(string trainingSetPath)
        {
            Trace.TraceInformation("Loading training data from {0}", trainingSetPath);

            this.TrainingData = new List<SpamRecord>();

            using (StreamReader sr = File.OpenText(trainingSetPath))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    SpamRecord spamRecord = new SpamRecord();

                    string[] sParts = s.Split(NB.Delimiter);
                    spamRecord.EmailId = sParts[0];
                    spamRecord.IsSpam = sParts[1].Equals("spam", StringComparison.InvariantCultureIgnoreCase) ? true : false;

                    this.TrainingData.Add(spamRecord);
                }
            }

            Trace.TraceInformation("Done loading {0} items for training data", this.TrainingData.Count());
        }

        public void Test(string testingSetPath)
        {

        }

    }
}
