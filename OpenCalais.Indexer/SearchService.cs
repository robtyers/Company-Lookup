using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;
using OpenCalais.Clients;
using OpenCalais.Models;
using OpenDemocracy.Repository;

namespace OpenCalais.Indexer
{
    public class SearchService
    {
        private readonly IEntitySearchClient _searchClient;
        private readonly ILog _log;

        private int _errorCount = 0;

        private Dictionary<string, string> Replace { get; } = new Dictionary<string, string>()
        {
            {"LTD", "LIMITED"},
            {"&", "AND"}
        };

        public int Delay { get; set; }
        public int SearchDepth { get; set; }
        public int MaxErrorCount { get; set; }
        
        public SearchService(IEntitySearchClient searchClient, ILog log)
        {
            Delay = 1000;
            
            _searchClient = searchClient;
            _log = log;
        }

        public IEnumerable<ResultFile.OutputRow> ProcessCompanies(ICompanyRepository repository)
        {
            var results = new List<ResultFile.OutputRow>();
            
            foreach (var companyName in repository.CompanyNames)
            {
                Console.WriteLine(string.Empty);
                Thread.Sleep(Delay); // throttle calls to OpenCalais

                var cleanCompanyName = 
                    Replace.Aggregate(companyName, (current, entry) => current.Replace(entry.Key, entry.Value));

                ProcessCompany(cleanCompanyName, ref results);
            }

            return results;
        }

        private void ProcessCompany(string companyName, ref List<ResultFile.OutputRow> results)
        {
            _log.Info($"Processing {companyName}");

            try
            {
                var entityList = GetEntityList(companyName);

                foreach (var entity in entityList)
                {
                    try
                    {
                        results.Add(new ResultFile.OutputRow(companyName, entity));
                    }
                    catch (Exception e)
                    {
                        HandleError(e);

                        var outputRow = new ResultFile.OutputRow(companyName, new NamedEntity() { Name = "(Error)" });
                        results.Add(outputRow);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(e);

                var outputRow = new ResultFile.OutputRow(companyName, new NamedEntity() {Name = "(Error)"});
                results.Add(outputRow);
            }
        }

        private List<NamedEntity> GetEntityList(string companyName)
        {
            var response = _searchClient.GetAsync(companyName).Result;
            _log.Debug($"Total matches: {response.Count}");

            return response;
        }

        private void HandleError(Exception e)
        {
            _log.Error(e.Message, e);
            _errorCount++;

            if (_errorCount >= MaxErrorCount)
                throw new Exception($"Stopping!  Maximum {MaxErrorCount} reached on this run");
        }
    }
}

