using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forex_arbitrage
{
    public static class Log
    {
        public static void Info(String value)
        {
            Console.WriteLine("[INFO] " + value);
        }

        public static void Error(String value)
        {
            Console.WriteLine("[ERROR] " + value);
        }
    }
}
