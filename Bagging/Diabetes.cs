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
        Quartiles = 1
    }

    /// <summary>
    /// This implementation is specific to this dataset
    /// http://archive.ics.uci.edu/ml/datasets/Pima+Indians+Diabetes
    /// </summary>
    public class Diabetes
    {
        private const char Delimiter = ',';
        private const int NumAttributes = 9;

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

        private static Instances LoadData(string filePath, Category category = Category.CommonRanges)
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
            if (category == Category.Quartiles)
            {
                instances = Diabetes.DefineQuartileAttributes();
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

        private static Instances DefineQuartileAttributes()
        {
            FastVector attributes = new FastVector();

            FastVector numberOfTimesPregnant = new FastVector();
            numberOfTimesPregnant.addElement("1");
            numberOfTimesPregnant.addElement("2");
            numberOfTimesPregnant.addElement("3");
            numberOfTimesPregnant.addElement("4");
            attributes.addElement(new weka.core.Attribute("numberOfTimesPregnant", numberOfTimesPregnant));

            FastVector plasmaGlucoseConcentration = new FastVector();
            plasmaGlucoseConcentration.addElement("1");
            plasmaGlucoseConcentration.addElement("2");
            plasmaGlucoseConcentration.addElement("3");
            plasmaGlucoseConcentration.addElement("4");
            attributes.addElement(new weka.core.Attribute("plasmaGlucoseConcentration", plasmaGlucoseConcentration));

            FastVector diastolicBloodPressure = new FastVector();
            diastolicBloodPressure.addElement("1");
            diastolicBloodPressure.addElement("2");
            diastolicBloodPressure.addElement("3");
            diastolicBloodPressure.addElement("4");
            attributes.addElement(new weka.core.Attribute("diastolicBloodPressure", diastolicBloodPressure));

            FastVector tricepsSkinFoldThickness = new FastVector();
            tricepsSkinFoldThickness.addElement("1");
            tricepsSkinFoldThickness.addElement("2");
            tricepsSkinFoldThickness.addElement("3");
            tricepsSkinFoldThickness.addElement("4");
            attributes.addElement(new weka.core.Attribute("tricepsSkinFoldThickness", tricepsSkinFoldThickness));

            FastVector twoHourSerumInsulin = new FastVector();
            twoHourSerumInsulin.addElement("1");
            twoHourSerumInsulin.addElement("2");
            twoHourSerumInsulin.addElement("3");
            twoHourSerumInsulin.addElement("4");
            attributes.addElement(new weka.core.Attribute("twoHourSerumInsulin", twoHourSerumInsulin));

            FastVector bmi = new FastVector();
            bmi.addElement("1");
            bmi.addElement("2");
            bmi.addElement("3");
            bmi.addElement("4");
            attributes.addElement(new weka.core.Attribute("bmi", bmi));

            FastVector diabetesPedigreeFunction = new FastVector();
            diabetesPedigreeFunction.addElement("1");
            diabetesPedigreeFunction.addElement("2");
            diabetesPedigreeFunction.addElement("3");
            diabetesPedigreeFunction.addElement("4");
            attributes.addElement(new weka.core.Attribute("diabetesPedigreeFunction", diabetesPedigreeFunction));

            FastVector age = new FastVector();
            age.addElement("1");
            age.addElement("2");
            age.addElement("3");
            age.addElement("4");
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

        private double[] BucketBoundaries(int numBuckets, double[] unsorted)
        {
            List<double> sorted = unsorted.ToList();
            sorted.OrderBy(x => x);

            double[] bucketBoundaries = new double[numBuckets];
            for (int i = 0; i < numBuckets; i++)
            {
                double percentile = i / (double) numBuckets;
                int percentileIndex = Convert.ToInt32(Math.Floor(sorted.Count * percentile));
                bucketBoundaries[i] = sorted[percentileIndex];
            }

            return bucketBoundaries;
        }
    }
}
