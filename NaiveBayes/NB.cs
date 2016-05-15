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

        private double LaplaceSmoothing = 1.0;

        public void Train(string trainingSetPath, double laplaceSmooting = 1.0)
        {
            this.LoadTrainingData(trainingSetPath);
            this.Likelihoods = new Dictionary<string, Dictionary<string, double>>();
            this.LaplaceSmoothing = laplaceSmooting;

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
                    double likelihood = (n_k + laplaceSmooting) / (double)(n + (laplaceSmooting * this.Vocabulary.Words.Keys.Count));
                    this.Likelihoods[targetValue].Add(word, likelihood);
                }
            }

            Trace.TraceInformation("Finished training");
            Trace.TraceInformation("");
        }

        public void Test(string testingSetPath)
        {
            this.LoadTestingData(testingSetPath);

            int trueSpam = 0;
            int falseSpam = 0;
            int trueHam = 0;
            int falseHam = 0;

            Trace.TraceInformation("IsCorrect,Predicted,Actual,Probability,Id");
            foreach (Document document in this.Documents.Values)
            {
                string result = this.Classify(document);

                if (result.Equals("spam", StringComparison.InvariantCultureIgnoreCase) && document.Value.Equals("spam", StringComparison.InvariantCultureIgnoreCase))
                {
                    trueSpam++;
                }
                else if (result.Equals("spam", StringComparison.InvariantCultureIgnoreCase) && document.Value.Equals("ham", StringComparison.InvariantCultureIgnoreCase))
                {
                    falseSpam++;
                }
                else if (result.Equals("ham", StringComparison.InvariantCultureIgnoreCase) && document.Value.Equals("ham", StringComparison.InvariantCultureIgnoreCase))
                {
                    trueHam++;
                }
                else if (result.Equals("ham", StringComparison.InvariantCultureIgnoreCase) && document.Value.Equals("spam", StringComparison.InvariantCultureIgnoreCase))
                {
                    falseHam++;
                }
            }

            Trace.TraceInformation("");
            Trace.TraceInformation("trueSpam: {0}", trueSpam);
            Trace.TraceInformation("falseSpam: {0}", falseSpam);
            Trace.TraceInformation("trueHam: {0}", trueHam);
            Trace.TraceInformation("falseHam: {0}", falseHam);
            Trace.TraceInformation("Laplace Smoothing Parameter: {0}", this.LaplaceSmoothing);
            Trace.TraceInformation("Accuracy predicting spam: {0:0.000}", (trueSpam + trueHam) / (double)(trueSpam + falseSpam + trueHam + falseHam));
            Trace.TraceInformation("Precision predicting spam: {0:0.000}", (trueSpam) / (double)(trueSpam + falseSpam));
            Trace.TraceInformation("Recall predicting spam: {0:0.000}", (trueSpam) / (double)(trueSpam + falseHam));

        }

        private string Classify(Document document)
        {
            // This method is the implementation of Classify_Naive_Bayes_Text from Mitchell Table 6.2

            double argmax = Double.MinValue;
            string estimatedTargetValue = String.Empty;

            foreach (string targetValue in this.Targets.Keys)
            {
                // This will be the cumulative product of the likelihoods for the given target value
                double cumulativeSumOfLogLikelihood = 1;
                bool atLeastOneMatch = false;

                foreach (string word in document.Words.Keys)
                {
                    if (!this.Likelihoods[targetValue].ContainsKey(word))
                    {
                        continue;
                    }

                    // Get the likelihood which was already calculated
                    double likelihood = this.Likelihoods[targetValue][word];
                    cumulativeSumOfLogLikelihood += Math.Log(likelihood);
                    atLeastOneMatch = true;
                }

                if (atLeastOneMatch == false)
                {
                    Trace.TraceInformation("No words in item {0} were in the training set", document.Id);
                    return String.Empty;
                }

                double tempArgMax =  Math.Log(this.Targets[targetValue].PriorProbability) + cumulativeSumOfLogLikelihood;

                //Trace.TraceInformation("Item: {0}, TargetValue: {1}, Probability: {2}", document.Id, targetValue, tempArgMax);

                if (tempArgMax >= argmax)
                {
                    argmax = tempArgMax;
                    estimatedTargetValue = targetValue;

                    //Trace.TraceInformation("{0} is max so far. Setting to max.", argmax);
                }

            }

            Trace.TraceInformation("{0},{1},{2},{3},{4}",
                estimatedTargetValue.Equals(document.Value, StringComparison.InvariantCultureIgnoreCase), estimatedTargetValue, document.Value, argmax, document.Id);
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
