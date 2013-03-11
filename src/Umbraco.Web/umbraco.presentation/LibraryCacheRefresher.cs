using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.interfaces;
using DeleteEventArgs = umbraco.cms.businesslogic.DeleteEventArgs;

namespace umbraco
{
	/// <summary>
	/// Special class made to listen to save events on objects where umbraco.library caches some of their objects
	/// </summary>
	public class LibraryCacheRefresher : IApplicationStartupHandler
	{
		public LibraryCacheRefresher()
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

        static void MediaServiceTrashing(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            library.ClearLibraryCacheForMedia(e.Entity.Id);
        }

        static void MediaServiceMoving(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            library.ClearLibraryCacheForMedia(e.Entity.Id);
        }

        static void MediaServiceDeleting(IMediaService sender, DeleteEventArgs<IMedia> e)
        {
            foreach (var item in e.DeletedEntities)
            {
                library.ClearLibraryCacheForMedia(item.Id);
            }
        }

        static void MediaServiceSaved(IMediaService sender, SaveEventArgs<IMedia> e)
        {
            foreach (var item in e.SavedEntities)
            {
                library.ClearLibraryCacheForMedia(item.Id);
            }
        }

        static void MemberBeforeDelete(Member sender, DeleteEventArgs e)
        {
            library.ClearLibraryCacheForMember(sender.Id);
        }

        static void MemberAfterSave(Member sender, SaveEventArgs e)
        {
            library.ClearLibraryCacheForMember(sender.Id);
        }
	}
}