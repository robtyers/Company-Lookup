using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenCalais.Models;

namespace OpenCalais.Clients
{
    public class EntitySearchClient : IEntitySearchClient, IDisposable
    {
        private readonly HttpClient _client;
        private const string BaseAddress = "https://api.thomsonreuters.com/permid/calais";

        public EntitySearchClient(HttpClient client, string apiKey)
        {
            _client = client;
            _client.DefaultRequestHeaders.Add("x-ag-access-token", apiKey);
            _client.DefaultRequestHeaders.Add("outputFormat", "application/json");
            _client.DefaultRequestHeaders.Add("x-calais-language", "English");
        }

        /// <summary>
        /// Return Named Entities
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public async Task<List<NamedEntity>> GetAsync(string term)
        {
            var content = new StringContent(term);
            content.Headers.ContentType = new MediaTypeHeaderValue("text/raw");
            var uri = new Uri(BaseAddress);

            var response = await _client.PostAsync(uri, content);

            if (response.IsSuccessStatusCode)
            {
                var calaisResult = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(calaisResult);
                var results = new List<NamedEntity>();

                foreach (var token in json.Children())
                {
                    try
                    {
                        var typeGroup = GetTypeGroup(token);
                        if (typeGroup == "entities")
                        {
                            results.Add(new NamedEntity
                            {
                                Category = GetCategory(token),
                                Match = GetName(token),
                                ConfidenceLevel = GetConfidenceLevel(token),
                                Name = GetResolvedName(token),
                                CommonName = GetResolvedCommonName(token),
                                OpenCalaisPermId = GetOpenCalaisPermId(token)
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                return results;
            }

            throw new HttpRequestException(response.StatusCode.ToString());
        }

        private static string GetTypeGroup(JToken token)
        {
            try
            {
                return (string) token.First["_typeGroup"];
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string GetCategory(JToken token)
        {
                try
                {
                    return (string) token.First["_type"];
                }
                catch
                {
                    return string.Empty;
                }
            }

        private static string GetOpenCalaisPermId(JToken token)
        {
            try
            {
                return (string) token.FirstOrDefault()["resolutions"].FirstOrDefault()["id"];
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string GetResolvedCommonName(JToken token)
        {
            try
            {
                return (string) token.FirstOrDefault()["resolutions"].FirstOrDefault()["commonname"];
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string GetResolvedName(JToken token)
        {
            try
            {
                return (string) token.FirstOrDefault()["resolutions"].FirstOrDefault()["name"];
            }
            catch
            {
                return string.Empty;
            }
        }

        private static float GetConfidenceLevel(JToken token)
        {
            try
            {
                return (float) (token.FirstOrDefault()["confidencelevel"]);
            }
            catch
            {
                return float.MinValue;
            }
        }

        private static string GetName(JToken token)
        {
            var match = string.Empty;

            try
            {
                match = (string) token.First["name"];
            }
            catch
            {
            }
            return match;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
