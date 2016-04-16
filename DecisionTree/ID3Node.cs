using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTree
{
    public class ID3Node
    {
        bool isLeaf = false;

        // If we split on an attribute, set this to the index of the attribute from the list of attributes
        int splitAttributeIndex = -1;

        // If this is a leaf, set this to the predicted value (an index into the possible values of the target attribute)
        int attributeValue = -1;

        // Each child node is indexed by its attribute value which is an index/int
        List<ID3Node> child = new List<ID3Node>();
    }
}