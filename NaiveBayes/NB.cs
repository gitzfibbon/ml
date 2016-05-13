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
        private Dictionary<string, Dictionary<string, double>> Likelihoods;
        private Dictionary<string, Document> Documents;

        public void Train(string trainingSetPath)
        {
            this.LoadTrainingData(trainingSetPath);
            this.Likelihoods = new Dictionary<string, Dictionary<string, double>>();

            // This is the main part of the Learn_Naive_Bayes_Test (#2) from Mitchell Table 6.2
            foreach (string targetValue in this.Targets.Keys)
            {
                this.Likelihoods.Add(targetValue, new Dictionary<string, double>());

                double priorProbability = this.Targets[targetValue].DocumentCount / (double)this.Vocabulary.ExampleCount;
                this.Targets[targetValue].PriorProbability = priorProbability;
                Trace.TraceInformation("Prior Probability for {0} is {1:0.00}", targetValue, priorProbability);

                int n = this.Targets[targetValue].Words.Keys.Count;
                Trace.TraceInformation("Number of distinct words for {0} is {1}", targetValue, n);

                foreach (string word in this.Vocabulary.Words.Keys)
                {
                    // number of times the word occurs for this Target
                    int n_k = this.Targets[targetValue].Words.ContainsKey(word) ? this.Targets[targetValue].Words[word] : 0;

                    // calculate likelihood
                    double likelihood = (n_k + 1) / (double)(n + this.Vocabulary.Words.Keys.Count);
                    double logLikelihood = Math.Log((n_k + 1) / (double)(n + this.Vocabulary.Words.Keys.Count));
                    //Trace.TraceInformation("{0},{1}", likelihood, logLikelihood);

                    this.Likelihoods[targetValue].Add(word, logLikelihood);
                }
            }

            Trace.TraceInformation("Finished training");
            Trace.TraceInformation("");
        }

        public void Test(string testingSetPath)
        {
            this.LoadTestingData(testingSetPath);

            foreach (Document document in this.Documents.Values)
            {
                this.Classify(document);
            }
        }

        private string Classify(Document document)
        {
            // This method is the implementation of Classify_Naive_Bayes_Text from Mitchell Table 6.2

            double argmax = 0;
            string estimatedTargetValue = String.Empty;

            foreach (string targetValue in this.Targets.Keys)
            {
                // This will be the cumulative product of the likelihoods for the given target value
                double cumulativeProduct = 1;
                bool atLeastOneMatch = false;

                foreach (string word in document.Words.Keys)
                {
                    if (!this.Likelihoods[targetValue].ContainsKey(word))
                    {
                        continue;
                    }

                    // Get the likelihood which was already calculated
                    double likelihood = this.Likelihoods[targetValue][word];
                    cumulativeProduct *= likelihood;
                    atLeastOneMatch = true;
                }

                if (atLeastOneMatch == false)
                {
                    Trace.TraceInformation("No words in item {0} were in the training set", document.Id);
                    return String.Empty;
                }

                double tempArgMax = this.Targets[targetValue].PriorProbability * cumulativeProduct;

                //Trace.TraceInformation("Item: {0}, TargetValue: {1}, Probability: {2}", document.Id, targetValue, tempArgMax);

                if (tempArgMax > argmax)
                {
                    argmax = tempArgMax;
                    estimatedTargetValue = targetValue;

                    //Trace.TraceInformation("{0} is max so far. Setting to max.", argmax);
                }

            }

            Trace.TraceInformation("{0} is max with probability {1}", estimatedTargetValue, argmax);
            return estimatedTargetValue;
        }


        private void LoadTrainingData(string trainingSetPath)
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

                    this.Vocabulary.ExampleCount++;
                    this.Targets[value].DocumentCount++;

                    // Add the words to the Target
                    for (int i = 2; i < parts.Length; i = i + 2)
                    {
                        string word = parts[i];
                        int wordCount = Convert.ToInt32(parts[i + 1]);

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

            Trace.TraceInformation("Vocabulary: {0} unique words, {1} total words, {2} documents", this.Vocabulary.Words.Keys.Count, this.Vocabulary.Words.Values.Sum(), this.Vocabulary.ExampleCount);
            Trace.TraceInformation("Spam: {0} unique words, {1} total words, {2} documents", this.Targets["spam"].Words.Keys.Count, this.Targets["spam"].Words.Values.Sum(), this.Targets["spam"].DocumentCount);
            Trace.TraceInformation("Ham: {0} unique words, {1} total words, {2} documents", this.Targets["ham"].Words.Keys.Count, this.Targets["ham"].Words.Values.Sum(), this.Targets["ham"].DocumentCount);
            Trace.TraceInformation("Done loading training data");
            Trace.TraceInformation("");
        }

        private void LoadTestingData(string testingSetPath)
        {
            Trace.TraceInformation("Loading testing data from {0}", testingSetPath);

            this.Documents = new Dictionary<string, Document>();

            using (StreamReader sr = File.OpenText(testingSetPath))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    string[] parts = s.Split(NB.Delimiter);
                    string id = parts[0];
                    string value = parts[1];

                    if (!this.Documents.ContainsKey(id))
                    {
                        this.Documents.Add(id, new Document(id, value));
                    }

                    // Add the words to the Document
                    for (int i = 2; i < parts.Length; i = i + 2)
                    {
                        string word = parts[i];
                        int wordCount = Convert.ToInt32(parts[i + 1]);

                        // Add to word list
                        this.Documents[id].Words.Add(word, wordCount);
                    }

                }
            }

            Trace.TraceInformation("Documents: {0} documents", this.Documents.Keys.Count);
            Trace.TraceInformation("Done loading testing data");
            Trace.TraceInformation("");
        }

    }
}
