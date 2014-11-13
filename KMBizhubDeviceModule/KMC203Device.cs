using System.Collections.Generic;
using System.Xml;

namespace KMBizhubDeviceModule
{
    /// <summary>
    /// Device access class supporting Konica Minolta C203.
    /// </summary>
    public class KMC203Device : KMBizhubDevice
    {
        /// <summary>
        /// The endpoint at which to receive active (including failed) jobs.
        /// </summary>
        public const string ActiveJobsEndpoint = "/wcd/job.xml";

        public KMC203Device(Dictionary<string, string> parameters)
            : base(parameters)
        {
        }

        public override ICollection<string> GetFailedJobIDs()
        {
            var ret = new List<string>();

            // ensure we're logged in
            Login();

            var doc = FetchXml(ActiveJobsEndpoint);
            var jobElements = doc.SelectNodes("/MFP/JobList/Print/Job[JobStatus/Status='ErrorPrinting']");
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
