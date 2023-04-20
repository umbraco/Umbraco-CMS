using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

public class StylesheetService : FileServiceBase, IStylesheetService
{
    private readonly IStylesheetRepository _stylesheetRepository;

    protected override string[] AllowedFileExtensions { get; } = { ".css" };

    public StylesheetService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IStylesheetRepository stylesheetRepository)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _stylesheetRepository = stylesheetRepository;
    }

    /// <inheritdoc />
    public Task<IStylesheet?> GetAsync(string path)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IStylesheet? stylesheet = _stylesheetRepository.Get(path);

        scope.Complete();
        return Task.FromResult(stylesheet);
    }
}
