using System;
using FailedPrintJobDeleter;
using log4net;

namespace FailedPrintJobDeleterCLI
{
    static class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            Config.Load();

            // setup logging, including console logging
            Util.SetupLogging();
            SetupConsoleLogging();

            // prepare everything
            var deleter = new JobDeleter();

            deleter.Start();

            Console.WriteLine("Starting. Press Enter or Escape to stop.");
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter || key.Key == ConsoleKey.Escape)
                {
                    break;
                }
            }
            Console.WriteLine("Stopping...");

            deleter.Stop();
        }

        static void SetupConsoleLogging()
        {
            var rootLogger = ((log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository()).Root;
            var consoleAppender = new log4net.Appender.ConsoleAppender
            {
                Threshold = log4net.Core.Level.Debug,
                Layout = new log4net.Layout.PatternLayout("%date [%thread] %-5level %logger [%property{NDC}] - %message%newline")
            };
            consoleAppender.ActivateOptions();
            rootLogger.AddAppender(consoleAppender);
        }
    }
}
