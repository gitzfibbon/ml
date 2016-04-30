using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollaborativeFiltering
{
    public class Log
    {
        public static bool LogImportantOn = true;
        public static bool LogVerboseOn = true;
        public static bool LogPedanticOn = false;

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

        public static void LogPedantic(string msg, params object[] args)
        {
            if (LogPedanticOn)
            {
                Console.WriteLine(String.Format(msg, args));
            }
        }

    }
}
