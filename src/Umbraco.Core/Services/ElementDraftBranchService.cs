using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc cref="IElementDraftBranchService" />
public class ElementDraftBranchService : ContentDraftBranchServiceBase<IElement, IElementRepository>, IElementDraftBranchService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementDraftBranchService" /> class.
    /// </summary>
    public ElementDraftBranchService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IContentDraftBranchRepository contentDraftBranchRepository,
        IElementService elementService,
        IElementRepository elementRepository,
        IUserIdKeyResolver userIdKeyResolver,
        IUserService userService,
        ILanguageService languageService,
        IOptionsMonitor<ContentSettings> contentSettingsMonitor)
        : base(provider, loggerFactory, eventMessagesFactory, contentDraftBranchRepository, elementService, elementRepository, userIdKeyResolver, userService, languageService, contentSettingsMonitor)
    {
    }
}
