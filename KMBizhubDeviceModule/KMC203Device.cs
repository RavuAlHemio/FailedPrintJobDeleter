using System.Collections.Generic;

namespace KMBizhubDeviceModule
{
    /// <summary>
    /// Device access class supporting Konica Minolta C203.
    /// </summary>
    public class KMC203Device : KMBizhubDevice
    {
        public KMC203Device(Dictionary<string, string> parameters)
            : base(parameters)
        {
        }

        public override string ErrorJobsXPath
        {
            get { return "/MFP/JobList/Print/Job[JobStatus/Status='ErrorPrinting']"; }
        }

        public override string ActiveJobsEndpoint
        {
            get { return "/wcd/job.xml"; }
        }
    }
}
