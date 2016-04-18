﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using weka.core;

namespace DecisionTree
{
    public class ID3Node
    {
        public bool IsLeaf = false;

        // If we split on an attribute, set this to the index of the attribute from the list of attributes
        public int SplitAttributeIndex = -1;

        // If this is a leaf, set this to the predicted value (an index into the possible values of the target attribute)
        public int AttributeValue = -1;

        // Each child node is indexed by its attribute value which is an index/int
        public List<ID3Node> ChildNodes = new List<ID3Node>();

        public static void Print(ID3Node node, Instances instances)
        {
            int targetAttribute = instances.numAttributes() - 1;

            string output;

            if (node.IsLeaf == true)
            {
                output = String.Format("Leaf ({0}) from ({1})", instances.attribute(targetAttribute).value(node.AttributeValue), instances.attribute(node.SplitAttributeIndex).name());
            }
            else
            {
                output = String.Format("Split ({0})", instances.attribute(node.SplitAttributeIndex).name());
            }

            Console.WriteLine(output);
        }

        public static void BFS(ID3Node root, Instances instances)
        {
            Queue<ID3Node> q = new Queue<ID3Node>();
            q.Enqueue(root);//You don't need to write the root here, it will be written in the loop
            while (q.Count() > 0)
            {
                ID3Node n = q.Dequeue();
                ID3Node.Print(n, instances);

                foreach (ID3Node child in n.ChildNodes)
                {
                    q.Enqueue(child);
                }
            }
        }
    }
}