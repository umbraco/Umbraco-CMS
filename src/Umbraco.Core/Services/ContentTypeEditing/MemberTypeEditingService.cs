using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

/// <summary>
///     Implementation of <see cref="IMemberTypeEditingService"/> for managing member types.
/// </summary>
/// <remarks>
///     This service handles creating and updating member types including their properties,
///     compositions, and member-specific settings such as property sensitivity and visibility.
/// </remarks>
internal sealed class MemberTypeEditingService : ContentTypeEditingServiceBase<IMemberType, IMemberTypeService, MemberTypePropertyTypeModel, MemberTypePropertyContainerModel>, IMemberTypeEditingService
{
    private readonly IMemberTypeService _memberTypeService;
    private readonly IUserService _userService;
    private readonly IReservedFieldNamesService _reservedFieldNamesService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeEditingService"/> class.
    /// </summary>
    /// <param name="contentTypeService">The content type service for composition validation.</param>
    /// <param name="memberTypeService">The member type service for managing member types.</param>
    /// <param name="dataTypeService">The data type service for validating property data types.</param>
    /// <param name="entityService">The entity service for resolving entity relationships.</param>
    /// <param name="shortStringHelper">The helper for generating safe aliases.</param>
    /// <param name="userService">The user service for validating user permissions.</param>
    /// <param name="reservedFieldNamesService">The service providing reserved field names.</param>
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

    /// <inheritdoc />
    public async Task<Attempt<IMemberType?, ContentTypeOperationStatus>> CreateAsync(MemberTypeCreateModel model, Guid userKey)
    {
        Attempt<IMemberType?, ContentTypeOperationStatus> result = await ValidateAndMapForCreationAsync(model, model.Key, model.ContainerKey);
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<IEnumerable<ContentTypeAvailableCompositionsResult>> GetAvailableCompositionsAsync(
        Guid? key,
        IEnumerable<Guid> currentCompositeKeys,
        IEnumerable<string> currentPropertyAliases) =>
        await FindAvailableCompositionsAsync(key, currentCompositeKeys, currentPropertyAliases);

    /// <inheritdoc />
    protected override IMemberType CreateContentType(IShortStringHelper shortStringHelper, int parentId)
        => new MemberType(shortStringHelper, parentId);

    /// <inheritdoc />
    protected override bool SupportsPublishing => false;

    /// <inheritdoc />
    protected override UmbracoObjectTypes ContentTypeObjectType => UmbracoObjectTypes.MemberType;

    /// <inheritdoc />
    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.MemberTypeContainer;

    /// <inheritdoc />
    protected override ISet<string> GetReservedFieldNames() => _reservedFieldNamesService.GetMemberReservedFieldNames();

    /// <summary>
    ///     Updates the visibility settings for member type properties.
    /// </summary>
    /// <param name="memberType">The member type to update.</param>
    /// <param name="model">The model containing property visibility settings.</param>
    private void UpdatePropertyTypeVisibility(IMemberType memberType, MemberTypeModelBase model)
    {
        foreach (MemberTypePropertyTypeModel propertyType in model.Properties)
        {
            memberType.SetMemberCanViewProperty(propertyType.Alias, propertyType.MemberCanView);
            memberType.SetMemberCanEditProperty(propertyType.Alias, propertyType.MemberCanEdit);
        }
    }

    /// <summary>
    ///     Updates the sensitivity settings for member type properties.
    /// </summary>
    /// <param name="memberType">The member type to update.</param>
    /// <param name="model">The model containing property sensitivity settings.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>
    ///     <c>true</c> if the sensitivity settings were updated successfully;
    ///     <c>false</c> if the user does not have permission to change sensitivity settings.
    /// </returns>
    /// <remarks>
    ///     Only users with access to sensitive data can modify the sensitivity flag on properties.
    /// </remarks>
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
