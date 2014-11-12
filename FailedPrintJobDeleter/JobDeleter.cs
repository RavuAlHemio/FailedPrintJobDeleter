using System;
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
        /// The print job deletion thread.
        /// </summary>
        private Thread _thread;

        protected void DeleteJobs()
        {
            foreach (var printerDevice in Config.PrinterDevices)
            {
                if (StopNow)
                {
                    break;
                }

                Logger.InfoFormat("fetching deletable jobs for {0}", printerDevice);

                var jobsToDelete = printerDevice.GetFailedJobIDs();
                foreach (var jobToDelete in jobsToDelete)
                {
                    Logger.InfoFormat("deleting job {1} on printer {0}", printerDevice, jobToDelete);
                    printerDevice.DeleteFailedJob(jobToDelete);
                }
            }
        }

        protected void Proc()
        {
            while (!StopNow)
            {
                DeleteJobs();

                if (StopNow)
                {
                    break;
                }

                try
                {
                    Thread.Sleep(TimeSpan.FromMinutes(Config.UpdatePeriodInMinutes));
                }
                catch (ThreadInterruptedException)
                {
                    Logger.Debug("interrupted!");
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
            _thread.Join();
        }
    }
}
