using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

internal sealed class ElementPublishingService : ContentPublishingServiceBase<IElement, IElementService>, IElementPublishingService
{
    public ElementPublishingService(
        ICoreScopeProvider coreScopeProvider,
        IElementService contentService,
        IUserIdKeyResolver userIdKeyResolver,
        IContentValidationService contentValidationService,
        IContentTypeService contentTypeService,
        ILanguageService languageService,
        IOptionsMonitor<ContentSettings> optionsMonitor,
        IRelationService relationService)
        : base(
            coreScopeProvider,
            contentService,
            userIdKeyResolver,
            contentValidationService,
            contentTypeService,
            languageService,
            optionsMonitor,
            relationService)
    {
    }

    protected override int WriteLockId => Constants.Locks.ElementTree;
}
