using System;
using System.IO;

namespace FailedPrintJobDeleter
{
    public static class Util
    {
        /// <summary>
        /// Sets up logging from a configuration file or chooses some sane defaults.
        /// </summary>
        public static void SetupLogging()
        {
            var confFile = new FileInfo(Path.Combine(ProgramDirectory, "Logging.conf"));
            if (confFile.Exists)
            {
                log4net.Config.XmlConfigurator.Configure(confFile);
            }
            else
            {
                var rootLogger = ((log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository()).Root;
                rootLogger.Level = log4net.Core.Level.Debug;
                log4net.LogManager.GetRepository().Configured = true;

                // log to a file
                var fileLogAppender = new log4net.Appender.FileAppender
                {
                    Layout = new log4net.Layout.PatternLayout("%date [%thread] %-5level %logger [%property{NDC}] - %message%newline"),
                    File = Path.Combine(ProgramDirectory, "Log.txt"),
                    AppendToFile = true
                };
                fileLogAppender.ActivateOptions();
                rootLogger.AddAppender(fileLogAppender);
            }
        }

        public static string ProgramDirectory
        {
            get
            {
                var localPath = (new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
                return Path.GetDirectoryName(localPath);
            }
        }
    }
}
