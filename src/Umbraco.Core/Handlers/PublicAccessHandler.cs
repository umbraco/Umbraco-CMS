using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Handlers;

public sealed class PublicAccessHandler :
    INotificationHandler<MemberGroupSavedNotification>,
    INotificationHandler<MemberGroupDeletedNotification>
{
    private readonly IPublicAccessService _publicAccessService;

    public PublicAccessHandler(IPublicAccessService publicAccessService) =>
        _publicAccessService = publicAccessService ?? throw new ArgumentNullException(nameof(publicAccessService));

    public void Handle(MemberGroupDeletedNotification notification) => Handle(notification.DeletedEntities);

    public void Handle(MemberGroupSavedNotification notification) => Handle(notification.SavedEntities);

    private void Handle(IEnumerable<IMemberGroup> affectedEntities)
    {
        var keyName = Constants.Entities.AdditionalDataKeys.MemberGroupPreviousName;
        foreach (IMemberGroup grp in affectedEntities)
        {
            // check if the name has changed
            if ((grp.AdditionalData?.ContainsKey(keyName) ?? false)
                && grp.AdditionalData[keyName] != null
                && grp.AdditionalData[keyName]?.ToString().IsNullOrWhiteSpace() == false
                && grp.AdditionalData[keyName]?.ToString() != grp.Name)
            {
                _publicAccessService.RenameMemberGroupRoleRules(
                    grp.AdditionalData[keyName]?.ToString(),
                    grp.Name);
            }
        }
    }
}
