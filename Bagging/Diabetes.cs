using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using weka.core;

namespace Bagging
{
    /// <summary>
    /// This implementation is specific to this dataset
    /// http://archive.ics.uci.edu/ml/datasets/Pima+Indians+Diabetes
    /// </summary>
    public class Diabetes
    {
        private const char Delimiter = ',';
        private const int NumAttributes = 9;

        public static void Run(string trainingSetPath, string testingSetPath, int numberOfModels)
        {
            Trace.TraceInformation("Starting bagging");
            Trace.TraceInformation("TrainingSetPath: {0}", trainingSetPath);
            Trace.TraceInformation("TestingSetPath: {0}", testingSetPath);
            Trace.TraceInformation("Models: {0}", numberOfModels);

            Instances instances = Diabetes.LoadData(trainingSetPath);
        }

        private static Instances LoadData(string filePath)
        {
            Trace.TraceInformation("Loading data from {0}", filePath);

            Instances instances = Diabetes.DefineAttributes();

            // Read in each row, set its attribute values, add it to the Instances object
            using (StreamReader sr = File.OpenText(filePath))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    Instance instance = new Instance(Diabetes.NumAttributes);
                    instance.setDataset(instances);
                    string[] parts = s.Split(Diabetes.Delimiter);

                    Diabetes.ValueForNumberOfTimesPregnant(instance, 0, parts[0]);
                    //instance.setValue(0, Diabetes.ValueForNumberOfTimesPregnant(parts[0]));

                    instances.add(instance);

                }
            }

            return instances;
        }

        // Define all the attributes for the diabetes dataset
        private static Instances DefineAttributes()
        {
            FastVector attributes = new FastVector();

            FastVector numberOfTimesPregnant = new FastVector();
            numberOfTimesPregnant.addElement("zero"); // 0
            numberOfTimesPregnant.addElement("low"); // 1-4
            numberOfTimesPregnant.addElement("medium"); // 5-9
            numberOfTimesPregnant.addElement("high"); // 10+
            attributes.addElement(new weka.core.Attribute("numberOfTimesPregnant", numberOfTimesPregnant));

            FastVector plasmaGlucoseConcentration = new FastVector();
            plasmaGlucoseConcentration.addElement("normal");
            plasmaGlucoseConcentration.addElement("high");
            attributes.addElement(new weka.core.Attribute("plasmaGlucoseConcentration", plasmaGlucoseConcentration));

            FastVector diastolicBloodPressure = new FastVector();
            diastolicBloodPressure.addElement("low");
            diastolicBloodPressure.addElement("normal");
            diastolicBloodPressure.addElement("pre-high");
            diastolicBloodPressure.addElement("high");
            attributes.addElement(new weka.core.Attribute("diastolicBloodPressure", diastolicBloodPressure));

            FastVector tricepsSkinFoldThickness = new FastVector();
            tricepsSkinFoldThickness.addElement("low");
            tricepsSkinFoldThickness.addElement("normal");
            tricepsSkinFoldThickness.addElement("high");
            attributes.addElement(new weka.core.Attribute("tricepsSkinFoldThickness", tricepsSkinFoldThickness));

            FastVector twoHourSerumInsulin = new FastVector();
            twoHourSerumInsulin.addElement("normal");
            twoHourSerumInsulin.addElement("high");
            attributes.addElement(new weka.core.Attribute("twoHourSerumInsulin", twoHourSerumInsulin));

            FastVector bmi = new FastVector();
            bmi.addElement("underweight");
            bmi.addElement("normal");
            bmi.addElement("overweight");
            bmi.addElement("obese");
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
            diabetes.addElement("no");
            diabetes.addElement("yes");
            attributes.addElement(new weka.core.Attribute("diabetes", diabetes));

            Instances instances = new Instances("diabetes", attributes, 0);
            return instances;
        }

        private static void ValueForNumberOfTimesPregnant(Instance instance, int attributeIndex, string inputValue)
        {
            int numberOfTimesPregnant = Int32.Parse(inputValue);

            if (numberOfTimesPregnant <= 0)
            {
                instance.setValue(attributeIndex, "zero");
            }
            else if (numberOfTimesPregnant <= 4)
            {
                instance.setValue(attributeIndex, "low");
            }
            else if (numberOfTimesPregnant <= 9)
            {
                instance.setValue(attributeIndex, "medium");
            }
            else if (numberOfTimesPregnant >= 10)
            {
                instance.setValue(attributeIndex, "high");
            }
            else
            {
                return;
            }
        }

        private static string GetValueFor(string inputValue)
        {
            switch (inputValue)
            {
                case "aaa":
                    return "bbb";
                case "ccc":
                    return "ddd";
                default:
                    return null;
            }
        }

    }
}
