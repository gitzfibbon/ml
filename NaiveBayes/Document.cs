using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaiveBayes
{
    public class Document
    {
        public Document(string id, string value)
        {
            this.Id = id;
            this.Words = new Dictionary<string, int>(); 
            this.Value = value; 
        }

        public string Id;
        public Dictionary<string, int> Words { get; set; }
        public string Value { get; set; }
    }
}
