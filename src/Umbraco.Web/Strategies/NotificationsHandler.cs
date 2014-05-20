using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
using umbraco;
using umbraco.BusinessLogic.Actions;

namespace Umbraco.Web.Strategies
{
    /// <summary>
    /// Subscribes to the relavent events in order to send out notifications
    /// </summary>
    public sealed class NotificationsHandler : ApplicationEventHandler
    {

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarted(umbracoApplication, applicationContext);

            ContentService.SentToPublish += (sender, args) =>
                applicationContext.Services.NotificationService.SendNotification(
                    args.Entity, ActionToPublish.Instance, applicationContext);

            //Send notifications for the published action
            ContentService.Published += (sender, args) =>
                                        args.PublishedEntities.ForEach(
                                            content =>
                                            applicationContext.Services.NotificationService.SendNotification(
                                                content, ActionPublish.Instance, applicationContext));

            //Send notifications for the update and created actions
            ContentService.Saved += (sender, args) =>
                {
                    //need to determine if this is updating or if it is new
                    foreach (var entity in args.SavedEntities)
                    {
                        var dirty = (IRememberBeingDirty) entity;
                        if (dirty.WasPropertyDirty("Id"))
                        {
                            //it's new
                            applicationContext.Services.NotificationService.SendNotification(
                                entity, ActionNew.Instance, applicationContext);
                        }
                        else
                        {
                            //it's updating
                            applicationContext.Services.NotificationService.SendNotification(
                                entity, ActionUpdate.Instance, applicationContext);
                        }
                    }
                };

            //Send notifications for the delete action
            ContentService.Deleted += (sender, args) =>
                                      args.DeletedEntities.ForEach(
                                          content =>
                                          applicationContext.Services.NotificationService.SendNotification(
                                              content, ActionDelete.Instance, applicationContext));
           
            //Send notifications for the unpublish action
            ContentService.UnPublished += (sender, args) =>
                                          args.PublishedEntities.ForEach(
                                              content =>
                                              applicationContext.Services.NotificationService.SendNotification(
                                                  content, ActionUnPublish.Instance, applicationContext));

        }

    }
}
