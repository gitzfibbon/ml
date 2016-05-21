using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NaiveBayes
{
    public class ExtraFeatures
    {
        // Steps:
        // 1. Read in the raw file
        // 2. Look for extra features
        // 3. Add the extra features in the words dictionary
        //      The dictionary key will start with a ! followed by the feature name
        public static void AddExtraFeatures(string fileId, Dictionary<string, int> words)
        {
            string dataFolder = ConfigurationManager.AppSettings["DataFolder"];
            if (String.IsNullOrWhiteSpace(dataFolder))
            {
                Trace.TraceWarning("DataFolder is not set in the configuration. Skipping extra features");
                return;
            }

            string filePath = Path.Combine(dataFolder, fileId.Substring(1)); // Ignore the leading '/' on the fileId

            if (!File.Exists(filePath))
            {
                Trace.TraceWarning("File {0} was not found. Skipping.", filePath);
                return;
            }

            Dictionary<string, int> features = new Dictionary<string, int>();
            StringBuilder fullText = new StringBuilder();

            using (StreamReader sr = File.OpenText(filePath))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    fullText.AppendLine(s);

                    ExtraFeatures.TestForKnownSender(s, words);
                    ExtraFeatures.TestForIsReply(s, words);
                    ExtraFeatures.TestForIsEdu(s, words);
                }
            }

            ExtraFeatures.TestForCatchPhrases(fullText.ToString(), words);
            ExtraFeatures.TestForAttachments(fullText.ToString(), words);
            
        }

        private static void AddOrIncrementWords(Dictionary<string, int> words, string key, int value)
        {
            if (words.ContainsKey(key))
            {
                words[key] += value;
            }
            else
            {
                words.Add(key, value);
            }
        }

        private static bool TestForKnownSender(string line, Dictionary<string, int> words)
        {
            // From: "Some Friend"
            if (Regex.IsMatch(line, "^From:", RegexOptions.Compiled) && !Regex.IsMatch(line, "@", RegexOptions.Compiled))
            {
                ExtraFeatures.AddOrIncrementWords(words, "!KnownSender", 1);
                return true;
            }

            return false;
        }

        private static bool TestForIsReply(string line, Dictionary<string, int> words)
        {
            // -----Original Message-----
            if (Regex.IsMatch(line, "-----Original Message-----", RegexOptions.Compiled))
            {
                ExtraFeatures.AddOrIncrementWords(words, "!IsReply", 1);
                return true;
            }

            return false;
        }
        private static bool TestForIsEdu(string line, Dictionary<string, int> words)
        {
            // From a .edu email address
            if (Regex.IsMatch(line, "^From:.*@.*edu>", RegexOptions.Compiled))
            {
                ExtraFeatures.AddOrIncrementWords(words, "!FromEdu", 1);
                return true;
            }

            return false;
        }

        private static void TestForCatchPhrases(string text, Dictionary<string, int> words)
        {
            if (Regex.IsMatch(text, "free money", RegexOptions.IgnoreCase & RegexOptions.Compiled))
            {
                ExtraFeatures.AddOrIncrementWords(words, "!CatchPhrase", 1);
            }

            if (Regex.IsMatch(text, "only $", RegexOptions.IgnoreCase & RegexOptions.Compiled))
            {
                ExtraFeatures.AddOrIncrementWords(words, "!CatchPhrase", 1);
            }

            if (Regex.IsMatch(text, "over 21", RegexOptions.IgnoreCase & RegexOptions.Compiled))
            {
                ExtraFeatures.AddOrIncrementWords(words, "!CatchPhrase", 1);
            }
        }
        private static void TestForAttachments(string text, Dictionary<string, int> words)
        {
            if (Regex.IsMatch(text, "Content-Disposition:.*attachment", RegexOptions.Compiled))
            {
                ExtraFeatures.AddOrIncrementWords(words, "!Attachment", 1);
            }
        }

    }
}
