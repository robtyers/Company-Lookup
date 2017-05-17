using System.Collections.Generic;
using System.Threading.Tasks;
using OpenCalais.Models;

namespace OpenCalais.Clients
{
    public interface IEntitySearchClient
    {
        /// <summary>
        /// Return Named Entities
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        Task<List<NamedEntity>> GetAsync(string term);
    }
}