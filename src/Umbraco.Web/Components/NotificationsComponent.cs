using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web._Legacy.Actions;

namespace Umbraco.Web.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class NotificationsComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        public void Initialize(INotificationService notificationService)
        {
            ContentService.SentToPublish += (sender, args) =>
                notificationService.SendNotification(args.Entity, ActionToPublish.Instance);

            //Send notifications for the published action
            ContentService.Published += (sender, args) =>
            {
                foreach (var content in args.PublishedEntities)
                    notificationService.SendNotification(content, ActionPublish.Instance);
            };

            //Send notifications for the update and created actions
            ContentService.Saved += (sender, args) =>
            {
                var newEntities = new List<IContent>();
                var updatedEntities =  new List<IContent>();

                //need to determine if this is updating or if it is new
                foreach (var entity in args.SavedEntities)
                {
                    var dirty = (IRememberBeingDirty) entity;
                    if (dirty.WasPropertyDirty("Id"))
                    {
                        //it's new
                        newEntities.Add(entity);
                    }
                    else
                    {
                        //it's updating
                        updatedEntities.Add(entity);
                    }
                }
                notificationService.SendNotification(newEntities, ActionNew.Instance);
                notificationService.SendNotification(updatedEntities, ActionUpdate.Instance);
            };

            //Send notifications for the delete action
            ContentService.Deleted += (sender, args) =>
            {
                foreach (var content in args.DeletedEntities)
                    notificationService.SendNotification(content, ActionDelete.Instance);
            };

            //Send notifications for the unpublish action
            ContentService.Unpublished += (sender, args) =>
            {
                foreach (var content in args.PublishedEntities)
                    notificationService.SendNotification(content, ActionUnpublish.Instance);
            };
        }
    }
}
