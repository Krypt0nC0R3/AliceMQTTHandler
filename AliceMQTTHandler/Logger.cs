using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceMQTTHandler
{
    static class Logger
    {
        public static void Message(string message)
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:dd.MM.yyyy HH.mm.ss:fff}] -> {message}");
            }
            catch { }
        }
        public static void Error(string message)
        {
            try
            {
                Console.WriteLine($"==========\n\n[{DateTime.Now:dd.MM.yyyy HH.mm.ss:fff}] -> Error!\n{message}\n\n==========");
            }
            catch { }
        }
        public static void Error(Exception e)
        {
            Error($"{e.Message}\n{e.StackTrace}");
        }
        public static void Warn(string message)
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:dd.MM.yyyy HH.mm.ss:fff}] -> Warning! {message}");
            }
            catch { }
        }
        public static void Warn(Exception e)
        {
            Warn($"{e.Message}\n{e.StackTrace}");
        }
    }
}
