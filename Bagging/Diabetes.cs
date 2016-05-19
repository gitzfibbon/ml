using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DecisionTree;
using weka.core;

namespace Bagging
{
    public enum Category
    {
        CommonRanges = 0,
        Buckets = 1
    }

    /// <summary>
    /// This implementation is specific to this dataset
    /// http://archive.ics.uci.edu/ml/datasets/Pima+Indians+Diabetes
    /// </summary>
    public class Diabetes
    {
        private const char Delimiter = ',';
        private const int NumAttributes = 9;
        private const int NumBuckets = 4;

        public static void Run(string trainingSetPath, string testingSetPath, int numberOfModels, int? randomSeed)
        {
            Trace.TraceInformation("Starting bagging");
            Trace.TraceInformation("TrainingSetPath: {0}", trainingSetPath);
            Trace.TraceInformation("TestingSetPath: {0}", testingSetPath);
            Trace.TraceInformation("Models: {0}", numberOfModels);

            Instances trainingInstances = Diabetes.LoadData(trainingSetPath);
            Bagging bagging = new Bagging();
            bagging.Train(trainingInstances, numberOfModels, randomSeed);
            Instances testingInstances = Diabetes.LoadData(testingSetPath);
            bagging.TestNonBagging(testingInstances);
            bagging.Test(testingInstances);
        }

        private static Instances LoadData(string filePath, Category category = Category.Buckets)
        {
            Trace.TraceInformation("Loading data from {0}", filePath);

            List<double[]> data = new List<double[]>();

            // Read in each row
            using (StreamReader sr = File.OpenText(filePath))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    string[] parts = s.Split(Diabetes.Delimiter);
                    double[] row = new double[Diabetes.NumAttributes];
                    for (int i = 0; i < parts.Length; i++)
                    {
                        row[i] = Double.Parse(parts[i]);
                    }
                    data.Add(row);
                }
            }

            // Set the attribute values, add to the Instances object
            Instances instances;
            if (category == Category.Buckets)
            {
                instances = Diabetes.DefineBucketAttributes(Diabetes.NumBuckets);

                // Calculate the bucket boundaries for each attribute except the target attribute
                List<double[]> allBucketBoundaries = new List<double[]>();
                for (int i = 0; i < Diabetes.NumAttributes - 1; i++)
                {
                    double[] attributeData = data.Select(x => x[i]).ToArray();
                    double[] bucketBoundaries = Diabetes.BucketBoundaries(Diabetes.NumBuckets, attributeData);
                    allBucketBoundaries.Add(bucketBoundaries);
                }

                // Put each value into a quartile/bucket
                for (int i = 0; i < data.Count; i++)
                {
                    Instance instance = new Instance(Diabetes.NumAttributes);
                    instance.setDataset(instances);

                    for (int j = 0; j < Diabetes.NumAttributes - 1; j++)
                    {
                        // Figure out which quartile/bucket the value should get dropped into
                        double[] bucketBoundaries = allBucketBoundaries[j];
                        for (int k = 0; k < bucketBoundaries.Length; k++)
                        {
                            double value = data[i][j];
                            if (k == 0 && value <= bucketBoundaries[k])
                            {
                                // bucket k
                                instance.setValue(j, k.ToString());
                                break;
                            }
                            else if (k == bucketBoundaries.Length - 1)
                            {
                                // bucket k+1
                                instance.setValue(j, (k+1).ToString());
                                break;
                            }
                            else if (value > bucketBoundaries[k] && value <= bucketBoundaries[k + 1])
                            {
                                // bucket k+1
                                instance.setValue(j, (k+1).ToString());
                                break;
                            }

                        }
                    }

                    Diabetes.ValueForDiabetes(instance, Diabetes.NumAttributes - 1, data[i][Diabetes.NumAttributes-1]);
                    instances.add(instance);
                }
            }
            else
            {
                instances = Diabetes.DefineCategoricalAttributes();

                foreach (double[] row in data)
                {
                    Instance instance = new Instance(Diabetes.NumAttributes);
                    instance.setDataset(instances);
                    Diabetes.ValueForNumberOfTimesPregnant(instance, 0, row[0]);
                    Diabetes.ValueForPlasmaGlucoseConcentrationt(instance, 1, row[1]);
                    Diabetes.ValueForDiastolicBloodPressure(instance, 2, row[2]);
                    Diabetes.ValueForTricepsSkinFoldThickness(instance, 3, row[3]);
                    Diabetes.ValueForTwoHourSerumInsulin(instance, 4, row[4]);
                    Diabetes.ValueForBmi(instance, 5, row[5]);
                    Diabetes.ValueForDiabetesPedigreeFunction(instance, 6, row[6]);
                    Diabetes.ValueForAge(instance, 7, row[7]);
                    Diabetes.ValueForDiabetes(instance, 8, row[8]);
                    instances.add(instance);
                }
            }

            return instances;
        }

        private static Instances DefineBucketAttributes(int numBuckets)
        {
            FastVector attributes = new FastVector();

            FastVector numberOfTimesPregnant = new FastVector();
            FastVector plasmaGlucoseConcentration = new FastVector();
            FastVector diastolicBloodPressure = new FastVector();
            FastVector tricepsSkinFoldThickness = new FastVector();
            FastVector twoHourSerumInsulin = new FastVector();
            FastVector bmi = new FastVector();
            FastVector diabetesPedigreeFunction = new FastVector();
            FastVector age = new FastVector();

            for (int i = 0; i < numBuckets; i++)
            {
                string attributeValueName = i.ToString();

                numberOfTimesPregnant.addElement(attributeValueName);
                plasmaGlucoseConcentration.addElement(attributeValueName);
                diastolicBloodPressure.addElement(attributeValueName);
                tricepsSkinFoldThickness.addElement(attributeValueName);
                twoHourSerumInsulin.addElement(attributeValueName);
                bmi.addElement(attributeValueName);
                diabetesPedigreeFunction.addElement(attributeValueName);
                age.addElement(attributeValueName);
            }
            
            attributes.addElement(new weka.core.Attribute("numberOfTimesPregnant", numberOfTimesPregnant));
            attributes.addElement(new weka.core.Attribute("plasmaGlucoseConcentration", plasmaGlucoseConcentration));
            attributes.addElement(new weka.core.Attribute("diastolicBloodPressure", diastolicBloodPressure));
            attributes.addElement(new weka.core.Attribute("tricepsSkinFoldThickness", tricepsSkinFoldThickness));
            attributes.addElement(new weka.core.Attribute("twoHourSerumInsulin", twoHourSerumInsulin));
            attributes.addElement(new weka.core.Attribute("bmi", bmi));
            attributes.addElement(new weka.core.Attribute("diabetesPedigreeFunction", diabetesPedigreeFunction));
            attributes.addElement(new weka.core.Attribute("age", age));

            FastVector diabetes = new FastVector();
            diabetes.addElement("0"); // negative
            diabetes.addElement("1"); // positive
            attributes.addElement(new weka.core.Attribute("diagnosis", diabetes));

            Instances instances = new Instances("diabetes", attributes, 0);
            return instances;
        }

        // Define all the attributes for the diabetes dataset
        private static Instances DefineCategoricalAttributes()
        {
            FastVector attributes = new FastVector();

            FastVector numberOfTimesPregnant = new FastVector();
            numberOfTimesPregnant.addElement("zero"); // 0
            numberOfTimesPregnant.addElement("low"); // 1-4
            numberOfTimesPregnant.addElement("medium"); // 5-9
            numberOfTimesPregnant.addElement("high"); // 10+
            attributes.addElement(new weka.core.Attribute("numberOfTimesPregnant", numberOfTimesPregnant));

            FastVector plasmaGlucoseConcentration = new FastVector();
            plasmaGlucoseConcentration.addElement("normal"); // < 140
            plasmaGlucoseConcentration.addElement("high"); // >= 140
            attributes.addElement(new weka.core.Attribute("plasmaGlucoseConcentration", plasmaGlucoseConcentration));

            FastVector diastolicBloodPressure = new FastVector();
            diastolicBloodPressure.addElement("low"); // < 60
            diastolicBloodPressure.addElement("normal"); // 60 to 79
            diastolicBloodPressure.addElement("pre-high"); // 80 to 89
            diastolicBloodPressure.addElement("high"); // 90+
            attributes.addElement(new weka.core.Attribute("diastolicBloodPressure", diastolicBloodPressure));

            FastVector tricepsSkinFoldThickness = new FastVector();
            tricepsSkinFoldThickness.addElement("low"); // < 4.5
            tricepsSkinFoldThickness.addElement("normal"); // 4.5 to 36.5
            tricepsSkinFoldThickness.addElement("high"); // > 36.5
            attributes.addElement(new weka.core.Attribute("tricepsSkinFoldThickness", tricepsSkinFoldThickness));

            FastVector twoHourSerumInsulin = new FastVector();
            twoHourSerumInsulin.addElement("normal"); // < 166
            twoHourSerumInsulin.addElement("high"); // >= 166
            attributes.addElement(new weka.core.Attribute("twoHourSerumInsulin", twoHourSerumInsulin));

            FastVector bmi = new FastVector();
            bmi.addElement("underweight"); // < 18.5
            bmi.addElement("normal"); // 18.5 to 25
            bmi.addElement("overweight"); // 25 to 30
            bmi.addElement("obese"); // 30+
            attributes.addElement(new weka.core.Attribute("bmi", bmi));

            FastVector diabetesPedigreeFunction = new FastVector();
            diabetesPedigreeFunction.addElement("low"); // < 0.2
            diabetesPedigreeFunction.addElement("normal"); // 0.2 to 0.7
            diabetesPedigreeFunction.addElement("high"); // > 0.7
            attributes.addElement(new weka.core.Attribute("diabetesPedigreeFunction", diabetesPedigreeFunction));

            FastVector age = new FastVector();
            age.addElement("young"); // under 30
            age.addElement("middle"); // 30 to 50 inclusive
            age.addElement("old"); // over 50
            attributes.addElement(new weka.core.Attribute("age", age));

            FastVector diabetes = new FastVector();
            diabetes.addElement("0"); // negative
            diabetes.addElement("1"); // positive
            attributes.addElement(new weka.core.Attribute("diagnosis", diabetes));

            Instances instances = new Instances("diabetes", attributes, 0);
            return instances;
        }

        private static void ValueForNumberOfTimesPregnant(Instance instance, int attributeIndex, double inputValue)
        {
            int value = Convert.ToInt32(inputValue);

            if (value <= 0)
            {
                instance.setValue(attributeIndex, "zero");
            }
            else if (value <= 4)
            {
                instance.setValue(attributeIndex, "low");
            }
            else if (value <= 9)
            {
                instance.setValue(attributeIndex, "medium");
            }
            else if (value >= 10)
            {
                instance.setValue(attributeIndex, "high");
            }
        }

        private static void ValueForPlasmaGlucoseConcentrationt(Instance instance, int attributeIndex, double inputValue)
        {
            int value = Convert.ToInt32(inputValue);

            if (value <= 0)
            {
                return;
            }
            else if (value < 140)
            {
                instance.setValue(attributeIndex, "normal");
            }
            else if (value >= 140)
            {
                instance.setValue(attributeIndex, "high");
            }
        }

        private static void ValueForDiastolicBloodPressure(Instance instance, int attributeIndex, double inputValue)
        {
            int value = Convert.ToInt32(inputValue);

            if (value <= 0)
            {
                return;
            }
            else if (value < 60)
            {
                instance.setValue(attributeIndex, "low");
            }
            else if (value < 80)
            {
                instance.setValue(attributeIndex, "normal");
            }
            else if (value < 90)
            {
                instance.setValue(attributeIndex, "pre-high");
            }
            else if (value >= 90)
            {
                instance.setValue(attributeIndex, "high");
            }
        }

        private static void ValueForTricepsSkinFoldThickness(Instance instance, int attributeIndex, double inputValue)
        {
            int value = Convert.ToInt32(inputValue);

            if (value <= 0)
            {
                return;
            }
            else if (value < 36.5)
            {
                instance.setValue(attributeIndex, "normal");
            }
            else if (value >= 36.5)
            {
                instance.setValue(attributeIndex, "high");
            }
        }

        private static void ValueForTwoHourSerumInsulin(Instance instance, int attributeIndex, double inputValue)
        {
            int value = Convert.ToInt32(inputValue);

            if (value <= 0)
            {
                return;
            }
            else if (value < 166)
            {
                instance.setValue(attributeIndex, "normal");
            }
            else if (value >= 166)
            {
                instance.setValue(attributeIndex, "high");
            }
        }

        private static void ValueForBmi(Instance instance, int attributeIndex, double inputValue)
        {
            double value = inputValue;

            if (value <= 0)
            {
                return;
            }
            else if (value < 18.5)
            {
                instance.setValue(attributeIndex, "underweight");
            }
            else if (value < 25)
            {
                instance.setValue(attributeIndex, "normal");
            }
            else if (value < 30)
            {
                instance.setValue(attributeIndex, "overweight");
            }
            else if (value >= 30)
            {
                instance.setValue(attributeIndex, "obese");
            }
        }

        private static void ValueForDiabetesPedigreeFunction(Instance instance, int attributeIndex, double inputValue)
        {
            double value = inputValue;

            if (value <= 0)
            {
                return;
            }
            else if (value < 0.2)
            {
                instance.setValue(attributeIndex, "low");
            }
            else if (value < 0.7)
            {
                instance.setValue(attributeIndex, "normal");
            }
            else if (value >= 0.7)
            {
                instance.setValue(attributeIndex, "high");
            }
        }

        private static void ValueForAge(Instance instance, int attributeIndex, double inputValue)
        {
            int value = Convert.ToInt32(inputValue);

            if (value <= 0)
            {
                return;
            }
            else if (value < 30)
            {
                instance.setValue(attributeIndex, "young");
            }
            else if (value < 50)
            {
                instance.setValue(attributeIndex, "middle");
            }
            else if (value >= 50)
            {
                instance.setValue(attributeIndex, "old");
            }
        }

        private static void ValueForDiabetes(Instance instance, int attributeIndex, double inputValue)
        {
            int value = Convert.ToInt32(inputValue);

            if (value == 0)
            {
                instance.setValue(attributeIndex, "0");
            }
            else if (value == 1)
            {
                instance.setValue(attributeIndex, "1");
            }
            else
            {
                return;
            }
        }

        // Gets the numerical values for specified percentiles.
        // Eg. if numBuckets is 2 it will return the median
        // Eg. if numBuckets is 4 it will return the median and 1st and 3rd quartiles
        private static double[] BucketBoundaries(int numBuckets, double[] unsorted)
        {
            List<double> sorted = unsorted.ToList().OrderBy(x => x).ToList();

            double[] bucketBoundaries = new double[numBuckets - 1];
            for (int i = 1; i < numBuckets; i++)
            {
                double percentile = i / (double)numBuckets;
                int percentileIndex = Convert.ToInt32(Math.Floor(sorted.Count * percentile));
                bucketBoundaries[i - 1] = sorted[percentileIndex];
            }

            return bucketBoundaries;
        }
    }
}
