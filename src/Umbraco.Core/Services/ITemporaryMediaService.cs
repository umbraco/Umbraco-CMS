using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

[Obsolete($"This service has been superseded by {nameof(IMediaImportService)}. Will be removed in V16.")]
public interface ITemporaryMediaService
{
    public IMedia Save(string temporaryLocation, Guid? startNode, string? mediaTypeAlias);
}
