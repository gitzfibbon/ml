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
        //private static double ApproximateChiSquared(double p, double n, double pi, double ni)
        private static double ApproximateChiSquared(Instances S, int attributeIndex, int targetAttributeIndex)
        {
            Dictionary<int, Instances> examplesList = new Dictionary<int, Instances>();
            for (int i = 0; i < S.attribute(attributeIndex).numValues(); i++)
            {
                examplesList.Add(i, new Instances(S, 0,0));
            }

            // Partition each example into a bucket based on its attribute value
            // Also, get a count of positive and negative target values
            double p = 0;
            double n = 0;
            for (int i = 0; i < S.numInstances(); i++ )
            {
                int value = (int)S.instance(i).value(attributeIndex);
                examplesList[value].add(S.instance(i));

                int targetValue = (int)S.instance(i).value(targetAttributeIndex);
                if (targetValue == ID3.PositiveTargetValue)
                {
                    p++;
                }
                else if (targetValue == ID3.NegativeTargetValue)
                {
                    n++;
                }
                else
                {
                    throw new Exception(String.Format("Unexpected targetValue value of {0}", targetValue));
                }
            }

            // Go through each partition to sum up the Chi-Squared statistic
            for (int i=0; i < S.attribute(attributeIndex).numValues(); i++)
            {
                double expectedPi = ChiSquare.ExpectedPi(p, n, pi, ni);
                double expectedNi = ChiSquare.ExpectedNi(p, n, pi, ni);

            }


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
