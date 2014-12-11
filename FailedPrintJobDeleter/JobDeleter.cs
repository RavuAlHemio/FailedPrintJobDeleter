using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using log4net;

namespace FailedPrintJobDeleter
{
    /// <summary>
    /// Manages the thread deleting the print jobs.
    /// </summary>
    public class JobDeleter
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Whether to stop processing.
        /// </summary>
        protected volatile bool StopNow = false;

        /// <summary>
        /// The configuration.
        /// </summary>
        private Config _config;

        /// <summary>
        /// The print job deletion thread.
        /// </summary>
        private Thread _thread;

        /// <summary>
        /// Initializes a job deleter.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        public JobDeleter(Config config)
        {
            _config = config;
        }

        protected void DeleteJobsOnPrinter(IPrinterDevice printerDevice)
        {
            for (; ; )
            {
                if (StopNow)
                {
                    return;
                }

                Logger.DebugFormat("fetching deletable jobs for {0}", printerDevice);

                ICollection<string> jobsToDelete;
                try
                {
                    jobsToDelete = printerDevice.GetFailedJobIDs();
                    foreach (var jobToDelete in jobsToDelete)
                    {
                        Logger.InfoFormat("deleting job {1} on printer {0}", printerDevice, jobToDelete);
                        printerDevice.DeleteFailedJob(jobToDelete);
                    }
                }
                catch (WebException we)
                {
                    Logger.WarnFormat("WebException while contacting {0}: {1}", printerDevice, we);
                    // next printer
                    break;
                }

                if (StopNow || jobsToDelete.Count == 0)
                {
                    return;
                }

                try
                {
                    Logger.Debug("Repetition delay.");
                    Thread.Sleep(TimeSpan.FromSeconds(_config.CheckRepetitionDelayInSeconds));
                }
                catch (ThreadInterruptedException)
                {
                    Logger.Debug("Interrupted!");
                }
            }
        }

        protected void Proc()
        {
            while (!StopNow)
            {
                foreach (var printerDevice in _config.PrinterDevices)
                {
                    if (StopNow)
                    {
                        return;
                    }

                    DeleteJobsOnPrinter(printerDevice);
                }

                if (StopNow)
                {
                    break;
                }

                try
                {
                    Logger.Debug("Sleeping.");
                    Thread.Sleep(TimeSpan.FromMinutes(_config.TimeBetweenChecksInMinutes));
                }
                catch (ThreadInterruptedException)
                {
                    Logger.Debug("Interrupted!");
                }
            }
        }

        public void Start()
        {
            StopNow = false;
            _thread = new Thread(Proc);
            _thread.Start();
        }

        public void Stop()
        {
            StopNow = true;
            _thread.Interrupt();
            _thread.Join();
        }
    }
}
