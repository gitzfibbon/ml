using System;
using DecisionTree;

class MainClass
{
    public static void Main(string[] args)
    {
        //Log.GainOn = true;
        //Log.InfoOn = true;
        //Log.NodeOn = true;
        Log.StatsOn = true;
        //Log.VerboseOn = true;

        double confidenceLevel = 0.95;
        string trainingSetPath = @"C:\coding\ml\test\training_subsetD.arff";
        string testingSetPath = @"C:\coding\ml\test\testingD.arff";

        if (args.Length >= 3)
        {
            confidenceLevel = Convert.ToDouble(args[0]);
            trainingSetPath = args[1];
            testingSetPath = args[2];
        }

        Console.WriteLine("confidenceLevel is {0}", confidenceLevel);
        Console.WriteLine("trainingSetPath is {0}", trainingSetPath);
        Console.WriteLine("testingSetPath is {0}", testingSetPath);

        ID3 id3 = new ID3();
        ID3Node root = id3.Train(trainingSetPath, confidenceLevel);
        id3.Test(testingSetPath, root);
    }

    private static void Full(double confidenceLevel)
    {
        ID3 id3 = new ID3();
        ID3Node root = id3.Train(@"C:\coding\ml\test\training_subsetD.arff", confidenceLevel);
        id3.Test(@"C:\coding\ml\test\testingD.arff", root);
    }

    private static void Tennis(double confidenceLevel)
    {
        ID3 id3 = new ID3();
        ID3Node root = id3.Train(@"C:\coding\ml\test\tennis.arff", confidenceLevel);
        id3.Test(@"C:\coding\ml\test\tennis.arff", root);
    }

}