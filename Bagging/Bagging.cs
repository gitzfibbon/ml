using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DecisionTree;
using weka.core;

namespace Bagging
{
    public class Bagging
    {
        private List<ID3> Models;

        public Bagging()
        {
            // Turn off all the logging from ID3
            DecisionTree.Log.GainOn = false;
            DecisionTree.Log.InfoOn = false;
            DecisionTree.Log.NodeOn = true;
            DecisionTree.Log.StatsOn = true;
            DecisionTree.Log.VerboseOn = false;
        }

        public void Train(Instances instances, int numberOfModels, double confidenceLevel = 0.0)
        {
            this.Models = new List<ID3>();

            // Do sampling with replacement and then train the model
            Random random = new Random(0);
            for (int i = 0; i < numberOfModels; i++)
            {
                Instances newInstances = new Instances(instances, 0);

                for (int j = 0; j < instances.numInstances(); j++)
                {
                    int randomSample = random.Next(0, instances.numInstances() - 1);
                    newInstances.add(instances.instance(randomSample));
                }

                ID3 model = new ID3();
                model.Train(newInstances, confidenceLevel);
                this.Models.Add(model);
            }
        }

        public void Test(Instances instances)
        {
            int targetAttributeIndex = instances.numAttributes() - 1;

            for (int i = 0; i < instances.numInstances(); i++)
            {
                // Create a dictionary to keep track of votes

                foreach(ID3 model in this.Models)
                {
                    int prediction = model.Predict(model.RootNode, instances.instance(i));
                }

                int actual = (int)instances.instance(i).value(targetAttributeIndex);
            }
        }
    }
}
