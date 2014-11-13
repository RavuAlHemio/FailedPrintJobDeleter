using System.Collections.Generic;
using System.Xml;

namespace KMBizhubDeviceModule
{
    /// <summary>
    /// Device access class supporting Konica Minolta C454, C554, and C554e.
    /// </summary>
    public class KMC554Device : KMBizhubDevice
    {
        /// <summary>
        /// The endpoint at which to receive active (including failed) jobs.
        /// </summary>
        public const string ActiveJobsEndpoint = "/wcd/job_active.xml";

        public KMC554Device(Dictionary<string, string> parameters)
            : base(parameters)
        {
        }

        public override ICollection<string> GetFailedJobIDs()
        {
            var ret = new List<string>();

            // ensure we're logged in
            Login();

            var doc = FetchXml(ActiveJobsEndpoint);
            var jobElements = doc.SelectNodes("/MFP/JobList/Job[JobStatus/Status='ErrorPrinting']");
            if (jobElements == null)
            {
                return ret.ToArray();
            }

            foreach (XmlElement jobElement in jobElements)
            {
                var jobIDNode = jobElement.SelectSingleNode("./JobID/text()");
                if (jobIDNode == null)
                {
                    continue;
                }

                var jobID = jobIDNode.Value;
                ret.Add(jobID);
            }

            return ret.ToArray();
        }
    }
}
