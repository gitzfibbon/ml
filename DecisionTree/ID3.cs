using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using weka.core;

namespace DecisionTree
{
    public class ID3
    {
        private int ClassAttributeIndex;

        public ID3Node root;

        public void Train(string arffFilePath)
        {
            this.root = new ID3Node();

            Instances examples = new weka.core.Instances(new java.io.FileReader(arffFilePath));
            this.ClassAttributeIndex = examples.numAttributes() - 1;

            double entropy = this.CalculateEntropy(examples, 273);
        }

        public void TrainRecursive(Instances examples, int targetAttribute, List<int> attributes)
        {

        }

        private double CalculateEntropy(Instances examples, int attributeIndex)
        {
            // Number of discrete values that this attribute can have
            int numValues = examples.attribute(attributeIndex).numValues();
            int numExamples = examples.numInstances();
            int[] proportions = new int[numValues];

            // Iterate through the examples to count the proportion of examples belonging to each attribute value
            for (int i = 0; i < numExamples; i++)
            {
                int? value = Double.IsNaN(examples.instance(i).value(attributeIndex)) ? null : (int?)(examples.instance(i).value(attributeIndex));

                if (value != null)
                {
                    proportions[(int)value]++;
                }
            }

            double entropy = 0;

            for (int i = 0; i < numValues; i++)
            {
                double proportion = proportions[i] / (double)numExamples;

                if (proportion != 0) // Skip 0 to avoid log(0)
                {
                    entropy += (-1 * proportion * Math.Log(proportion, 2));
                }
            }
            
            return entropy;
        }
    }
}
