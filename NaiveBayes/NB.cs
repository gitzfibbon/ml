using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaiveBayes
{
    public class NB
    {
        private const char Delimiter = ' ';

        private Vocabulary Vocabulary;
        private Dictionary<string, Target> Targets;

        public void Train(string trainingSetPath)
        {
            this.LoadTrainingData(trainingSetPath);

            foreach (string targetValue in this.Targets.Keys)
            {

            }
        }

        public void LoadTrainingData(string trainingSetPath)
        {
            Trace.TraceInformation("Loading training data from {0}", trainingSetPath);

            this.Vocabulary = new Vocabulary();
            this.Targets = new Dictionary<string, Target>();

            using (StreamReader sr = File.OpenText(trainingSetPath))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    string[] parts = s.Split(NB.Delimiter);
                    string value = parts[1];

                    if (!this.Targets.ContainsKey(value))
                    {
                        this.Targets.Add(value, new Target(value));
                    }

                    this.Targets[value].DocumentCount++;

                    // Add the words to the Target
                    for (int i = 2; i < parts.Length; i = i + 2)
                    {
                        string word = parts[i];
                        int wordCount = Convert.ToInt32(parts[i+1]);

                        // Add to the vocabulary list
                        if (this.Vocabulary.Words.ContainsKey(word))
                        {
                            this.Vocabulary.Words[word] += wordCount;
                        }
                        else
                        {
                            this.Vocabulary.Words.Add(word, wordCount);
                        }

                        // Add to the spam or ham list
                        if (this.Targets[value].Words.ContainsKey(word))
                        {
                            this.Targets[value].Words[word] += wordCount;
                        }
                        else
                        {
                            this.Targets[value].Words.Add(word, wordCount);
                        }
                    }

                }
            }

            Trace.TraceInformation("Found {0} unique words for vocabulary", this.Vocabulary.Words.Keys.Count);
            Trace.TraceInformation("Found {0} unique words for spam", this.Targets["spam"].Words.Keys.Count);
            Trace.TraceInformation("Found {0} unique words for ham", this.Targets["ham"].Words.Keys.Count);
            Trace.TraceInformation("Done loading training data");
        }

        public void Test(string testingSetPath)
        {

        }


    }
}
