using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;
using OpenCorporates.Clients;
using OpenCorporates.Models;
using OpenCorporates.Models.Entities;
using OpenDemocracy.Repository;

namespace OpenCorporates.Indexer
{
    public class SearchService
    {
        private readonly ICompanySearchClient _searchClient;
        private readonly ICompanyNetworkClient _networkClient;
        private readonly ICompanyDetailClient _detailClient;
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
        public int MaxRecursion { get; set; }
        public bool ShowInactive { get; set; }

        public SearchService(
            ICompanySearchClient searchClient, ICompanyNetworkClient networkClient, ICompanyDetailClient detailClient, ILog log)
        {
            Delay = 1000;
            SearchDepth = 1;
            NetworkConfidence = 80;
            MaxErrorCount = 5;
            MaxRecursion = 5;
            ShowInactive = false;

            _searchClient = searchClient;
            _networkClient = networkClient;
            _detailClient = detailClient;
            _log = log;
        }

        public IEnumerable<ResultFile.OutputRow> ProcessCompanies(ICompanyRepository repository)
        {
            var results = new List<ResultFile.OutputRow>();

            foreach (var companyName in repository.CompanyNames)
            {
                Console.WriteLine(string.Empty);
                Thread.Sleep(Delay); // throttle calls to OpenCorporates

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
                var companyList = GetCompanyList(companyName);
                companyList = FilterByJurisdictionCode(companyList);
                companyList = FilterByCurrentStatus(companyList);

                if (companyList.Any())
                {
                    var searchDepth = SearchDepth > companyList.Count ? companyList.Count : SearchDepth;
                    _log.Debug($"Processing Top {searchDepth} of {companyList.Count} matching companies");

                    foreach (var cnt in Enumerable.Range(0, searchDepth))
                    {
                        var company = companyList.ElementAt(cnt).Company;
                        var outputRow = new ResultFile.OutputRow(companyName, company);

                        var detail = GetCompanyDetail(company);
                        detail.Company.IndustryCodes.ForEach(
                            i => outputRow.IndustryCodes += $"{i.IndustryCode.Code} ({i.IndustryCode.Description}); ");
                        try
                        {
                            var parent = GetParentCompany(companyName, company.JurisdictionCode, company.CompanyNumber);
                            outputRow.ParentCompanyName =
                                (parent == outputRow.ResolvedCompanyName || parent == companyName)
                                    ? string.Empty
                                    : parent;

                        }
                        catch (Exception e)
                        {
                            _log.Error(e.Message, e);
                            outputRow.ParentCompanyName = "(error)";
                        }

                        results.Add(outputRow);
                    }
                }
                else
                {
                    _log.Warn("No match.");

                    var outputRow = new ResultFile.OutputRow(companyName, new Company() { Name = string.Empty});
                    results.Add(outputRow);
                }
            }
            catch (Exception e)
            {
                HandleError(e);

                var outputRow = new ResultFile.OutputRow(companyName, new Company() {Name = "(Error)"});
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
            if (string.IsNullOrEmpty(JurisdictionCode))
                return companyList;

            companyList = companyList.Where(cl => cl.Company.JurisdictionCode == JurisdictionCode).ToList();
            _log.Debug($"Filtering companies by Jurisdiction Code: '{JurisdictionCode}' (Found {companyList.Count})");

            return companyList;
        }

        private List<CompanyListItem> FilterByCurrentStatus(List<CompanyListItem> companyList)
        {
            const string active = "Active";

            if (ShowInactive)
                return companyList;

            companyList = companyList.Where(cl => cl.Company.CurrentStatus.Equals(active, StringComparison.InvariantCultureIgnoreCase)).ToList();
            _log.Debug($"Filtering companies by Current Status: '{active}' (Found {companyList.Count})");

            return companyList;
        }
        
        private string GetParentCompany(string companyName, string jurisdictionCode, string companyNumber, int cnt = 0)
        {
            _log.Debug($"Checking for parents for {companyName}");
            var networkList = GetCompanyNetwork(jurisdictionCode, companyNumber);

            var parent = networkList
                .FirstOrDefault(nl => !nl.ParentName.Equals(companyName));
                
            if (parent == null)
                return companyName;

            cnt++;
            if (cnt == MaxRecursion)
                return companyName;

            var url = parent.ParentOpencorporatesUrl.Split('/').Reverse().Take(2).Reverse().ToList();
            return GetParentCompany(parent.ParentName, url[0], url[1], cnt);
        }
        
        private List<CompanyListItem> GetCompanyList(string companyName)
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

        private CompanyResult GetCompanyDetail(Company company)
        {
            var response = _detailClient.GetAsync(company.JurisdictionCode, company.CompanyNumber).Result;
            _log.Debug($"Total industry type matches: {response.Results.Company.IndustryCodes.Count}");

            return response.Results;
        }
    }
}

