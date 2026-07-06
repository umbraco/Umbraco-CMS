using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Factory for creating element version presentation models from content version metadata.
/// </summary>
internal sealed class ElementVersionPresentationFactory : ContentVersionPresentationFactoryBase<ElementVersionItemResponseModel>, IElementVersionPresentationFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementVersionPresentationFactory"/> class.
    /// </summary>
    /// <param name="entityService">Service used to interact with entities within the Umbraco CMS.</param>
    /// <param name="userIdKeyResolver">Resolves user ID keys for mapping or identification purposes.</param>
    public ElementVersionPresentationFactory(IEntityService entityService, IUserIdKeyResolver userIdKeyResolver)
        : base(entityService, userIdKeyResolver)
    {
    }

    /// <inheritdoc />
    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Element;

    /// <inheritdoc />
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
