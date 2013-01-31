using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface ITemplateRepository : IRepositoryQueryable<int, ITemplate>
    {
        ITemplate Get(string alias);
        IEnumerable<ITemplate> GetAll(params string[] aliases);
    }
}