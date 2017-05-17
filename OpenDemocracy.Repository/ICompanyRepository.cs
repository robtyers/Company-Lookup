using System.Collections.Generic;

namespace OpenDemocracy.Repository
{
    public interface ICompanyRepository
    {
        IEnumerable<string> CompanyNames { get; }
    }
}