using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;
using OpenCorporates.Clients;
using OpenCorporates.Models;
using OpenCorporates.Models.Entities;

namespace CompanyIndexer
{
    public class SearchService
    {
        private readonly ICompanySearchClient _searchClient;
        private readonly ICompanyNetworkClient _networkClient;
        private readonly ILog _log;

        private int _errorCount = 0;

        private Dictionary<string, string> Replace { get; } = new Dictionary<string, string>()
        {
            {"LTD", "LIMITED"},
            {"&", "AND"}
        };

        public int Delay { get; set; }
        public int SearchDepth { get; set; }
        public int NetworkConfidence { get; set; }
        public string JurisdictionCode { get; set; }
        public int MaxErrorCount { get; set; }
        public bool ShowInactive { get; set; }

        public SearchService(ICompanySearchClient searchClient, ICompanyNetworkClient networkClient)
        {
            Delay = 1000;
            SearchDepth = 1;
            NetworkConfidence = 80;
            MaxErrorCount = 5;
            ShowInactive = false;

            _searchClient = searchClient;
            _networkClient = networkClient;
        }

        public SearchService(ICompanySearchClient searchClient, ICompanyNetworkClient networkClient, ILog log) : this(searchClient, networkClient)
        {
            _log = log;
        }

        public IEnumerable<ResultFile.OutputRow> ProcessCompanyNames(ICompanyRepository repository)
        {
            var results = new List<ResultFile.OutputRow>();

            foreach (var companyName in repository.CompanyNames)
            {
                Console.WriteLine("");
                Thread.Sleep(Delay); // throttle calls to OpenCorporates

                var cleanCompanyName = 
                    Replace.Aggregate(companyName, (current, entry) => current.Replace(entry.Key, entry.Value));

                try
                {
                    ProcessCompanyName(cleanCompanyName, ref results);
                }
                catch (Exception e)
                {
                    HandleError(e);
                }
            }

            return results;
        }

        private void ProcessCompanyName(string cleanCompanyName, ref List<ResultFile.OutputRow> results)
        {
            _log.Info($"Processing {cleanCompanyName}");
            var companyList = GetCompanies(cleanCompanyName);
            companyList = FilterByJurisdictionCode(companyList);
            companyList = FilterByCurrentStatus(companyList);

            if (companyList.Count == 0)
            {
                var outputRow = new ResultFile.OutputRow(cleanCompanyName, new Company() { Name = "(No match)" });
                results.Add(outputRow);

                _log.Warn("No match.");
                return;
            }

            var searchDepth = SearchDepth > companyList.Count ? companyList.Count : SearchDepth;
            _log.Debug($"Processing Top {searchDepth} of {companyList.Count} matching companies");

            foreach (var cnt in Enumerable.Range(0, searchDepth))
            {
                var company = companyList.ElementAt(cnt).Company;
                var outputRow = new ResultFile.OutputRow(cleanCompanyName, company);

                try
                {
                    ProcessParentCompanies(cleanCompanyName, company, outputRow);
                }
                catch (Exception e)
                {
                    HandleError(e);
                }

                results.Add(outputRow);
            }
        }

        private void HandleError(Exception e)
        {
            _log.Error(e.Message, e);
            _errorCount++;

            if (_errorCount >= MaxErrorCount)
                throw new Exception($"Stopping!  Maximum {MaxErrorCount} reached on this run");
        }

        private List<CompanyListItem> FilterByJurisdictionCode(List<CompanyListItem> companyList)
        {
            if (!string.IsNullOrEmpty(JurisdictionCode))
            {
                companyList = companyList.Where(cl => cl.Company.JurisdictionCode == JurisdictionCode).ToList();
                _log.Debug($"Filtering companies by Jurisdiction Code: '{JurisdictionCode}' (Found {companyList.Count})");
            }

            return companyList;
        }

        private List<CompanyListItem> FilterByCurrentStatus(List<CompanyListItem> companyList)
        {
            const string active = "Active";

            if (!ShowInactive)
            {
                companyList = companyList.Where(cl => cl.Company.CurrentStatus.Equals(active, StringComparison.InvariantCultureIgnoreCase)).ToList();
                _log.Debug($"Filtering companies by Current Status: '{active}' (Found {companyList.Count})");
            }

            return companyList;
        }

        private void ProcessParentCompanies(string cleanCompanyName, Company company, ResultFile.OutputRow outputRow)
        {
            _log.Debug("Checking for parents");
            var networkList = GetCompanyNetwork(company.JurisdictionCode, company.CompanyNumber);

            var parents = networkList
                .Where(nl => !nl.ParentName.Equals(cleanCompanyName))
                .Where(nl => !string.IsNullOrEmpty(nl.ParentName));

            foreach (var statement in parents)
            {
                _log.Debug($"Found {statement.ParentName}");

                outputRow.Parents.Add(
                    new ResultFile.CompanyDetail()
                    {
                        ResolvedName = statement.ParentName,
                        OpenCorporatesUrl = statement.ParentOpencorporatesUrl
                    });
            }
        }

        private List<CompanyListItem> GetCompanies(string companyName)
        {
            var response = _searchClient.GetAsync(companyName).Result;
            _log.Debug($"Total company matches: {response.Results.TotalCount}");

            return response.Results.Companies;
        }

        private IEnumerable<StatementResult> GetCompanyNetwork(string jurisdictionCode, string companyNumber)
        {
            var response = _networkClient.GetAsync(jurisdictionCode, companyNumber, NetworkConfidence).Result;
            _log.Debug($"Total company network matches: {response.Results.Count}");

            return response.Results;
        }
    }
}
