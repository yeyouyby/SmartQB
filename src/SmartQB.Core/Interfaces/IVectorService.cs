using SmartQB.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartQB.Core.Interfaces;

public interface IVectorService
{
    Task<List<Question>> SearchSimilarAsync(string query, int limit = 10);
}
