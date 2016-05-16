using System;
using System.Collections.Generic;
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
        private const char Delimiter = ' ';

        public static void Run(string trainingSetPath, string testingSetPath, int numberOfModels)
        {
            Diabetes.LoadTrainingData(trainingSetPath);
        }

        private static Instances LoadTrainingData(string trainingSetPath)
        {
            Diabetes.DefineAttributes();

            return null;
        }

        // Define all the attributes for the diabetes dataset
        private static Instances DefineAttributes()
        {
            FastVector attributes = new FastVector();

            FastVector numberOfTimesPregnant = new FastVector();
            numberOfTimesPregnant.addElement("zero"); // 0
            numberOfTimesPregnant.addElement("low"); // low
            numberOfTimesPregnant.addElement("medium"); // medium
            numberOfTimesPregnant.addElement("high"); // high
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
    }
}
