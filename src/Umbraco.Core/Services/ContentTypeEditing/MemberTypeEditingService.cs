using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

internal sealed class MemberTypeEditingService : ContentTypeEditingServiceBase<IMemberType, IMemberTypeService, MemberTypePropertyTypeModel, MemberTypePropertyContainerModel>, IMemberTypeEditingService
{
    private readonly IMemberTypeService _memberTypeService;

    public MemberTypeEditingService(
        IContentTypeService contentTypeService,
        IMemberTypeService memberTypeService,
        IDataTypeService dataTypeService,
        IEntityService entityService,
        IShortStringHelper shortStringHelper)
        : base(contentTypeService, memberTypeService, dataTypeService, entityService, shortStringHelper)
        => _memberTypeService = memberTypeService;

    public async Task<Attempt<IMemberType?, ContentTypeOperationStatus>> CreateAsync(MemberTypeCreateModel model, Guid userKey)
    {
        Attempt<IMemberType?, ContentTypeOperationStatus> result = await ValidateAndMapForCreationAsync(model, model.Key, containerKey: null);
        if (result.Success)
        {
            IMemberType memberType = result.Result ?? throw new InvalidOperationException($"{nameof(ValidateAndMapForCreationAsync)} succeeded but did not yield any result");
            await _memberTypeService.SaveAsync(memberType, userKey);
        }

        return result;
    }

    public async Task<Attempt<IMemberType?, ContentTypeOperationStatus>> UpdateAsync(IMemberType memberType, MemberTypeUpdateModel model, Guid userKey)
    {
        Attempt<IMemberType?, ContentTypeOperationStatus> result = await ValidateAndMapForUpdateAsync(memberType, model);
        if (result.Success)
        {
            memberType = result.Result ?? throw new InvalidOperationException($"{nameof(ValidateAndMapForUpdateAsync)} succeeded but did not yield any result");
            await _memberTypeService.SaveAsync(memberType, userKey);
        }

        return result;
    }

    public async Task<IEnumerable<ContentTypeAvailableCompositionsResult>> GetAvailableCompositionsAsync(
        Guid? key,
        IEnumerable<Guid> currentCompositeKeys,
        IEnumerable<string> currentPropertyAliases) =>
        await FindAvailableCompositionsAsync(key, currentCompositeKeys, currentPropertyAliases);

    protected override IMemberType CreateContentType(IShortStringHelper shortStringHelper, int parentId)
        => new MemberType(shortStringHelper, parentId);

    protected override bool SupportsPublishing => false;

    protected override UmbracoObjectTypes ContentTypeObjectType => UmbracoObjectTypes.MemberType;

    protected override UmbracoObjectTypes ContainerObjectType => throw new NotSupportedException("Member type tree does not support containers");
}
