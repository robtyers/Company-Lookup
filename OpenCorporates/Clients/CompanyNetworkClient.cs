using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenCorporates.Models;

namespace OpenCorporates.Clients
{
    public class CompanyNetworkClient : BaseClient, ICompanyNetworkClient
    {
        // {0} = Jurisdiction Code {1} = Company Number
        private const string QueryString = "/companies/{0}/{1}/network";

        public CompanyNetworkClient(HttpClient client, string apiKey) : base(client, apiKey)
        {

        }

        public async Task<StatementResponse> GetAsync(string jurisdiction, string companyNumber, int confidence = 80)
        {
            if (string.IsNullOrEmpty(jurisdiction))
                throw new ArgumentException("Argument cannot be empty", nameof(jurisdiction));

            if (string.IsNullOrEmpty(companyNumber))
                throw new ArgumentException("Argument cannot be empty", nameof(companyNumber));

            if (confidence < 0 ||confidence > 100)
                throw new ArgumentException("Confidence must be a number between 0 and 100", nameof(confidence));
            var queryString = GetQueryString(jurisdiction, companyNumber, confidence);
            var result = string.Empty;
            var cnt = 0;

            do
            {
                try
                {
                    var response = await Client.GetAsync(queryString);
                    response.EnsureSuccessStatusCode();
                    result = await response.Content.ReadAsStringAsync();
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Thread.Sleep(3000);
                    cnt++;
                }
            } while (cnt < 3);
            
            return JsonConvert.DeserializeObject<StatementResponse>(result, SerializerSettings);
        }

        private string GetQueryString(string jurisdiction, string companyNumber, int confidence)
        {
            var queryString = string.Format(QueryString, jurisdiction, companyNumber, confidence);

            if (!string.IsNullOrEmpty(ApiKey))
                queryString = $"{queryString}&amp;{ApiParam}{ApiKey}";

            return queryString;
        }
    }
}
