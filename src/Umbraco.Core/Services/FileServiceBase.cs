using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public abstract class FileServiceBase : RepositoryService
{
    public FileServiceBase(ICoreScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory) : base(provider, loggerFactory, eventMessagesFactory)
    {
    }

    protected abstract string[] AllowedFileExtensions { get; }

    protected virtual bool HasValidFileExtension(string fileName)
        => AllowedFileExtensions.Contains(Path.GetExtension(fileName));

    protected virtual bool HasValidFileName(string fileName)
    {
        if (fileName.ContainsAny(Path.GetInvalidFileNameChars()))
        {
            return false;
        }

        return true;
    }
}
