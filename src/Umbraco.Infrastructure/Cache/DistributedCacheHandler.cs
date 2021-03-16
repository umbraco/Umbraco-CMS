using System.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Infrastructure.Services.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache
{
    internal class DistributedCacheHandler :
        INotificationHandler<MemberSavedNotification>,
        INotificationHandler<MemberDeletedNotification>,
        INotificationHandler<UserSavedNotification>,
        INotificationHandler<UserDeletedNotification>,
        INotificationHandler<UserGroupSavedNotification>,
        INotificationHandler<UserGroupDeletedNotification>,
        INotificationHandler<PublicAccessEntrySavedNotification>,
        INotificationHandler<PublicAccessEntryDeletedNotification>
    {
        private readonly DistributedCache _distributedCache;

        public DistributedCacheHandler(DistributedCache distributedCache) => _distributedCache = distributedCache;

        public void Handle(MemberSavedNotification notification) => _distributedCache.RefreshMemberCache(notification.SavedEntities.ToArray());

        public void Handle(MemberDeletedNotification notification) => _distributedCache.RemoveMemberCache(notification.DeletedEntities.ToArray());

        public void Handle(UserSavedNotification notification)
        {
            foreach (var entity in notification.SavedEntities)
            {
                _distributedCache.RefreshUserCache(entity.Id);
            }
        }

        public void Handle(UserDeletedNotification notification)
        {
            foreach (var entity in notification.DeletedEntities)
            {
                _distributedCache.RemoveUserCache(entity.Id);
            }
        }

        public void Handle(UserGroupSavedNotification notification)
        {
            foreach (var entity in notification.SavedEntities)
            {
                _distributedCache.RefreshUserGroupCache(entity.Id);
            }
        }

        public void Handle(UserGroupDeletedNotification notification)
        {
            foreach (var entity in notification.DeletedEntities)
            {
                _distributedCache.RemoveUserGroupCache(entity.Id);
            }
        }

        public void Handle(PublicAccessEntrySavedNotification notification) => _distributedCache.RefreshPublicAccess();

        public void Handle(PublicAccessEntryDeletedNotification notification) => _distributedCache.RefreshPublicAccess();
    }
}
