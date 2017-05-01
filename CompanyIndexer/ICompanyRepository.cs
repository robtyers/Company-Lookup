using System.Collections.Generic;

namespace CompanyIndexer
{
    public interface ICompanyRepository
    {
        IEnumerable<string> CompanyNames { get; }
    }
}