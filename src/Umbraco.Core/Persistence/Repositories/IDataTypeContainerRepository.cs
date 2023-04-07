namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IDataTypeContainerRepository : IEntityContainerRepository
{

    bool HasDuplicateName(Guid parentKey, string name);
    bool HasDuplicateName(int parentId, string name);
}
