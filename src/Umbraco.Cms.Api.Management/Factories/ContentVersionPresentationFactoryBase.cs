using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

internal abstract class ContentVersionPresentationFactoryBase<TVersionItemResponseModel>
{
    private readonly IEntityService _entityService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    protected abstract UmbracoObjectTypes ItemObjectType { get; }

    protected ContentVersionPresentationFactoryBase(IEntityService entityService, IUserIdKeyResolver userIdKeyResolver)
    {
        _entityService = entityService;
        _userIdKeyResolver = userIdKeyResolver;
    }

    protected abstract TVersionItemResponseModel VersionItemResponseModelFactory(
        Guid versionId,
        ReferenceByIdModel item,
        ReferenceByIdModel documentType,
        ReferenceByIdModel user,
        DateTimeOffset versionDate,
        bool isCurrentPublishedVersion,
        bool isCurrentDraftVersion,
        bool preventCleanup);

    public async Task<TVersionItemResponseModel> CreateAsync(ContentVersionMeta contentVersion)
    {
        ReferenceByIdModel userReference = contentVersion.UserId switch
        {
            Constants.Security.UnknownUserId => new ReferenceByIdModel(),
            _ => new ReferenceByIdModel(await _userIdKeyResolver.GetAsync(contentVersion.UserId)),
        };

        return VersionItemResponseModelFactory(
            contentVersion.VersionId.ToGuid(), // this is a magic guid since versions do not have keys in the DB
            new ReferenceByIdModel(_entityService.GetKey(contentVersion.ContentId, ItemObjectType).Result),
            new ReferenceByIdModel(
                _entityService.GetKey(contentVersion.ContentTypeId, UmbracoObjectTypes.DocumentType)
                    .Result),
            userReference,
            new DateTimeOffset(contentVersion.VersionDate),
            contentVersion.CurrentPublishedVersion,
            contentVersion.CurrentDraftVersion,
            contentVersion.PreventCleanup);
    }

    public async Task<IEnumerable<TVersionItemResponseModel>> CreateMultipleAsync(IEnumerable<ContentVersionMeta> contentVersions)
        => await CreateMultipleImplAsync(contentVersions).ToArrayAsync();

    private async IAsyncEnumerable<TVersionItemResponseModel> CreateMultipleImplAsync(IEnumerable<ContentVersionMeta> contentVersions)
    {
        foreach (ContentVersionMeta contentVersion in contentVersions)
        {
            yield return await CreateAsync(contentVersion);
        }
    }
}
