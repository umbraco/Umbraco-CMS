using System.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Infrastructure.Services.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache
{
    internal class DistributedCacheHandler :
        INotificationHandler<SavedNotification<IMember>>,
        INotificationHandler<DeletedNotification<IMember>>,
        INotificationHandler<SavedNotification<IUser>>,
        INotificationHandler<DeletedNotification<IUser>>,
        INotificationHandler<SavedNotification<IUserGroup>>,
        INotificationHandler<DeletedNotification<IUserGroup>>,
        INotificationHandler<SavedNotification<PublicAccessEntry>>,
        INotificationHandler<DeletedNotification<PublicAccessEntry>>
    {
        private readonly DistributedCache _distributedCache;

        public DistributedCacheHandler(DistributedCache distributedCache) => _distributedCache = distributedCache;

        public void Handle(SavedNotification<IMember> notification) => _distributedCache.RefreshMemberCache(notification.SavedEntities.ToArray());

        public void Handle(DeletedNotification<IMember> notification) => _distributedCache.RemoveMemberCache(notification.DeletedEntities.ToArray());

        public void Handle(SavedNotification<IUser> notification)
        {
            foreach (var entity in notification.SavedEntities)
            {
                _distributedCache.RefreshUserCache(entity.Id);
            }
        }

        public void Handle(DeletedNotification<IUser> notification)
        {
            foreach (var entity in notification.DeletedEntities)
            {
                _distributedCache.RemoveUserCache(entity.Id);
            }
        }

        public void Handle(SavedNotification<IUserGroup> notification)
        {
            foreach (var entity in notification.SavedEntities)
            {
                _distributedCache.RefreshUserGroupCache(entity.Id);
            }
        }

        public void Handle(DeletedNotification<IUserGroup> notification)
        {
            foreach (var entity in notification.DeletedEntities)
            {
                _distributedCache.RemoveUserGroupCache(entity.Id);
            }
        }

        public void Handle(SavedNotification<PublicAccessEntry> notification) => _distributedCache.RefreshPublicAccess();

        public void Handle(DeletedNotification<PublicAccessEntry> notification) => _distributedCache.RefreshPublicAccess();
    }
}
