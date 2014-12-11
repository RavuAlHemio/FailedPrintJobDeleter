using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Xml;
using FailedPrintJobDeleter;
using log4net;

namespace KMBizhubDeviceModule
{
    /// <summary>
    /// Device access class supporting Konica Minolta bizhub devices.
    /// </summary>
    public abstract class KMBizhubDevice : IPrinterDevice
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected const string PaperJamCode = "140005";
        protected static readonly HashSet<string> NonErrorCodes = new HashSet<string> { PaperJamCode };

        /// <summary>
        /// The endpoint to which to post login requests.
        /// </summary>
        public const string LoginEndpoint = "/wcd/ulogin.cgi";

        /// <summary>
        /// The endpoint at which to receive active (including failed) jobs.
        /// </summary>
        public const string DeleteJobEndpoint = "/wcd/user.cgi";

        /// <summary>
        /// The endpoint at which to receive general printer status information.
        /// </summary>
        public const string CommonStatusEndpoint = "/wcd/common.xml";

        /// <summary>
        /// The hostname of the printer.
        /// </summary>
        public string Hostname { get; protected set; }

        /// <summary>
        /// The password of the admin user.
        /// </summary>
        public string AdminPassword { get; protected set; }

        /// <summary>
        /// Whether HTTPS should be used.
        /// </summary>
        public bool Https { get; protected set; }

        /// <summary>
        /// The web client.
        /// </summary>
        protected readonly CookieWebClient Client;

        /// <summary>
        /// Initialize a KMBizhubDevice with the given parameters.
        /// </summary>
        /// <param name="parameters">Parameters to this module.</param>
        public KMBizhubDevice(Dictionary<string, string> parameters)
        {
            Hostname = parameters["Hostname"];
            AdminPassword = parameters["AdminPassword"];
            Https = parameters.ContainsKey("Https") && bool.Parse(parameters["Https"]);

            Client = new CookieWebClient();
            Client.IgnoreCookiePaths = true;
        }

        /// <summary>
        /// XPath string to fetch all jobs that have an error.
        /// </summary>
        public abstract string ErrorJobsXPath { get; }

        /// <summary>
        /// The endpoint at which to receive active (including failed) jobs.
        /// </summary>
        public abstract string ActiveJobsEndpoint { get; }

        /// <summary>
        /// Returns the URI for a specific endpoint on the printer.
        /// </summary>
        /// <param name="endpoint">The endpoint for which to return a URI.</param>
        /// <returns>The URI for the given endpoint on the printer.</returns>
        protected virtual Uri GetUri(string endpoint)
        {
            return new Uri(string.Format(
                "http{0}://{1}{2}",
                Https ? "s" : "",
                Hostname,
                endpoint
            ));
        }

        /// <summary>
        /// Fetches an XML document from the printer.
        /// </summary>
        /// <param name="endpoint">The endpoint for which to return the XML document.</param>
        protected virtual XmlDocument FetchXml(string endpoint)
        {
            var docString = Client.DownloadString(GetUri(endpoint));
            var doc = new XmlDocument();
            doc.LoadXml(docString);
            return doc;
        }

        public ICollection<string> GetFailedJobIDs()
        {
            var ret = new List<string>();

            // ensure we're logged in
            Login();

            // check status
            var statusDoc = FetchXml(CommonStatusEndpoint);
            var statusElement = statusDoc.SelectSingleNode("/MFP/DeviceStatus");
            if (statusElement != null)
            {
                var printerStatusNode = statusElement.SelectSingleNode("./PrintStatus/text()");
                var scannerStatusNode = statusElement.SelectSingleNode("./ScanStatus/text()");
                var printerStatus = printerStatusNode == null ? "unknown" : printerStatusNode.Value;
                var scannerStatus = scannerStatusNode == null ? "unknown" : scannerStatusNode.Value;

                Logger.DebugFormat("{0}: printer status {1}, scanner status {2}", this, printerStatus, scannerStatus);

                if (NonErrorCodes.Contains(printerStatus))
                {
                    Logger.InfoFormat("{0}: non-delete status {1}; not deleting anything", this, printerStatus);
                    return ret.ToArray();
                }
            }

            var doc = FetchXml(ActiveJobsEndpoint);
            var jobElements = doc.SelectNodes(ErrorJobsXPath);
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

        public virtual void DeleteFailedJob(string jobID)
        {
            // ensure we're logged in
            Login();

            // FIXME: check if job is actually failed?

            var values = new NameValueCollection
            {
                {"func", "PSL_J_DEL"},
                {"H_JID", jobID}
            };

            Client.UploadValues(
                GetUri(DeleteJobEndpoint),
                "POST",
                values
            );
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", GetType().Name, Hostname);
        }

        protected void AddCookie(string cookieName, string cookieValue)
        {
            Client.CookieJar.Add(new Cookie(
                cookieName,
                cookieValue,
                "/",
                Hostname
            ));
        }

        protected virtual void Login()
        {
            // I want the HTML edition
            AddCookie("vm", "Html");

            var values = new NameValueCollection
            {
                {"func", "PSL_LP0_TOP"},
                {"R_ADM", "Admin"},
                {"password", AdminPassword}
            };

            Client.UploadValues(
                GetUri(LoginEndpoint),
                "POST",
                values
            );
        }
    }
}
