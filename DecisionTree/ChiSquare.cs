using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using weka.core;

namespace DecisionTree
{
    public class ChiSquare
    {
        /// <summary>
        /// Return true if within the confidence interval
        /// </summary>
        public static bool ChiSquaredTest(double confidenceInterval, Instances S, int attributeIndex, int targetAttributeIndex)
        {
            double threshold = 1 - confidenceInterval;

            int df = S.attribute(attributeIndex).numValues() - 1;
            double chiSquaredStatistic = ChiSquare.ApproximateChiSquared(S, attributeIndex, targetAttributeIndex);
            double pValue = ChiSquareUtils.pochisq(chiSquaredStatistic, df);
            
            Log.LogStats("ChiSquared pValue is {0} and threshold is {1}", pValue, threshold);
            
            bool result = (pValue <= threshold);
            return result;
        }

        private static double ApproximateChiSquared(Instances S, int attributeIndex, int targetAttributeIndex)
        {
            // In the int array, element 1 is Positive and element 2 is Negative
            int indexOfPositive = 0;
            int indexOfNegative = 1;
            Dictionary<int, int[]> examplesList = new Dictionary<int, int[]>();
            for (int i = 0; i < S.attribute(attributeIndex).numValues(); i++)
            {
                examplesList.Add(i, new int[2]);
            }

            // Partition each example into a bucket based on its attribute value
            // Also, get a count of positive and negative target values
            double p = 0;
            double n = 0;
            int droppedExamples = 0;
            for (int i = 0; i < S.numInstances(); i++)
            {
                double value = S.instance(i).value(attributeIndex);

                if (Double.IsNaN(value))
                {
                    // Drop missing/unknown values but keep track of how many are dropped
                    droppedExamples++;
                    Log.LogVerbose("IsNaN encountered calculating chi-squared stat for attribute {0}", attributeIndex);
                    continue;
                }

                int targetValue = (int)S.instance(i).value(targetAttributeIndex);

                if (targetValue == ID3.PositiveTargetValue)
                {
                    p++;
                    examplesList[(int)value][indexOfPositive]++;
                }
                else if (targetValue == ID3.NegativeTargetValue)
                {
                    n++;
                    examplesList[(int)value][indexOfNegative]++;
                }
                else
                {
                    throw new Exception(String.Format("Unexpected targetValue value of {0}", targetValue));
                }
            }

            // Go through each partition to sum up the Chi-Squared statistic
            double chiSquaredStatistic = 0;
            for (int i = 0; i < S.attribute(attributeIndex).numValues(); i++)
            {
                double pi = examplesList[i][indexOfPositive];
                double ni = examplesList[i][indexOfNegative];

                double expectedPi = ChiSquare.ExpectedPi(p, n, pi, ni);
                double expectedNi = ChiSquare.ExpectedNi(p, n, pi, ni);

                chiSquaredStatistic += (Math.Pow(pi - expectedPi, 2) / expectedPi) + (Math.Pow(ni - expectedNi, 2) / expectedNi);
            }

            return chiSquaredStatistic;
        }

        private static double ExpectedPi(double p, double n, double pi, double ni)
        {
            double result = p * (pi + ni) / (p + n);
            return result;
        }

        private static double ExpectedNi(double p, double n, double pi, double ni)
        {
            double result = n * (pi + ni) / (p + n);
            return result;
        }

    }
}
