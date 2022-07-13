using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface ITemplateRepository : IReadWriteQueryRepository<int, ITemplate>, IFileRepository
{
    ITemplate? Get(string? alias);

    IEnumerable<ITemplate> GetAll(params string[] aliases);

    IEnumerable<ITemplate> GetChildren(int masterTemplateId);

    IEnumerable<ITemplate> GetDescendants(int masterTemplateId);
}
