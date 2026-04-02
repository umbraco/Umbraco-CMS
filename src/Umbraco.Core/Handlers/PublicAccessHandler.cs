using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Handlers;

/// <summary>
///     Handles public access rule updates when member groups are renamed or deleted.
/// </summary>
/// <remarks>
///     <para>
///         This handler monitors member group saving and deleting notifications to ensure that
///         public access rules referencing member groups are updated when group names change.
///     </para>
///     <para>
///         When a member group is renamed, this handler updates all public access rules that
///         reference the group's previous name to use the new name.
///     </para>
/// </remarks>
public sealed class PublicAccessHandler :
    INotificationAsyncHandler<MemberGroupSavingNotification>,
    INotificationHandler<MemberGroupSavedNotification>,
    INotificationAsyncHandler<MemberGroupDeletingNotification>,
    INotificationHandler<MemberGroupDeletedNotification>
{
    private readonly IPublicAccessService _publicAccessService;
    private readonly IMemberGroupService _memberGroupService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublicAccessHandler"/> class.
    /// </summary>
    /// <param name="publicAccessService">The public access service for managing access rules.</param>
    /// <param name="memberGroupService">The member group service.</param>
    public PublicAccessHandler(IPublicAccessService publicAccessService, IMemberGroupService memberGroupService)
    {
        _publicAccessService = publicAccessService;
        _memberGroupService = memberGroupService;
    }

    /// <inheritdoc />
    public async Task HandleAsync(MemberGroupSavingNotification notification, CancellationToken cancellationToken)
        => await SaveStateAsync(notification.SavedEntities, notification);

    /// <inheritdoc />
    public async Task HandleAsync(MemberGroupDeletingNotification notification, CancellationToken cancellationToken)
        => await SaveStateAsync(notification.DeletedEntities, notification);

    /// <inheritdoc />
    public void Handle(MemberGroupDeletedNotification notification) => Handle(notification.DeletedEntities, notification);

    /// <inheritdoc />
    public void Handle(MemberGroupSavedNotification notification) => Handle(notification.SavedEntities, notification);

    /// <summary>
    ///     Saves the current member group names to the notification state for later comparison.
    /// </summary>
    /// <param name="affectedEntities">The member groups being affected by the operation.</param>
    /// <param name="notification">The notification containing the state dictionary.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SaveStateAsync(IEnumerable<IMemberGroup> affectedEntities, IStatefulNotification notification)
    {
        foreach (IMemberGroup memberGroup in affectedEntities)
        {
            // store the current group name in the notification state
            var currentMemberGroupName = (await _memberGroupService.GetAsync(memberGroup.Key))?.Name;
            if (currentMemberGroupName.IsNullOrWhiteSpace())
            {
                continue;
            }

            notification.State[StateKey(memberGroup)] = currentMemberGroupName;
        }
    }

    /// <summary>
    ///     Handles the post-save or post-delete notification by updating public access rules if group names changed.
    /// </summary>
    /// <param name="affectedEntities">The member groups that were affected.</param>
    /// <param name="notification">The notification containing the saved state.</param>
    private void Handle(IEnumerable<IMemberGroup> affectedEntities, IStatefulNotification notification)
    {
        foreach (IMemberGroup grp in affectedEntities)
        {
            // check if the group name has changed and update group based rules accordingly
            if (notification.State.TryGetValue(StateKey(grp), out var stateValue) is false
                || stateValue is not string previousName
                || previousName.IsNullOrWhiteSpace()
                || previousName == grp.Name)
            {
                continue;
            }

            _publicAccessService.RenameMemberGroupRoleRules(previousName, grp.Name);
        }
    }

    /// <summary>
    ///     Generates a unique state key for tracking member group state changes.
    /// </summary>
    /// <param name="memberGroup">The member group to generate a key for.</param>
    /// <returns>A unique key string for the member group.</returns>
    private static string StateKey(IMemberGroup memberGroup) => $"{nameof(PublicAccessHandler)}:{memberGroup.Key}";
}
