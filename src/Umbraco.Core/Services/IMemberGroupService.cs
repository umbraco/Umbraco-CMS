using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IMemberGroupService : IService
{
    IEnumerable<IMemberGroup> GetAll();

    IMemberGroup? GetById(int id);

    IMemberGroup? GetById(Guid id);

    IEnumerable<IMemberGroup> GetByIds(IEnumerable<int> ids);

    IMemberGroup? GetByName(string? name);

    void Save(IMemberGroup memberGroup);

    void Delete(IMemberGroup memberGroup);
}
