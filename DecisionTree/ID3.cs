using System;
using System.Collections;
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
        public static int PositiveTargetValue = 0;
        public static int NegativeTargetValue = 1;


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

            Log.LogStats("truePositive: {0}", truePositive);
            Log.LogStats("falseNegative: {0}", falseNegative);
            Log.LogStats("falsePositive: {0}", falsePositive);
            Log.LogStats("trueNegative: {0}", trueNegative);
            Log.LogStats("actualPositive: {0}", actualPositive);
            Log.LogStats("actualNegative: {0}", actualNegative);
            Log.LogStats("predictedPositive: {0}", predictedPositive);
            Log.LogStats("predictedNegative: {0}", predictedNegative);

            double precision = truePositive / (double)(truePositive + falsePositive);
            double recall = truePositive / (double)(truePositive + falseNegative);
            double accuracy = (truePositive + trueNegative) / (double)(S.numInstances());

            Log.LogStats("precision: {0}", precision);
            Log.LogStats("recall: {0}", recall);
            Log.LogStats("accuracy: {0}", accuracy);

        }

        private int Predict(ID3Node node, Instance example)
        {
            // If the node is a leaf, return the value
            if (node.IsLeaf)
            {
                return node.TargetValue;
            }

            // Else, figure out which path to take
            double attributeValue = example.value(node.SplitAttributeIndex);

            if (Double.IsNaN(attributeValue))
            {
                // TODO: use fractional test based on weights
                int highestWeightedAttribute = 0;
                for (int i = 0; i < node.ChildNodes.Count(); i++)
                {
                    if (node.ChildNodes[i].Weight > node.ChildNodes[highestWeightedAttribute].Weight)
                    {
                        highestWeightedAttribute = i;
                    }
                }

                attributeValue = highestWeightedAttribute;
            }

            ID3Node nextNode = node.ChildNodes[(int)attributeValue];

            return this.Predict(nextNode, example);
        }

        public ID3Node Train(string arffFilePath, double confidenceLevel)
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

            this.TrainRecursive(this.RootNode, S, targetAttributeIndex, attributeIndexes, confidenceLevel);

            if (Log.NodeOn == true)
            {
                ID3Node.BFS(this.RootNode, S);
            }

            Log.LogStats("Number of Nodes is {0}", ID3Node.NodeCount(this.RootNode));
            Log.LogStats("Max Tree Depth is {0}", ID3Node.MaxDepth(this.RootNode));

            return this.RootNode;
        }

        public void TrainRecursive(ID3Node root, Instances S, int targetAttributeIndex, List<int> attributeList, double confidenceLevel)
        {
            // For each possible discrete value that the target attribute can have, count how many times it is present in the examples
            Dictionary<int, Instances> targetValueCounts = new Dictionary<int, Instances>();
            for (int i = 0; i < S.attribute(targetAttributeIndex).numValues(); i++)
            {
                targetValueCounts.Add(i, new Instances(S, 0, 0));
            }

            // Check the most common target attribute value of every example in S
            // and keep track of whether all target values are the same value
            int countOfS = S.numInstances();
            int firstTargetValue = (int)S.instance(0).value(targetAttributeIndex);
            bool allTargetValuesAreEqual = true;
            for (int i = 0; i < countOfS; i++)
            {
                if (Double.IsNaN(S.instance(i).value(targetAttributeIndex)))
                {
                    // For target values, this shouldn't happen
                    throw new Exception(String.Format("Value at targetAttributeIndex {0} is NaN", targetAttributeIndex));
                }

                int value = (int)S.instance(i).value(targetAttributeIndex);
                targetValueCounts[value].add(S.instance(i));

                if (firstTargetValue != value)
                {
                    allTargetValuesAreEqual = false;
                }
            }

            // If all target values are the same we can make this a leaf with that value and return
            if (allTargetValuesAreEqual == true)
            {
                root.IsLeaf = true;
                root.TargetValue = firstTargetValue;
                Log.LogInfo("All Targets Equal. Node with split {0}, value {1}, leaf {2}, weight {3}", root.SplitAttributeIndex, root.TargetValue, root.IsLeaf, root.Weight);
                return;
            }

            // Find the most common target attribute value
            int mostCommonTargetValue = 0;
            for (int i = 0; i < targetValueCounts.Count(); i++)
            {
                if (targetValueCounts[i].numInstances() > targetValueCounts[mostCommonTargetValue].numInstances())
                {
                    mostCommonTargetValue = i;
                }
            }

            // Check if the attribute list is empty and if so return most common target value
            if (attributeList.Count == 0)
            {
                // Now set the node to this target value and return
                root.IsLeaf = true;
                root.TargetValue = mostCommonTargetValue;
                Log.LogInfo("Attribute List Empty. Node with split {0}, value {1}, leaf {2}, weight {3}", root.SplitAttributeIndex, root.TargetValue, root.IsLeaf, root.Weight);
                return;
            }

            // Figure out which attribute will give us the most gain
            double gainSum = 0;
            SortedList<double, int> sortedGainList = new SortedList<double, int>();
            for (int i = 0; i < attributeList.Count(); i++)
            {

                double gain = this.CalculateGain(S, i, targetAttributeIndex);
                gainSum += gain;

                // TODO: remove
                if (Double.IsNaN(gain))
                {
                }

                // We use a sorted list which must have a unique key. Since the key is gain, then this might not be unique
                // across all attributes. Thus, if we encounter duplicate keys figure out which on has higher gain ratio.
                // Whichever has higher gain ratio wins and gets into the list. Later, we pick from the list the attribute
                // with highest gain ratio anyways so we won't lose any information with this approach.
                if (sortedGainList.ContainsKey(gain))
                {
                    double oldGainRatio = this.CalculateGainRatio(S, sortedGainList[gain], targetAttributeIndex);
                    double newGainRatio = this.CalculateGainRatio(S, i, targetAttributeIndex);

                    if (newGainRatio > oldGainRatio)
                    {
                        // Replace the old value with the one that has higher gain ratio
                        sortedGainList[gain] = i;
                    }
                }
                else
                {
                    sortedGainList.Add(gain, i);
                }


            }

            double maxGain = sortedGainList.Last().Key;
            int maxGainAttribute = sortedGainList.Last().Value;
            double averageGain = gainSum / attributeList.Count();

            // Use gain ratio on top N% from the gainListOrdered and calculate maxGainRatio
            double maxGainRatio = 0;
            int maxGainRatioAttribute = sortedGainList.Count() - 1; // default to the largest gain
            double NPercent = 0.2;
            int topNPercent = (int)Math.Ceiling(NPercent * sortedGainList.Count());
            for (int i = 0; i < topNPercent; i++)
            {
                int reverse_i = sortedGainList.Count() - 1 - i; // Since we are search the list from bottom to top

                int index = sortedGainList.ElementAt(reverse_i).Value;
                double gainRatio = this.CalculateGainRatio(S, index, targetAttributeIndex);

                if (gainRatio > maxGainRatio)
                {
                    maxGainRatio = gainRatio;
                    maxGainRatioAttribute = index;
                }
            }

            // Now we know which attribute to split on
            Log.LogGain("MaxGainRatio {0} from attrib {1}. Max Gain {2} from attrib {3}. Avg Gain {4}.", maxGainRatio, maxGainRatioAttribute, maxGain, maxGainAttribute, averageGain);

            // Check if we should stop splitting
            if (ChiSquare.ChiSquaredTest(confidenceLevel, S, maxGainRatioAttribute, targetAttributeIndex) == false)
            {
                root.IsLeaf = true;
                root.TargetValue = mostCommonTargetValue;
                Log.LogInfo("ChiSquared stop split. Node with split {0}, value {1}, leaf {2}, weight {3}", root.SplitAttributeIndex, root.TargetValue, root.IsLeaf, root.Weight);
                return;
            }

            // We are going to split. Create a new list of attributes that won't include the attribute we split on.
            root.SplitAttributeIndex = maxGainRatioAttribute;
            List<int> newAttributeList = new List<int>(attributeList);
            newAttributeList.RemoveAt(maxGainRatioAttribute);

            // Partition the examples by their attribute value
            Dictionary<int, Instances> examplesVi = new Dictionary<int, Instances>();
            // Initialize the examplesVi dictionary
            for (int i = 0; i < S.attribute(maxGainRatioAttribute).numValues(); i++)
            {
                examplesVi.Add(i, new Instances(S, 0, 0));
            }

            // Fill the examplesVi dictionary
            int totalExamplesVi = 0;
            for (int i = 0; i < S.numInstances(); i++)
            {
                if (Double.IsNaN(S.instance(i).value(maxGainRatioAttribute)))
                {
                    Log.LogVerbose("IsNaN encountered for instance {0} of maxGainAttribute {1}", i, maxGainRatioAttribute);
                    continue;
                }

                int value = (int)S.instance(i).value(maxGainRatioAttribute);
                examplesVi[value].add(S.instance(i));
                totalExamplesVi++;
            }

            // Split
            for (int i = 0; i < S.attribute(maxGainRatioAttribute).numValues(); i++)
            {
                ID3Node newChild = new ID3Node();
                root.ChildNodes.Add(newChild);

                if (examplesVi[i].numInstances() == 0)
                {
                    newChild.IsLeaf = true;
                    newChild.TargetValue = mostCommonTargetValue;
                    Log.LogInfo("No instances to split on. Create new leaf child from parent split {0}, new value {1}", root.SplitAttributeIndex, newChild.TargetValue, root.IsLeaf, root.Weight);
                }
                else
                {
                    Log.LogInfo("Splitting from node with split {0}, value {1}, leaf {2}, weight {3}", root.SplitAttributeIndex, root.TargetValue, root.IsLeaf, root.Weight);

                    newChild.IsLeaf = false;
                    newChild.SplitAttributeIndex = i;
                    newChild.Weight = examplesVi[i].numInstances() / (double)totalExamplesVi;
                    this.TrainRecursive(newChild, examplesVi[i], targetAttributeIndex, newAttributeList, confidenceLevel);
                }

            }

        }

        private double CalculateGainRatio(Instances S, int attributeIndex, int targetAttributeIndex)
        {
            double gain = this.CalculateGain(S, attributeIndex, targetAttributeIndex);
            double splitInformation = this.CalculateSplitInformation(S, attributeIndex);

            return gain / splitInformation;
        }

        private double CalculateSplitInformation(Instances S, int attributeIndex)
        {
            int numAttributeValues = S.attribute(attributeIndex).numValues();
            int[] valueCounts = new int[numAttributeValues];
            int numInstances = 0;

            // Count how many attributes of each attribute value
            for (int i = 0; i < S.numInstances(); i++)
            {
                double value = S.instance(i).value(attributeIndex);

                if (Double.IsNaN(value))
                {
                    Log.LogVerbose("IsNaN encountered calculating split information for attribute {0}", attributeIndex);
                    continue;
                }

                numInstances++;
                valueCounts[(int)value]++;
            }

            // Calculate Split Information
            double splitInformation = 0;
            for (int i = 0; i < numAttributeValues; i++)
            {
                double temp = valueCounts[i] / (double)numInstances;
                splitInformation += -1 * temp * Math.Log(temp, 2);
            }

            return splitInformation;
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

            // These will be used to count positive and negative classes that have attribute ? for this attribute
            int pu = 0;
            int nu = 0;

            // Iterate through each possible value of the attribute we're examining and populate SvList
            int countOfS = S.numInstances();
            int droppedExamples = 0;
            for (int i = 0; i < countOfS; i++)
            {
                if (Double.IsNaN(S.instance(i).value(attributeIndex)))
                {
                    // For unknown/missing values we drop them and count how many so we can update calculations later
                    droppedExamples++;
                    Log.LogVerbose("IsNaN encountered calculating gain for attribute {0}", attributeIndex);

                    int targetValue = (int)S.instance(i).value(targetAttributeIndex);
                    if (targetValue == ID3.PositiveTargetValue)
                    {
                        pu++;
                    }
                    else if (targetValue == ID3.NegativeTargetValue)
                    {
                        nu++;
                    }
                    else
                    {
                        throw new Exception(String.Format("Unexpected target value of {0}", targetValue));
                    }

                    continue;
                }

                int value = (int)S.instance(i).value(attributeIndex);
                SvList[value].add(S.instance(i));
            }

            if (countOfS == droppedExamples)
            {
                // None of the examples had a value for the attribute
                return 0;
            }

            // Sum the expected entropy of the set Sv for each value of v (per Mitchell Eq. 3.4)
            double expectedEntropy = 0;
            for (int i = 0; i < S.attribute(attributeIndex).numValues(); i++)
            {
                double countOfSv = SvList[i].numInstances();
                double ratio_i = countOfSv / (countOfS - droppedExamples);
                double pAdjustment = pu * ratio_i;
                double nAdjustment = nu * ratio_i;
                double entropyOfSv = this.CalculateEntropy(SvList[i], targetAttributeIndex, pAdjustment, nAdjustment);

                expectedEntropy += entropyOfSv * ((countOfSv + pAdjustment + nAdjustment) / (countOfS));
            }

            // Now we have all the info we need to calculate gain for this attribute
            double entropyS = this.CalculateEntropy(S, targetAttributeIndex);
            double gain = entropyS - expectedEntropy;

            // TODO: remove
            if (Double.IsNaN(gain))
            {
            }
            else if (gain <= 0)
            {
            }

            return gain;
        }

        /// <summary>
        /// pAdjustment and nAdjustment are to adjust for unknown values. Taken from page 98 of the ID3 (Quinlan) paper.
        /// pAdjustment is pu * ratioi and nAdjustment is nu * ratioi
        /// </summary>
        private double CalculateEntropy(Instances S, int targetAttributeIndex, double pAdjustment = 0, double nAdjustment = 0)
        {
            // Number of discrete values that the target attribute can have
            int numTargetValues = S.attribute(targetAttributeIndex).numValues();

            // For each possible discrete value that the target attribute can have, count how many times it is present in the examples
            // For boolean target attributes it is just true/false
            int[] targetValueCounts = new int[numTargetValues]; // 0 positive, 1 negative

            // Iterate through the examples to count the proportion of examples for each value of the target attribute
            int countOfS = S.numInstances();
            if (countOfS == 0)
            {
                // Return 0 if there are no examples
                return 0;
            }

            for (int i = 0; i < countOfS; i++)
            {
                if (Double.IsNaN(S.instance(i).value(targetAttributeIndex)))
                {
                    // This shouldn't happen
                    throw new Exception(String.Format("Value at targetAttributeIndex {0} is NaN", targetAttributeIndex));
                }

                int value = (int)S.instance(i).value(targetAttributeIndex);
                targetValueCounts[value]++;
            }

            // Calculate the positive and negative terms from Mitchell Eq. 3.1 for entropy

            double pProportion = (targetValueCounts[0] + pAdjustment) / (double)(countOfS + pAdjustment + nAdjustment);
            double pPlus = 0;
            if (pProportion != 0) // Skip 0 to avoid log(0)
            {
                pPlus = (-1 * pProportion * Math.Log(pProportion, 2));
            }

            double nProportion = (targetValueCounts[1] + nAdjustment) / (double)(countOfS + pAdjustment + nAdjustment);
            double nPlus = 0;
            if (nProportion != 0) // Skip 0 to avoid log(0)
            {
                nPlus = (-1 * nProportion * Math.Log(nProportion, 2));
            }

            double entropy = pPlus + nPlus;

            // TODO: remove
            if (entropy > 1 || entropy < 0)
            {

            }

            return entropy;
        }
    }
}
