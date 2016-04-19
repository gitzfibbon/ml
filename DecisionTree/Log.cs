using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTree
{
    public class Log
    {
        public static bool GainOn = false;
        public static bool InfoOn = false;
        public static bool NodeOn = false;
        public static bool StatsOn = false;
        public static bool VerboseOn = false;

        public static void LogGain(string msg, params object[] args)
        {
            if (GainOn)
            {
                Console.WriteLine(String.Format(msg, args));
            }
        }

        public static void LogInfo(string msg, params object[] args)
        {
            if (InfoOn)
            {
                Console.WriteLine(String.Format(msg, args));
            }
        }

        public static void LogNode(string msg, params object[] args)
        {
            if (NodeOn)
            {
                Console.WriteLine(String.Format(msg, args));
            }
        }


        public static void LogStats(string msg, params object[] args)
        {
            if (StatsOn)
            {
                Console.WriteLine(String.Format(msg, args));
            }
        }

        public static void LogVerbose(string msg, params object[] args)
        {
            if (VerboseOn)
            {
                Console.WriteLine(String.Format(msg, args));
            }
        }


    }
}
