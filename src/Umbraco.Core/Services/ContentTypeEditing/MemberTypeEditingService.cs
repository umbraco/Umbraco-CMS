using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

internal sealed class MemberTypeEditingService : ContentTypeEditingServiceBase<IMemberType, IMemberTypeService, MemberTypePropertyTypeModel, MemberTypePropertyContainerModel>, IMemberTypeEditingService
{
    private readonly IMemberTypeService _memberTypeService;
    private readonly IUserService _userService;
    private readonly IReservedFieldNamesService _reservedFieldNamesService;

    public MemberTypeEditingService(
        IContentTypeService contentTypeService,
        IMemberTypeService memberTypeService,
        IDataTypeService dataTypeService,
        IEntityService entityService,
        IShortStringHelper shortStringHelper,
        IUserService userService,
        IReservedFieldNamesService reservedFieldNamesService)
        : base(contentTypeService, memberTypeService, dataTypeService, entityService, shortStringHelper)
    {
        _memberTypeService = memberTypeService;
        _userService = userService;
        _reservedFieldNamesService = reservedFieldNamesService;
    }

    [Obsolete("Use the non obsolete constructor instead. Scheduled for removal in v16")]
    public MemberTypeEditingService(
        IContentTypeService contentTypeService,
        IMemberTypeService memberTypeService,
        IDataTypeService dataTypeService,
        IEntityService entityService,
        IShortStringHelper shortStringHelper,
        IUserService userService)
        : base(contentTypeService, memberTypeService, dataTypeService, entityService, shortStringHelper)
    {
        _memberTypeService = memberTypeService;
        _userService = userService;
        _reservedFieldNamesService = StaticServiceProvider.Instance.GetRequiredService<IReservedFieldNamesService>();
    }

    public async Task<Attempt<IMemberType?, ContentTypeOperationStatus>> CreateAsync(MemberTypeCreateModel model, Guid userKey)
    {
        Attempt<IMemberType?, ContentTypeOperationStatus> result = await ValidateAndMapForCreationAsync(model, model.Key, containerKey: null);
        if (result.Success is false)
        {
            return result;
        }

        IMemberType memberType = result.Result ?? throw new InvalidOperationException($"{nameof(ValidateAndMapForCreationAsync)} succeeded but did not yield any result");
        if (await UpdatePropertyTypeSensitivity(memberType, model, userKey) is false)
        {
            return Attempt.FailWithStatus<IMemberType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.NotAllowed, memberType);
        }

        UpdatePropertyTypeVisibility(memberType, model);

        await _memberTypeService.SaveAsync(memberType, userKey);
        return result;
    }

    public async Task<Attempt<IMemberType?, ContentTypeOperationStatus>> UpdateAsync(IMemberType memberType, MemberTypeUpdateModel model, Guid userKey)
    {
        Attempt<IMemberType?, ContentTypeOperationStatus> result = await ValidateAndMapForUpdateAsync(memberType, model);
        if (result.Success is false)
        {
            return result;
        }

        memberType = result.Result ?? throw new InvalidOperationException($"{nameof(ValidateAndMapForUpdateAsync)} succeeded but did not yield any result");
        if (await UpdatePropertyTypeSensitivity(memberType, model, userKey) is false)
        {
            return Attempt.FailWithStatus<IMemberType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.NotAllowed, memberType);
        }

        UpdatePropertyTypeVisibility(memberType, model);

        await _memberTypeService.SaveAsync(memberType, userKey);
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

    protected override ISet<string> GetReservedFieldNames() => _reservedFieldNamesService.GetMemberReservedFieldNames();

    private void UpdatePropertyTypeVisibility(IMemberType memberType, MemberTypeModelBase model)
    {
        foreach (MemberTypePropertyTypeModel propertyType in model.Properties)
        {
            memberType.SetMemberCanViewProperty(propertyType.Alias, propertyType.MemberCanView);
            memberType.SetMemberCanEditProperty(propertyType.Alias, propertyType.MemberCanEdit);
        }
    }

    private async Task<bool> UpdatePropertyTypeSensitivity(IMemberType memberType, MemberTypeModelBase model, Guid userKey)
    {
        IUser user = await _userService.GetAsync(userKey)
                     ?? throw new ArgumentException($"Could not find a user with the specified user key ({userKey})", nameof(userKey));

        var canChangeSensitivity = user.HasAccessToSensitiveData();

        foreach (MemberTypePropertyTypeModel propertyType in model.Properties)
        {
            var changed = memberType.IsSensitiveProperty(propertyType.Alias) != propertyType.IsSensitive;
            if (changed && canChangeSensitivity is false)
            {
                return false;
            }

            memberType.SetIsSensitiveProperty(propertyType.Alias, propertyType.IsSensitive);
        }

        return true;
    }
}
