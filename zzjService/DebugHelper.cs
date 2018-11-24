using System;
using System.Collections.Generic;
using System.Text;

namespace zzjService
{
    public class DebugHelper
    {
        public static void PrintTxMessage(string s)
        {
            var fc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[{DateTime.Now}]Tx::{s}");
            Console.ForegroundColor = fc;
        }

        public static void PrintRxMessage(string s)
        {
            var fc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{DateTime.Now}]Rx::{s}");
            Console.ForegroundColor = fc;
        }

    }
}
