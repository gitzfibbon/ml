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
        public Bagging()
        {
            // Turn off all the logging from ID3
            DecisionTree.Log.GainOn = false;
            DecisionTree.Log.InfoOn = false;
            DecisionTree.Log.NodeOn = false;
            DecisionTree.Log.StatsOn = false;
            DecisionTree.Log.VerboseOn = false;
        }

        public List<ID3> Train(Instances S, int numberOfModels, double confidenceLevel = 0.95)
        {
            List<ID3> models = new List<ID3>();

            // Train the ensemble
            for (int i = 0; i < numberOfModels; i++)
            {
                Instances newInstances = new Instances(S, 0);

                Random random = new Random(0);
                for (int j = 0; j < S.numInstances(); j++)
                {
                    int randomSample = random.Next(0, S.numInstances() - 1);
                    newInstances.add(S.instance(randomSample));
                }

                ID3 model = new ID3();
                model.Train(newInstances, confidenceLevel);
                models.Add(model);
            }

            return models;
        }

        public void Test(Instances S, List<ID3> models, double confidenceLevel = 0.95)
        {
        }
    }
}
