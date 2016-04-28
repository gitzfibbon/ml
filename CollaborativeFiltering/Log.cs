using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollaborativeFiltering
{
    public class Log
    {
        public static bool LogAlwaysOn = true;
        public static bool LogImportantOn = true;
        public static bool LogVerboseOn = true;

        public static void LogAlways(string msg, params object[] args)
        {
            if (LogAlwaysOn)
            {
                Console.WriteLine(String.Format(msg, args));
            }
        }

        public static void LogImportant(string msg, params object[] args)
        {
            if (LogImportantOn)
            {
                Console.WriteLine(String.Format(msg, args));
            }
        }

        public static void LogVerbose(string msg, params object[] args)
        {
            if (LogVerboseOn)
            {
                Console.WriteLine(String.Format(msg, args));
            }
        }

    }
}
