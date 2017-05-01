using System.Collections.Generic;
using System.IO;

namespace CompanyIndexer
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly string _fileName;
        
        public IEnumerable<string> CompanyNames => File.ReadLines(_fileName);

        public CompanyRepository(string fileName)
        {
            _fileName = fileName;
        }
    }
}
