using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.interfaces;

namespace umbraco
{
	/// <summary>
	/// Special class made to listen to save events on objects where umbraco.library caches some of their objects
	/// </summary>
	public class LibraryCacheRefresher : IApplicationStartupHandler
	{
		public LibraryCacheRefresher()
		{
			if (UmbracoSettings.UmbracoLibraryCacheDuration > 0)
			{
				Member.AfterSave += new Member.SaveEventHandler(Member_AfterSave);
				Member.BeforeDelete += new Member.DeleteEventHandler(Member_BeforeDelete);
				Media.AfterSave += new Media.SaveEventHandler(Media_AfterSave);
				Media.BeforeDelete += new Media.DeleteEventHandler(Media_BeforeDelete);
			}

			// now handled directly by the IRoutesCache implementation
			//content.AfterUpdateDocumentCache += new content.DocumentCacheEventHandler(content_AfterUpdateDocumentCache);
			//content.AfterRefreshContent += new content.RefreshContentEventHandler(content_AfterRefreshContent);
		}

		//void content_AfterRefreshContent(Document sender, RefreshContentEventArgs e)
		//{
		//    library.ClearNiceUrlCache();
		//}

		//void content_AfterUpdateDocumentCache(Document sender, DocumentCacheEventArgs e)
		//{
		//    library.ClearNiceUrlCache();
		//}

		void Member_BeforeDelete(Member sender, DeleteEventArgs e)
		{
			library.ClearLibraryCacheForMember(sender.Id);
		}

		void Media_BeforeDelete(Media sender, DeleteEventArgs e)
		{
			library.ClearLibraryCacheForMedia(sender.Id);
		}

		void Media_AfterSave(Media sender, SaveEventArgs e)
		{
			library.ClearLibraryCacheForMedia(sender.Id);
		}

		void Member_AfterSave(Member sender, SaveEventArgs e)
		{
			library.ClearLibraryCacheForMember(sender.Id);
		}
	}
}