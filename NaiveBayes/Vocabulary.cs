using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaiveBayes
{
    public class Vocabulary
    {
        public Vocabulary()
        {
            this.Words = new Dictionary<string, int>();
        }
        public Dictionary<string, int> Words { get; set; }
    }
}
