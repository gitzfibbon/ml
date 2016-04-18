using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using weka.core;
using MathNet.Numerics;

namespace DecisionTree
{
    public class ID3
    {
        //private int TargetAttributeIndex;
        private ID3Node RootNode;

        public void Test(string arffFilePath, ID3Node root)
        {
            // Load the examples into S
            Instances S = new weka.core.Instances(new java.io.FileReader(arffFilePath));
            int targetAttributeIndex = S.numAttributes() - 1;

            // Evaluate each example
            int truePositive = 0;
            int falseNegative = 0;
            int falsePositive = 0;
            int trueNegative = 0;
            int actualPositive = 0;
            int actualNegative = 0;
            int predictedPositive = 0;
            int predictedNegative = 0;

            for (int i = 0; i < S.numInstances(); i++)
            {
                // Compare predicted value to actual value
                int predictedValue = this.Predict(root, S.instance(i));
                int actualValue = (int)S.instance(i).value(targetAttributeIndex);

                // Classify it as TP, TN, FP, FN
                if (actualValue == 0)
                {
                    // Actual value is true
                    actualPositive++;

                    if (predictedValue == 0)
                    {
                        predictedPositive++;
                        truePositive++;
                    }
                    else if (predictedValue == 1)
                    {
                        predictedNegative++;
                        falseNegative++;
                    }
                    else
                    {
                        throw new Exception(String.Format("Unexpected predicted value of {0}", predictedValue));
                    }

                }
                else if (actualValue == 1)
                {
                    // Actual value is false
                    actualNegative++;

                    if (predictedValue == 0)
                    {
                        predictedPositive++;
                        falsePositive++;
                    }
                    else if (predictedValue == 1)
                    {
                        predictedNegative++;
                        trueNegative++;
                    }
                    else
                    {
                        throw new Exception(String.Format("Unexpected predicted value of {0}", predictedValue));
                    }
                }
                else
                {
                    throw new Exception(String.Format("Unexpected actual value of {0}", actualValue));
                }
            }

            Console.WriteLine("truePositive: {0}", truePositive);
            Console.WriteLine("falseNegative: {0}", falseNegative);
            Console.WriteLine("falsePositive: {0}", falsePositive);
            Console.WriteLine("trueNegative: {0}", trueNegative);
            Console.WriteLine("actualPositive: {0}", actualPositive);
            Console.WriteLine("actualNegative: {0}", actualNegative);
            Console.WriteLine("predictedPositive: {0}", predictedPositive);
            Console.WriteLine("predictedNegative: {0}", predictedNegative);

            double precision = truePositive / (double)(truePositive + falsePositive);
            double recall = truePositive / (double)(truePositive + falseNegative);
            double accuracy = (truePositive + trueNegative) / (double)(S.numInstances());

            Console.WriteLine("precision: {0}", precision);
            Console.WriteLine("recall: {0}", recall);
            Console.WriteLine("accuracy: {0}", accuracy);

        }

        private int Predict(ID3Node node, Instance example)
        {
            // If the node is a leaf, return the value
            if (node.IsLeaf)
            {
                return node.AttributeValue;
            }

            // Else, figure out which path to take
            int attributeValue = (int)example.value(node.SplitAttributeIndex);
            ID3Node nextNode = node.ChildNodes[attributeValue];

            return this.Predict(nextNode, example);
        }

        public ID3Node Train(string arffFilePath, bool printTree = false)
        {
            // Load the examples into S
            Instances S = new weka.core.Instances(new java.io.FileReader(arffFilePath));
            int targetAttributeIndex = S.numAttributes() - 1;

            // Store the attribute indexes in a list. They will get removed as we split on attributes.
            List<int> attributeIndexes = new List<int>();
            for (int i = 0; i < S.numAttributes() - 1; i++)
            {
                attributeIndexes.Add(i);
            }

            this.RootNode = new ID3Node();

            this.TrainRecursive(this.RootNode, S, targetAttributeIndex, attributeIndexes);

            if (printTree == true)
            {
                ID3Node.BFS(this.RootNode, S);
            }

            return this.RootNode;
        }

        public void TrainRecursive(ID3Node root, Instances S, int targetAttributeIndex, List<int> attributeIndexes)
        {
            if (S.numInstances() == 0)
            {
                return;
            }

            // For each possible discrete value that the target attribute can have, count how many times it is present in the examples
            Dictionary<int, Instances> targetValueCounts = new Dictionary<int, Instances>();
            for (int i = 0; i < S.attribute(targetAttributeIndex).numValues(); i++)
            {
                targetValueCounts.Add(i, new Instances(S, 0, 0));
            }

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
                targetValueCounts[value].add(S.instance(i));

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

            // Find the most common target attribute value
            int mostCommonTargetValueIndex = 0;
            for (int i = 1; i < targetValueCounts.Count(); i++)
            {
                if (targetValueCounts[i].numInstances() > targetValueCounts[mostCommonTargetValueIndex].numInstances())
                {
                    mostCommonTargetValueIndex = i;
                }
            }

            // Check if the attribute list is empty and return most common target value if so
            if (attributeIndexes.Count == 0)
            {
                // Now set the node to this target value and return
                root.IsLeaf = true;
                root.AttributeValue = mostCommonTargetValueIndex;
                return;
            }

            // Figure out which attribute will give us the most gain
            int maxGainAttributeIndex = 0;
            double maxGain = 0;
            for (int i = 0; i < attributeIndexes.Count(); i++)
            {
                double gain = this.CalculateGain(S, i, targetAttributeIndex);

                if (gain > maxGain)
                {
                    maxGainAttributeIndex = i;
                    maxGain = gain;
                }
            }

            // Now we know which attribute to split on
            int maxGainAttribute = attributeIndexes[maxGainAttributeIndex];
            root.SplitAttributeIndex = maxGainAttribute;
            List<int> newAttributeIndexes = new List<int>(attributeIndexes);
            newAttributeIndexes.RemoveAt(maxGainAttributeIndex);


            Dictionary<int, Instances> examplesVi = new Dictionary<int, Instances>();
            // Initialize the examplesVi dictionary
            for (int i = 0; i < S.attribute(maxGainAttribute).numValues(); i++)
            {
                examplesVi.Add(i, new Instances(S, 0, 0));
            }

            // Fill the examplesVi dictionary
            for (int i = 0; i < S.numInstances(); i++)
            {
                if (Double.IsNaN(S.instance(i).value(maxGainAttribute)))
                {
                    // TODO: This will happen. How to handle it?
                    continue;
                    //throw new Exception(String.Format("Value at targetAttributeIndex {0} is NaN", targetAttributeIndex));

                }

                int value = (int)S.instance(i).value(maxGainAttribute);
                examplesVi[value].add(S.instance(i));
            }

            for (int i = 0; i < S.attribute(maxGainAttribute).numValues(); i++)
            {
                ID3Node newChild = new ID3Node();
                root.ChildNodes.Add(newChild);

                if (examplesVi[i].numInstances() == 0)
                {
                    newChild.IsLeaf = true;
                    newChild.SplitAttributeIndex = i;
                    newChild.AttributeValue = mostCommonTargetValueIndex;
                }
                else
                {
                    newChild.IsLeaf = false;
                    newChild.SplitAttributeIndex = i;
                    this.TrainRecursive(newChild, examplesVi[i], targetAttributeIndex, newAttributeIndexes);
                }

            }

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
