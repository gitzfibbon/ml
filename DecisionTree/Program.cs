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

        Small();
 
        //IkvmExample.classifyTest();
    }

    private static void Full()
    {
        ID3 id3 = new ID3();
        ID3Node root = id3.Train(@"C:\coding\ml\test\testingD.arff");
        id3.Test(@"C:\coding\ml\test\training_subsetD.arff", root);
    }

    private static void Small()
    {
        ID3 id3 = new ID3();
        ID3Node root = id3.Train(@"C:\coding\ml\test\devD.arff");
        id3.Test(@"C:\coding\ml\test\devD.arff", root);

    }

    private static void Tennis()
    {
        ID3 id3 = new ID3();
        ID3Node root = id3.Train(@"C:\coding\ml\test\tennis.arff");
        id3.Test(@"C:\coding\ml\test\tennis.arff", root);
    }

}