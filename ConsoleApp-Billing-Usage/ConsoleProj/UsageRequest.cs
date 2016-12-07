using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Configuration;
using System.IO;

namespace ARMAPI_Test
{
    public class UsageRequest
    {
        public void Execute(string token, string startDate, string endDate)
        {
            // Build up the HttpWebRequest
            string url = string.Format("providers/Microsoft.Commerce/UsageAggregates?api-version=2015-06-01-preview&reportedstartTime={0}+00%3a00%3a00Z&reportedEndTime={1}+00%3a00%3a00Z",
                startDate, endDate);
            string requestURL = String.Format("{0}/{1}/{2}/{3}",
                       ConfigurationManager.AppSettings["ARMBillingServiceURL"],
                       "subscriptions",
                       ConfigurationManager.AppSettings["SubscriptionID"],
                       url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestURL);

            // Add the OAuth Authorization header, and Content Type header
            request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);
            request.ContentType = "application/json";

            // Call the Usage API, dump the output to the console window
            // Call the REST endpoint
            Console.WriteLine("Calling Usage service...");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Console.WriteLine(String.Format("Usage service response status: {0}", response.StatusDescription));
            Stream receiveStream = response.GetResponseStream();
        }
    }
}
