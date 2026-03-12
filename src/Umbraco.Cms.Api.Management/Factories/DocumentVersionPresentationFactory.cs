using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentVersionPresentationFactory : ContentVersionPresentationFactoryBase<DocumentVersionItemResponseModel>, IDocumentVersionPresentationFactory
{

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Factories.DocumentVersionPresentationFactory"/> class.
    /// </summary>
    /// <param name="entityService">Service used to interact with entities within the Umbraco CMS.</param>
    /// <param name="userIdKeyResolver">Resolves user ID keys for mapping or identification purposes.</param>
    public DocumentVersionPresentationFactory(IEntityService entityService, IUserIdKeyResolver userIdKeyResolver)
        : base(entityService, userIdKeyResolver)

    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Document;

    /// <summary>
    /// Asynchronously creates a <see cref="Umbraco.Cms.Api.Management.Models.DocumentVersionItemResponseModel"/> instance from the specified <see cref="Umbraco.Cms.Core.Models.ContentVersionMeta"/>.
    /// </summary>
    /// <param name="contentVersion">The content version metadata from which to construct the response model.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains the constructed <see cref="Umbraco.Cms.Api.Management.Models.DocumentVersionItemResponseModel"/> representing the provided content version.
    /// </returns>


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
