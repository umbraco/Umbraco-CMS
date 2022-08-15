namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface INodeCountRepository
{
    int GetNodeCount(Guid nodeType);

    int GetMediaCount();
}
