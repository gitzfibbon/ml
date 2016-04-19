using System;
using DecisionTree;

class MainClass
{
    public static void Main(string[] args)
    {
        Log.GainOn = true;
        Log.InfoOn = true;
        Log.NodeOn = false;
        Log.StatsOn = true;
        //Log.VerboseOn = true;

        //Tennis(0.9);
        //Small(0.9);
        Full(0.99);

        //IkvmExample.classifyTest();
    }

    private static void Full(double confidenceLevel)
    {
        ID3 id3 = new ID3();
        ID3Node root = id3.Train(@"C:\coding\ml\test\training_subsetD.arff", confidenceLevel);
        id3.Test(@"C:\coding\ml\test\testingD.arff", root);
    }

    private static void Small(double confidenceLevel)
    {
        ID3 id3 = new ID3();
        ID3Node root = id3.Train(@"C:\coding\ml\test\devD.arff", confidenceLevel);
        id3.Test(@"C:\coding\ml\test\devD.arff", root);

    }

    private static void Tennis(double confidenceLevel)
    {
        ID3 id3 = new ID3();
        ID3Node root = id3.Train(@"C:\coding\ml\test\tennis.arff", confidenceLevel);
        id3.Test(@"C:\coding\ml\test\tennis.arff", root);
    }

}