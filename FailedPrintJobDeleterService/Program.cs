using System.ServiceProcess;

namespace FailedPrintJobDeleterService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var servicesToRun = new ServiceBase[] 
            { 
                new FailedPrintJobDeleterService() 
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
