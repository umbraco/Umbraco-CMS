using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class ElementVersionPresentationFactory : ContentVersionPresentationFactoryBase<ElementVersionItemResponseModel>, IElementVersionPresentationFactory
{
    public ElementVersionPresentationFactory(IEntityService entityService, IUserIdKeyResolver userIdKeyResolver)
        : base(entityService, userIdKeyResolver)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Element;

    protected override ElementVersionItemResponseModel VersionItemResponseModelFactory(
        Guid versionId,
        ReferenceByIdModel item,
        ReferenceByIdModel documentType,
        ReferenceByIdModel user,
        DateTimeOffset versionDate,
        bool isCurrentPublishedVersion,
        bool isCurrentDraftVersion,
        bool preventCleanup)
        => new(versionId, item, documentType, user, versionDate, isCurrentPublishedVersion, isCurrentDraftVersion, preventCleanup);
}
