using System.Collections.Generic;

namespace FailedPrintJobDeleter
{
    /// <summary>
    /// A printer device.
    /// </summary>
    public interface IPrinterDevice
    {
        /// <summary>
        /// Fetch a list of failed job IDs.
        /// </summary>
        /// <returns>List of failed job IDs.</returns>
        IEnumerable<string> GetFailedJobIDs();

        /// <summary>
        /// Deletes a job which failed.
        /// </summary>
        /// <param name="jobID">The ID of the failed job to delete.</param>
        void DeleteFailedJob(string jobID);
    }
}
