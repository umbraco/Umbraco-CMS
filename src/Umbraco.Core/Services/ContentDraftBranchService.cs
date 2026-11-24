using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc cref="IContentDraftBranchService" />
public class ContentDraftBranchService : ContentDraftBranchServiceBase<IContent, IDocumentRepository>, IContentDraftBranchService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentDraftBranchService" /> class.
    /// </summary>
    public ContentDraftBranchService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IContentDraftBranchRepository contentDraftBranchRepository,
        IContentService contentService,
        IDocumentRepository documentRepository,
        IUserIdKeyResolver userIdKeyResolver,
        IUserService userService,
        ILanguageService languageService,
        IOptionsMonitor<ContentSettings> contentSettingsMonitor)
        : base(provider, loggerFactory, eventMessagesFactory, contentDraftBranchRepository, contentService, documentRepository, userIdKeyResolver, userService, languageService, contentSettingsMonitor)
    {
    }
}
