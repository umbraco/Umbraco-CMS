using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Handlers;

public sealed class PublicAccessHandler :
    INotificationAsyncHandler<MemberGroupSavingNotification>,
    INotificationHandler<MemberGroupSavedNotification>,
    INotificationAsyncHandler<MemberGroupDeletingNotification>,
    INotificationHandler<MemberGroupDeletedNotification>
{
    private readonly IPublicAccessService _publicAccessService;
    private readonly IMemberGroupService _memberGroupService;

    public PublicAccessHandler(IPublicAccessService publicAccessService, IMemberGroupService memberGroupService)
    {
        _publicAccessService = publicAccessService;
        _memberGroupService = memberGroupService;
    }

    public async Task HandleAsync(MemberGroupSavingNotification notification, CancellationToken cancellationToken)
        => await SaveStateAsync(notification.SavedEntities, notification);

    public async Task HandleAsync(MemberGroupDeletingNotification notification, CancellationToken cancellationToken)
        => await SaveStateAsync(notification.DeletedEntities, notification);

    public void Handle(MemberGroupDeletedNotification notification) => Handle(notification.DeletedEntities, notification);

    public void Handle(MemberGroupSavedNotification notification) => Handle(notification.SavedEntities, notification);

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

    private static string StateKey(IMemberGroup memberGroup) => $"{nameof(PublicAccessHandler)}:{memberGroup.Key}";
}
