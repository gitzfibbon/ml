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
        private int TargetAttributeIndex;

        public ID3Node root;

        public void Train(string arffFilePath)
        {
            this.root = new ID3Node();

            Instances examples = new weka.core.Instances(new java.io.FileReader(arffFilePath));
            this.TargetAttributeIndex = examples.numAttributes() - 1;

            double entropy = this.CalculateEntropy(examples, this.TargetAttributeIndex);
        }

        public void TrainRecursive(Instances examples, int targetAttribute, List<int> attributes)
        {

        }

        private double CalculateEntropy(Instances examples, int targetAttributeIndex)
        {
            // Number of discrete values that the target attribute can have
            int numTargetValues = examples.attribute(targetAttributeIndex).numValues();

            int numExamples = examples.numInstances();
            
            // For each possible discrete value that the target attribute can have, count how many times it is present in the examples
            // For boolean target attributes it is just true/false
            int[] targetValueCounts = new int[numTargetValues];

            // Iterate through the examples to count the proportion of examples for each value of the target attribute
            for (int i = 0; i < numExamples; i++)
            {
                if (Double.IsNaN(examples.instance(i).value(targetAttributeIndex)))
                {
                    // This shouldn't happen (?)
                    throw new Exception("Value at targetAttributeIndex is NaN");
                }

                int value = (int)examples.instance(i).value(targetAttributeIndex);
                targetValueCounts[value]++;
            }

            double entropy = 0;

            for (int i = 0; i < numTargetValues; i++)
            {
                double proportion = targetValueCounts[i] / (double)numExamples;

                if (proportion != 0) // Skip 0 to avoid log(0)
                {
                    entropy += (-1 * proportion * Math.Log(proportion, 2));
                }
            }
            
            return entropy;
        }
    }
}
