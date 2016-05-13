using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaiveBayes
{
    public class Target
    {
        public Target(string value)
        {
            this.DocumentCount = 0;
            this.PriorProbability = 0;
            this.Value = value;
            this.Words = new Dictionary<string, int>();
        }

        public int DocumentCount { get; set; }
        public double PriorProbability { get; set; }
        public Dictionary<string, int> Words { get; set; }
        public string Value { get; set; }
    }
}
