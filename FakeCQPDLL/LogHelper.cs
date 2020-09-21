using Launcher.Sdk.Cqp.Enum;
using System;

namespace CQP
{
    public static class LogHelper
    {        
        public static void WriteLine(string pluginname,CQLogLevel priority, string type, params string[] messages)
        {
            string msg = string.Empty;
            foreach (var item in messages)
                msg += item;
            switch (priority)
            {
                case CQLogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case CQLogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case CQLogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case CQLogLevel.Fatal:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case CQLogLevel.InfoSuccess:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case CQLogLevel.InfoSend:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case CQLogLevel.InfoReceive:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case CQLogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][{pluginname} {type}]{msg}");
        }
        public static void WriteLine(params string[] messages)
        {
            string msg = string.Empty;
            foreach (var item in messages)
                msg += item;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}");
        }
    }
}
