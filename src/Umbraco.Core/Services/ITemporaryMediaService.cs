using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface ITemporaryMediaService
{
    public IMedia Save(string temporaryLocation, Guid? startNode, string? mediaTypeAlias);
}
