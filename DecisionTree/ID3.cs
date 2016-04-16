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

            double gain = this.CalculateGain(examples, 3, this.TargetAttributeIndex);
        }

        public void TrainRecursive(Instances examples, int targetAttribute, List<int> attributes)
        {

        }

        private double CalculateGain(Instances S, int attributeIndex, int targetAttributeIndex)
        {
            // This will store Sv (per Mitchell Eq. 3.4) for each v in Values(A)
            Dictionary<int, Instances> SvList = new Dictionary<int,Instances>();

            // Initialize SvList
            for (int i=0; i< S.attribute(attributeIndex).numValues(); i++)
            {
                SvList.Add(i, new Instances(S, 0, 0));
            }

            // Iterate through each possible value of the attribute we're examining and populate SvList
            int countOfS = S.numInstances();
            for (int i = 0; i < countOfS; i++)
            {
                if (Double.IsNaN(S.instance(i).value(attributeIndex)))
                {
                    // TODO: Need to decide on how to handle this
                    continue;
                }

                int value = (int)S.instance(i).value(attributeIndex);
                SvList[value].add(S.instance(i));
            }

            // Sum the expected entropy of the set Sv for each value of v (per Mitchell Eq. 3.4)
            double expectedEntropy = 0;
            for (int i = 0; i < S.attribute(attributeIndex).numValues(); i++)
            {
                double countOfSv = SvList[i].numInstances();
                double entropyOfSv = this.CalculateEntropy(SvList[i], targetAttributeIndex);

                expectedEntropy += (countOfSv / countOfS) * entropyOfSv;
            }

            // Now we have all the info we need to calculate gain for this attribute
            double entropyS = this.CalculateEntropy(S, targetAttributeIndex);
            double gain = entropyS - expectedEntropy;

            return gain;
        }

        private double CalculateEntropy(Instances S, int targetAttributeIndex)
        {
            // Number of discrete values that the target attribute can have
            int numTargetValues = S.attribute(targetAttributeIndex).numValues();

            // For each possible discrete value that the target attribute can have, count how many times it is present in the examples
            // For boolean target attributes it is just true/false
            int[] targetValueCounts = new int[numTargetValues];

            // Iterate through the examples to count the proportion of examples for each value of the target attribute
            int countOfS = S.numInstances();
            for (int i = 0; i < countOfS; i++)
            {
                if (Double.IsNaN(S.instance(i).value(targetAttributeIndex)))
                {
                    // This shouldn't happen (?)
                    throw new Exception(String.Format("Value at targetAttributeIndex {0} is NaN", targetAttributeIndex));
                }

                int value = (int)S.instance(i).value(targetAttributeIndex);
                targetValueCounts[value]++;
            }

            double entropy = 0;

            for (int i = 0; i < numTargetValues; i++)
            {
                double proportion = targetValueCounts[i] / (double)countOfS;

                if (proportion != 0) // Skip 0 to avoid log(0)
                {
                    entropy += (-1 * proportion * Math.Log(proportion, 2));
                }
            }
            
            return entropy;
        }
    }
}
