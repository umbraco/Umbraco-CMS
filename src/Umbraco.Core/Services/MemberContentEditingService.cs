using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides services for editing member content in Umbraco.
/// </summary>
/// <remarks>
///     This service handles member content editing operations including validation, updating,
///     and deleting members. It extends the base content editing service with member-specific
///     functionality such as handling sensitive property access control.
/// </remarks>
internal sealed class MemberContentEditingService
    : ContentEditingServiceBase<IMember, IMemberType, IMemberService, IMemberTypeService>, IMemberContentEditingService
{
    private readonly ILogger<ContentEditingServiceBase<IMember, IMemberType, IMemberService, IMemberTypeService>> _logger;
    private readonly IUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberContentEditingService" /> class.
    /// </summary>
    /// <param name="contentService">The member service for member operations.</param>
    /// <param name="contentTypeService">The member type service for member type operations.</param>
    /// <param name="propertyEditorCollection">The collection of property editors.</param>
    /// <param name="dataTypeService">The data type service for data type operations.</param>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="scopeProvider">The core scope provider for managing database transactions.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user keys to IDs.</param>
    /// <param name="memberValidationService">The service for validating member content.</param>
    /// <param name="userService">The user service for user operations.</param>
    /// <param name="optionsMonitor">The options monitor for content settings.</param>
    /// <param name="relationService">The relation service for managing content relations.</param>
    /// <param name="contentTypeFilters">The collection of content type filters.</param>
    public MemberContentEditingService(
        IMemberService contentService,
        IMemberTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ILogger<ContentEditingServiceBase<IMember, IMemberType, IMemberService, IMemberTypeService>> logger,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver,
        IMemberValidationService memberValidationService,
        IUserService userService,
        IOptionsMonitor<ContentSettings> optionsMonitor,
        IRelationService relationService,
        ContentTypeFilterCollection contentTypeFilters)
        : base(contentService, contentTypeService, propertyEditorCollection, dataTypeService, logger, scopeProvider, userIdKeyResolver, memberValidationService, optionsMonitor, relationService, contentTypeFilters)
    {
        _logger = logger;
        _userService = userService;
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateAsync(MemberEditingModelBase editingModel, Guid memberTypeKey)
        => await ValidatePropertiesAsync(editingModel, memberTypeKey);

    /// <inheritdoc />
    public async Task<Attempt<MemberUpdateResult, ContentEditingOperationStatus>> UpdateAsync(IMember member, MemberEditingModelBase updateModel, Guid userKey)
    {
        IMemberType memberType = await ContentTypeService.GetAsync(member.ContentType.Key)
                                 ?? throw new InvalidOperationException($"The member type {member.ContentType.Alias} could not be found.");

        IUser user = await _userService.GetRequiredUserAsync(userKey);

        if (ValidateAccessToSensitiveProperties(member, memberType, updateModel, user) is false)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.NotAllowed, new MemberUpdateResult());
        }

        // store any sensitive properties that must be explicitly carried over between updates (due to missing user access to sensitive data)
        Dictionary<string, object?> sensitivePropertiesToRetain = FindSensitivePropertiesToRetain(member, memberType, user);

        Attempt<MemberUpdateResult, ContentEditingOperationStatus> result = await MapUpdate<MemberUpdateResult>(member, updateModel);
        if (result.Success == false)
        {
            return Attempt.FailWithStatus(result.Status, result.Result);
        }

        // restore the retained sensitive properties after the base update (they have been removed from the member at this point)
        RetainSensitiveProperties(member, sensitivePropertiesToRetain);

        // the create mapping might succeed, but this doesn't mean the model is valid at property level.
        // we'll return the actual property validation status if the entire operation succeeds.
        ContentEditingOperationStatus validationStatus = result.Status;
        ContentValidationResult validationResult = result.Result.ValidationResult;

        var currentUserId = await GetUserIdAsync(userKey);
        ContentEditingOperationStatus operationStatus = Save(member, currentUserId);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(validationStatus, new MemberUpdateResult { Content = member, ValidationResult = validationResult })
            : Attempt.FailWithStatus(operationStatus, new MemberUpdateResult { Content = member });
    }

    /// <inheritdoc />
    public async Task<Attempt<IMember?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey)
        => await HandleDeleteAsync(key, userKey, false);

    /// <inheritdoc />
    protected override IMember New(string? name, int parentId, IMemberType memberType)
        => throw new NotSupportedException("Member creation is not supported by this service. This should never be called.");

    /// <inheritdoc />
    protected override OperationResult? Move(IMember member, int newParentId, int userId)
        => throw new InvalidOperationException("Move is not supported for members");

    /// <inheritdoc />
    protected override IMember? Copy(IMember member, int newParentId, bool relateToOriginal, bool includeDescendants, int userId)
        => throw new NotSupportedException("Copy is not supported for Member");

    /// <inheritdoc />
    protected override OperationResult? MoveToRecycleBin(IMember member, int userId)
        => throw new InvalidOperationException("Recycle bin is not supported for members");

    /// <inheritdoc />
    protected override OperationResult? Delete(IMember member, int userId)
        => ContentService.Delete(member, userId).Result;

    /// <summary>
    ///     Saves the specified member with the given user ID.
    /// </summary>
    /// <param name="member">The member to save.</param>
    /// <param name="userId">The ID of the user performing the save operation.</param>
    /// <returns>
    ///     The operation status indicating success or failure of the save operation.
    /// </returns>
    private ContentEditingOperationStatus Save(IMember member, int userId)
    {
        try
        {
            Attempt<OperationResult?> saveResult = ContentService.Save(member, userId);
            return saveResult.Result?.Result switch
            {
                // these are the only result states currently expected from Save
                OperationResultType.Success => ContentEditingOperationStatus.Success,
                OperationResultType.FailedCancelledByEvent => ContentEditingOperationStatus.CancelledByNotification,

                // for any other state we'll return "unknown" so we know that we need to amend this
                _ => ContentEditingOperationStatus.Unknown
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Member save operation failed");
            return ContentEditingOperationStatus.Unknown;
        }
    }

    /// <summary>
    ///     Validates that the user has access to modify the sensitive properties included in the update model.
    /// </summary>
    /// <param name="member">The member being updated.</param>
    /// <param name="memberType">The member type defining the property structure.</param>
    /// <param name="updateModel">The update model containing properties to be modified.</param>
    /// <param name="user">The user performing the update.</param>
    /// <returns>
    ///     <c>true</c> if the user has access to all sensitive properties in the update model or has access to sensitive data;
    ///     otherwise, <c>false</c> if the user is attempting to modify sensitive properties without proper access.
    /// </returns>
    private bool ValidateAccessToSensitiveProperties(IMember member, IMemberType memberType, MemberEditingModelBase updateModel, IUser user)
    {
        if (user.HasAccessToSensitiveData())
        {
            return true;
        }

        var sensitivePropertyAliases = memberType.GetSensitivePropertyTypeAliases().ToArray();
        return updateModel
            .Properties
            .Select(property => property.Alias)
            .Intersect(sensitivePropertyAliases, StringComparer.OrdinalIgnoreCase)
            .Any() is false;
    }

    /// <summary>
    ///     Finds the sensitive property values that need to be retained during an update for users without sensitive data access.
    /// </summary>
    /// <param name="member">The member whose sensitive properties should be retained.</param>
    /// <param name="memberType">The member type defining which properties are sensitive.</param>
    /// <param name="user">The user performing the update.</param>
    /// <returns>
    ///     A dictionary mapping property aliases to their current values for sensitive properties,
    ///     or an empty dictionary if the user has access to sensitive data.
    /// </returns>
    private Dictionary<string, object?> FindSensitivePropertiesToRetain(IMember member, IMemberType memberType, IUser user)
    {
        if (user.HasAccessToSensitiveData())
        {
            return new Dictionary<string, object?>();
        }

        var sensitivePropertyAliases = memberType.GetSensitivePropertyTypeAliases().ToArray();
        // NOTE: this is explicitly NOT handling variance. if variance becomes a thing for members, this needs to be amended.
        return sensitivePropertyAliases.ToDictionary(alias => alias, alias => member.GetValue(alias));
    }

    /// <summary>
    ///     Restores the retained sensitive property values to the member after a base update operation.
    /// </summary>
    /// <param name="member">The member to restore sensitive properties on.</param>
    /// <param name="sensitivePropertyValues">The dictionary of sensitive property values to restore.</param>
    private void RetainSensitiveProperties(IMember member, Dictionary<string, object?> sensitivePropertyValues)
    {
        foreach (KeyValuePair<string, object?> sensitiveProperty in sensitivePropertyValues)
        {
            // NOTE: this is explicitly NOT handling variance. if variance becomes a thing for members, this needs to be amended.
            member.SetValue(sensitiveProperty.Key, sensitiveProperty.Value);
        }
    }
}
