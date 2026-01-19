using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentVersionPresentationFactory : ContentVersionPresentationFactoryBase<DocumentVersionItemResponseModel>, IDocumentVersionPresentationFactory
{
    public DocumentVersionPresentationFactory(IEntityService entityService, IUserIdKeyResolver userIdKeyResolver)
        : base(entityService, userIdKeyResolver)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Document;

    protected override DocumentVersionItemResponseModel VersionItemResponseModelFactory(
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
