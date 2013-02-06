using Umbraco.Core;
using Umbraco.Core.Services;
using umbraco;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.macro;
using umbraco.cms.businesslogic.member;
using System.Linq;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Class which listens to events on business level objects in order to invalidate the cache amongst servers when data changes
    /// </summary>
    public class CacheRefresherEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            if (UmbracoSettings.UmbracoLibraryCacheDuration <= 0) return;

            //Bind to macro events

            Macro.AfterSave += MacroAfterSave;

            //Bind to member events

            Member.AfterSave += MemberAfterSave;
            Member.BeforeDelete += MemberBeforeDelete;

            //Bind to media events

            MediaService.Saved += MediaServiceSaved;
            //We need to perform all of the 'before' events here because we need a reference to the
            //media item's Path before it is moved/deleting/trashed
            //see: http://issues.umbraco.org/issue/U4-1653
            MediaService.Deleting += MediaServiceDeleting;
            MediaService.Moving += MediaServiceMoving;
            MediaService.Trashing += MediaServiceTrashing;
        }

        /// <summary>
        /// Flush macro from cache
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void MacroAfterSave(Macro sender, SaveEventArgs e)
        {				
            DistributedCache.Instance.RefreshMacroCache(sender.Id);
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
            DistributedCache.Instance.RefreshMemberCache(sender.Id);            
        }

        static void MemberAfterSave(Member sender, SaveEventArgs e)
        {
            DistributedCache.Instance.RefreshMemberCache(sender.Id);
        }
    }
}