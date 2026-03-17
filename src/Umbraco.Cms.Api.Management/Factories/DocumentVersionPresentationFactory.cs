using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentVersionPresentationFactory : IDocumentVersionPresentationFactory
{
    private readonly IEntityService _entityService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Factories.DocumentVersionPresentationFactory"/> class.
    /// </summary>
    /// <param name="entityService">Service used to interact with entities within the Umbraco CMS.</param>
    /// <param name="userIdKeyResolver">Resolves user ID keys for mapping or identification purposes.</param>
    public DocumentVersionPresentationFactory(
        IEntityService entityService,
        IUserIdKeyResolver userIdKeyResolver)
    {
        _entityService = entityService;
        _userIdKeyResolver = userIdKeyResolver;
    }

    /// <summary>
    /// Asynchronously creates a <see cref="Umbraco.Cms.Api.Management.Models.DocumentVersionItemResponseModel"/> instance from the specified <see cref="Umbraco.Cms.Core.Models.ContentVersionMeta"/>.
    /// </summary>
    /// <param name="contentVersion">The content version metadata from which to construct the response model.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains the constructed <see cref="Umbraco.Cms.Api.Management.Models.DocumentVersionItemResponseModel"/> representing the provided content version.
    /// </returns>
    public async Task<DocumentVersionItemResponseModel> CreateAsync(ContentVersionMeta contentVersion)
    {
        ReferenceByIdModel userReference = contentVersion.UserId switch
        {
            Constants.Security.UnknownUserId => new ReferenceByIdModel(),
            _ => new ReferenceByIdModel(await _userIdKeyResolver.GetAsync(contentVersion.UserId)),
        };

        return new DocumentVersionItemResponseModel(
            contentVersion.VersionId.ToGuid(), // this is a magic guid since versions do not have keys in the DB
            new ReferenceByIdModel(_entityService.GetKey(contentVersion.ContentId, UmbracoObjectTypes.Document).Result),
            new ReferenceByIdModel(
                _entityService.GetKey(contentVersion.ContentTypeId, UmbracoObjectTypes.DocumentType)
                    .Result),
            userReference,
            new DateTimeOffset(contentVersion.VersionDate),
            contentVersion.CurrentPublishedVersion,
            contentVersion.CurrentDraftVersion,
            contentVersion.PreventCleanup);
    }

    /// <summary>
    /// Creates multiple document version item response models asynchronously from the given content versions.
    /// </summary>
    /// <param name="contentVersions">The collection of content version metadata to create response models for.</param>
    /// <returns>A task representing the asynchronous operation, containing an enumerable of document version item response models.</returns>
    public async Task<IEnumerable<DocumentVersionItemResponseModel>> CreateMultipleAsync(IEnumerable<ContentVersionMeta> contentVersions)
        => await CreateMultipleImplAsync(contentVersions).ToArrayAsync();

    private async IAsyncEnumerable<DocumentVersionItemResponseModel> CreateMultipleImplAsync(IEnumerable<ContentVersionMeta> contentVersions)
    {
        foreach (ContentVersionMeta contentVersion in contentVersions)
        {
            yield return await CreateAsync(contentVersion);
        }
    }
}
