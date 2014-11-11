using System;
using System.Net;

namespace FailedPrintJobDeleter
{
    public class CookieWebClient : WebClient
    {
        private readonly CookieContainer _container = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            var webRequest = base.GetWebRequest(address);
            var httpRequest = webRequest as HttpWebRequest;
            if (httpRequest != null)
            {
                httpRequest.CookieContainer = _container;
            }
            return webRequest;
        }
    }
}
