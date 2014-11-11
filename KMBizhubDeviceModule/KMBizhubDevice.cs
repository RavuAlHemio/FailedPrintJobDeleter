﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using FailedPrintJobDeleter;

namespace KMBizhubDeviceModule
{
    /// <summary>
    /// Device access class supporting Konica Minolta bizhub devices.
    /// </summary>
    public abstract class KMBizhubDevice : IPrinterDevice
    {
        /// <summary>
        /// The endpoint to which to post login requests.
        /// </summary>
        public const string LoginEndpoint = "/wcd/ulogin.cgi";

        /// <summary>
        /// The endpoint at which to receive active (including failed) jobs.
        /// </summary>
        public const string DeleteJobEndpoint = "/wcd/user.cgi";

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
        }

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
        /// Logs into the printer's user mode as admin.
        /// </summary>
        protected virtual void Login()
        {
            var values = new NameValueCollection
            {
                {"func", "PSL_LP0_TOP"},
                {"R_ADM", "Admin"},
                {"password", AdminPassword},
                {"Mode", "Admin"},
                {"ViewMode", "Html"},
                {"BrowseMode", "Low"}
            };

            Client.UploadValues(
                GetUri(LoginEndpoint),
                "POST",
                values
            );
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

        public abstract IEnumerable<string> GetFailedJobIDs();

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
    }
}