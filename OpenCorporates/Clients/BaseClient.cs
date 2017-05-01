using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using OpenCorporates.Json;

namespace OpenCorporates.Clients
{
    public abstract class BaseClient
    {
        private const string BaseAddress = "https://api.opencorporates.com";
        protected const string ApiParam = "api_token=";

        protected readonly HttpClient Client;
        protected readonly string ApiKey;
        protected readonly JsonSerializerSettings SerializerSettings;

        protected BaseClient(HttpClient client, string apiKey)
        {
            ApiKey = apiKey;
            Client = client;
            Client.BaseAddress = new Uri(BaseAddress);
            
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            SerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new SnakeCasePropertyNamesContractResolver()
            };
        }
    }
}
