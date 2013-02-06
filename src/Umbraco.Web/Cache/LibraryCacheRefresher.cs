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
            //We need to perform all of the 'before' events here because we need a reference to the
            //media item's Path before it is moved/deleting/trashed
            //see: http://issues.umbraco.org/issue/U4-1653
            MediaService.Deleting += MediaServiceDeleting;
            MediaService.Moving += MediaServiceMoving;
            MediaService.Trashing += MediaServiceTrashing;
        }

        static void MediaServiceTrashing(IMediaService sender, Core.Events.MoveEventArgs<Core.Models.IMedia> e)
        {
            ApplicationContext.Current.ApplicationCache.ClearLibraryCacheForMedia(e.Entity.Id);
        }

        static void MediaServiceMoving(IMediaService sender, Core.Events.MoveEventArgs<Core.Models.IMedia> e)
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