namespace Umbraco.Cms.Core.Services;

public interface INodeCountService
{
    int GetNodeCount(Guid nodeType);

    int GetMediaCount();
}
