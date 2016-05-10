using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaiveBayes
{
    public class SpamRecord
    {
        public string EmailId { get; set; }

        public bool IsSpam { get; set; }

        public Dictionary<string, int> Words { get; set; }
    }
}
