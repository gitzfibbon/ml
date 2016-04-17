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

        public ID3Node Train(string arffFilePath)
        {
            // Load the examples into S
            Instances S = new weka.core.Instances(new java.io.FileReader(arffFilePath));
            this.TargetAttributeIndex = S.numAttributes() - 1;

            // Store the attribute indexes in a list. They will get removed as we split on attributes.
            List<int> attributeIndexes = new List<int>();
            for (int i = 0; i < S.numAttributes() - 1; i++)
            {
                attributeIndexes.Add(i);
            }

            ID3Node root = new ID3Node();

            this.TrainRecursive(root, S, this.TargetAttributeIndex, attributeIndexes);

            return root;
        }

        public void TrainRecursive(ID3Node root, Instances S, int targetAttributeIndex, List<int> attributeIndexes)
        {
            if (S.numInstances() == 0)
            {
                return;
            }

            // For each possible discrete value that the target attribute can have, count how many times it is present in the examples
            int[] targetValueCounts = new int[S.attribute(targetAttributeIndex).numValues()];

            // Check the most common target attribute value of every example in S
            // Also keep track of whether all values are the same
            int countOfS = S.numInstances();
            int firstTargetValue = (int)S.instance(0).value(targetAttributeIndex);
            bool allTargetValuesAreEqual = true;
            for (int i = 0; i < countOfS; i++)
            {
                if (Double.IsNaN(S.instance(i).value(targetAttributeIndex)))
                {
                    // This shouldn't happen (?)
                    throw new Exception(String.Format("Value at targetAttributeIndex {0} is NaN", targetAttributeIndex));
                }

                int value = (int)S.instance(i).value(targetAttributeIndex);
                targetValueCounts[value]++;

                if (firstTargetValue != value)
                {
                    allTargetValuesAreEqual = false;
                }
            }

            // Check if all target values are the same in which case we make this a leaf node
            if (allTargetValuesAreEqual == true)
            {
                root.IsLeaf = true;
                root.AttributeValue = firstTargetValue;
                return;
            }

            // Check if the attribute list is empty and return most common target value if so
            if (attributeIndexes.Count == 0)
            {
                // Find the most common target attribute value
                int mostCommonTargetValueIndex = 0;
                for (int i=1; i < targetValueCounts.Count(); i++)
                {
                    if (targetValueCounts[i] > targetValueCounts[mostCommonTargetValueIndex])
                    {
                        mostCommonTargetValueIndex = i;
                    }
                }

                // Now set the node to this target value and return
                root.IsLeaf = true;
                root.AttributeValue = mostCommonTargetValueIndex;
                return;
            }

            double gain = this.CalculateGain(S, 3, this.TargetAttributeIndex);

        }

        private double CalculateGain(Instances S, int attributeIndex, int targetAttributeIndex)
        {
            // This will store Sv (per Mitchell Eq. 3.4) for each v in Values(A)
            Dictionary<int, Instances> SvList = new Dictionary<int, Instances>();

            // Initialize SvList
            for (int i = 0; i < S.attribute(attributeIndex).numValues(); i++)
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
