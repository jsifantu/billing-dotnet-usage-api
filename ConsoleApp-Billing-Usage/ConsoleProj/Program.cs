using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net;
using System.IO;
using System.Linq.Expressions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System.Configuration;

namespace ARMAPI_Test
{
    //#error Please update the appSettings section in app.config, then remove this statement

    class Program
    {
        //This is a sample console application that shows you how to grab a token from AAD for the current user of the app, and then get usage data for the customer with that token.
        //The same caveat remains, that the current user of the app needs to be part of either the Owner, Reader or Contributor role for the requested AzureSubID.
        static void Main(string[] args)
        {
            try {

                //Get the AAD token to get authorized to make the call to the Usage API
                string token;
                var tokenFilePath = "..\\..\\..\\..\\oathtoken.txt";
                if (!File.Exists(tokenFilePath)) {
                    token = GetOAuthTokenFromAAD();
                    using (var writer = new StreamWriter(tokenFilePath)) {
                        writer.WriteLine(token);
                    }
                } else {
                    using (var reader = new StreamReader(tokenFilePath)) {
                        token = reader.ReadToEnd();
                    }
                }

                /*Setup API call to Usage API
                    Callouts:
                    * See the App.config file for all AppSettings key/value pairs
                    * You can get a list of offer numbers from this URL: http://azure.microsoft.com/en-us/support/legal/offer-details/
                    * See the Azure Usage API specification for more details on the query parameters for this API.
                    * The Usage Service/API is currently in preview; please use 2016-05-01-preview for api-version
                    * Please see the readme if you are having problems configuring or authenticating: https://github.com/Azure-Samples/billing-dotnet-usage-api

                */
                var startDate = ConfigurationManager.AppSettings["StartDate"];
                var endDate = ConfigurationManager.AppSettings["EndDate"];
                ProcessRequest(token, startDate, endDate);
            } catch (Exception e) {
                Console.WriteLine(String.Format("{0} \n\n{1}", e.Message, e.InnerException != null ? e.InnerException.Message : ""));
            }
            Console.ReadLine();
        }

        private static void ProcessRequest(string token, string startDate, string endDate)
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
            var filePath = Environment.CurrentDirectory + "..\\..\\..\\..\\BillingUsage.json";
            // Pipes the stream to a higher level stream reader with the required encoding format. 
            using (var readStream = new StreamReader(receiveStream, Encoding.UTF8)) {
                var usageResponse = readStream.ReadToEnd();
                Console.WriteLine("Usage stream received.");
                Console.WriteLine("Writing to JSON file " + filePath);
                using (var writer = new StreamWriter(filePath)) {
                    writer.WriteLine(usageResponse);
                }
                Console.WriteLine("JSON output complete.");
                filePath = Environment.CurrentDirectory + "..\\..\\..\\..\\BillingUsage.csv";
                Console.WriteLine("Writing to CSV file " + filePath);
                UsagePayload payLoad = JsonConvert.DeserializeObject<UsagePayload>(usageResponse);
                using (var writer = new StreamWriter(filePath)) {
                    payLoad.ToCSV(writer);
                }
            }

#if SHOW_OUPUT
                // Convert the Stream to a strongly typed RateCardPayload object.  
                // You can also walk through this object to manipulate the individuals member objects. 
                UsagePayload payload = JsonConvert.DeserializeObject<UsagePayload>(usageResponse);
                Console.WriteLine(usageResponse.ToString());
                response.Close();
                readStream.Close();
                Console.WriteLine("JSON output complete.  Press ENTER to close.");
                Console.ReadLine();
#endif
            receiveStream.Close();
            response.Close();
        }

        public static string GetOAuthTokenFromAAD()
        {
            var authenticationContext = new AuthenticationContext(String.Format("{0}/{1}",
                                                                    ConfigurationManager.AppSettings["ADALServiceURL"],
                                                                    ConfigurationManager.AppSettings["TenantDomain"]));

            //Ask the logged in user to authenticate, so that this client app can get a token on his behalf
            var result = authenticationContext.AcquireToken(String.Format("{0}/", ConfigurationManager.AppSettings["ARMBillingServiceURL"]),
                                                            ConfigurationManager.AppSettings["ClientID"],
                                                            new Uri(ConfigurationManager.AppSettings["ADALRedirectURL"]),
                                                            PromptBehavior.Always);

            if (result == null) {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            return result.AccessToken;
        }

    }
}
