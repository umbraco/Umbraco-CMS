using Umbraco.Core;
using Umbraco.Core.Services;
using umbraco;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using System.Linq;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Special class made to listen to save events on objects where umbraco.library caches some of their objects
    /// </summary>
    public class LibraryCacheRefresher : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            if (UmbracoSettings.UmbracoLibraryCacheDuration <= 0) return;


            Member.AfterSave += MemberAfterSave;
            Member.BeforeDelete += MemberBeforeDelete;

            MediaService.Saved += MediaServiceSaved;
            MediaService.Deleting += MediaServiceDeleting;
            MediaService.Moved += MediaServiceMoved;
            MediaService.Trashed += MediaService_Trashed;
        }

        static void MediaService_Trashed(IMediaService sender, Core.Events.MoveEventArgs<Core.Models.IMedia> e)
        {
            ApplicationContext.Current.ApplicationCache.ClearLibraryCacheForMedia(e.Entity.Id);
        }

        static void MediaServiceMoved(IMediaService sender, Core.Events.MoveEventArgs<Core.Models.IMedia> e)
        {
            ApplicationContext.Current.ApplicationCache.ClearLibraryCacheForMedia(e.Entity.Id);            
        }

        static void MediaServiceDeleting(IMediaService sender, Core.Events.DeleteEventArgs<Core.Models.IMedia> e)
        {
            foreach (var item in e.DeletedEntities)
            {
                ApplicationContext.Current.ApplicationCache.ClearLibraryCacheForMedia(item.Id);
            }
        }

        //NOTE: this should not need to be done, the SavedCollection event shouldn't even exist!! :(
        static void MediaServiceSavedCollection(IMediaService sender, Core.Events.SaveEventArgs<System.Collections.Generic.IEnumerable<Core.Models.IMedia>> e)
        {            
            foreach (var item in e.SavedEntities.SelectMany(x => x))
            {
                ApplicationContext.Current.ApplicationCache.ClearLibraryCacheForMedia(item.Id);
            }
        }

        static void MediaServiceSaved(IMediaService sender, Core.Events.SaveEventArgs<Core.Models.IMedia> e)
        {
            foreach (var item in e.SavedEntities)
            {
                ApplicationContext.Current.ApplicationCache.ClearLibraryCacheForMedia(item.Id);
            }
        }

        static void MemberBeforeDelete(Member sender, DeleteEventArgs e)
        {
            ApplicationContext.Current.ApplicationCache.ClearLibraryCacheForMember(sender.Id);
        }

        static void MemberAfterSave(Member sender, SaveEventArgs e)
        {
            ApplicationContext.Current.ApplicationCache.ClearLibraryCacheForMember(sender.Id);
        }
    }
}