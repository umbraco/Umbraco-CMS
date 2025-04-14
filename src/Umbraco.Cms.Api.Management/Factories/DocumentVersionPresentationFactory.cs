using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentVersionPresentationFactory : IDocumentVersionPresentationFactory
{
    private readonly IEntityService _entityService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    public DocumentVersionPresentationFactory(
        IEntityService entityService,
        IUserIdKeyResolver userIdKeyResolver)
    {
        _entityService = entityService;
        _userIdKeyResolver = userIdKeyResolver;
    }

    public async Task<DocumentVersionItemResponseModel> CreateAsync(ContentVersionMeta contentVersion) =>
        new(
            contentVersion.VersionId.ToGuid(), // this is a magic guid since versions do not have keys in the DB
            new ReferenceByIdModel(_entityService.GetKey(contentVersion.ContentId, UmbracoObjectTypes.Document).Result),
            new ReferenceByIdModel(_entityService.GetKey(contentVersion.ContentTypeId, UmbracoObjectTypes.DocumentType)
                .Result),
            new ReferenceByIdModel(await _userIdKeyResolver.GetAsync(contentVersion.UserId)),
            new DateTimeOffset(contentVersion.VersionDate),
            contentVersion.CurrentPublishedVersion,
            contentVersion.CurrentDraftVersion,
            contentVersion.PreventCleanup);

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
