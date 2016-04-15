using System;
using DecisionTree;

class MainClass
{
    public static void Main(string[] args)
    {
        ID3 id3 = new ID3();
        id3.Train(@"C:\coding\ml\test\devD.arff");
        //id3.Train(@"C:\coding\ml\test\testingD.arff");
        //id3.Train(@"C:\coding\ml\test\training_subsetD.arff");
        
        //IkvmExample.classifyTest();
    }


}