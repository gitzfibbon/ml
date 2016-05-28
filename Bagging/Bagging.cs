using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DecisionTree;
using weka.core;

namespace Bagging
{
    public class Bagging
    {
        private ID3 NonBaggingModel;
        private List<ID3> Models;
        private const double ConfidenceLevel = 0;

        public Bagging()
        {
            // Turn off all the logging from ID3
            DecisionTree.Log.GainOn = false;
            DecisionTree.Log.InfoOn = false;
            DecisionTree.Log.NodeOn = false;
            DecisionTree.Log.StatsOn = false;
            DecisionTree.Log.VerboseOn = false;
        }

        public void Train(Instances instances, int numberOfModels, int? randomSeed = null, int maxDepth = 0)
        {
            // First train and test without bagging
            this.NonBaggingModel = new ID3();
            this.NonBaggingModel.Train(instances, Bagging.ConfidenceLevel, maxDepth);

            this.Models = new List<ID3>();

            // Do sampling with replacement and then train the model
            Random random = randomSeed == null ? new Random() : new Random((int)randomSeed);
            for (int i = 0; i < numberOfModels; i++)
            {
                Instances newInstances = new Instances(instances, 0);

                for (int j = 0; j < instances.numInstances(); j++)
                {
                    int randomSample = random.Next(0, instances.numInstances() - 1);
                    newInstances.add(instances.instance(randomSample));
                }

                ID3 model = new ID3();
                model.Train(newInstances, Bagging.ConfidenceLevel, maxDepth);
                this.Models.Add(model);
            }
        }

        public List<int> TestNonBagging(Instances instances)
        {
            // List of predictions for each test instance
            List<int> predictions = new List<int>();

            int targetAttributeIndex = instances.numAttributes() - 1;
            int correct = 0;
            int incorrect = 0;

            for (int i = 0; i < instances.numInstances(); i++)
            {
                int prediction = this.NonBaggingModel.Predict(this.NonBaggingModel.RootNode, instances.instance(i));
                predictions.Add(prediction);
                int actual = (int)instances.instance(i).value(targetAttributeIndex);

                if (prediction == actual) { correct++; }
                else { incorrect++; }
            }

            Trace.TraceInformation("");
            Trace.TraceInformation("Results for non-bagging");
            Trace.TraceInformation("Correct: {0}", correct);
            Trace.TraceInformation("Incorrect: {0}", incorrect);
            Trace.TraceInformation("Accuracy: {0}", correct / (double)(correct + incorrect));


            return predictions;
        }

        public List<int> Test(Instances instances)
        {
            List<int> predictions = new List<int>();

            int targetAttributeIndex = instances.numAttributes() - 1;
            int correct = 0;
            int incorrect = 0;

            for (int i = 0; i < instances.numInstances(); i++)
            {
                // Create a dictionary to keep track of votes
                Dictionary<int, int> votes = new Dictionary<int, int>();

                foreach (ID3 model in this.Models)
                {
                    int vote = model.Predict(model.RootNode, instances.instance(i));

                    if (votes.ContainsKey(vote)) { votes[vote]++; }
                    else { votes.Add(vote, 1); }
                }

                int highestVote = votes.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                predictions.Add(highestVote);
                int actual = (int)instances.instance(i).value(targetAttributeIndex);

                if (highestVote == actual) { correct++; }
                else { incorrect++; }
            }

            Trace.TraceInformation("");
            Trace.TraceInformation("Results for bagging");
            Trace.TraceInformation("Correct: {0}", correct);
            Trace.TraceInformation("Incorrect: {0}", incorrect);
            Trace.TraceInformation("Accuracy: {0}", correct / (double)(correct + incorrect));

            return predictions;
        }
    }
}
