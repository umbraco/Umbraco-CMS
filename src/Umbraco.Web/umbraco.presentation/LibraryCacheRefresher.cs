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
				Member.AfterSave += MemberAfterSave;
				Member.BeforeDelete += MemberBeforeDelete;
				Media.AfterSave += MediaAfterSave;
				Media.BeforeDelete += MediaBeforeDelete;
                //we need to do this before the move so that we still have the item's current path
                //in order to invalidate the media cache. Pretty sure this was why the BeforeDelete was
                //occuring as well which must have been before we had a recycle bin for media.
                //see : http://issues.umbraco.org/issue/U4-1653
                CMSNode.BeforeMove += MediaBeforeMove;
			}
		}

	    static void MediaBeforeMove(object sender, MoveEventArgs e)
        {
            if (!(sender is Media))
                return;

            library.ClearLibraryCacheForMedia(((Media) sender).Id);
        }

	    static void MemberBeforeDelete(Member sender, DeleteEventArgs e)
		{
			library.ClearLibraryCacheForMember(sender.Id);
		}

	    static void MediaBeforeDelete(Media sender, DeleteEventArgs e)
		{
			library.ClearLibraryCacheForMedia(sender.Id);
		}

	    static void MediaAfterSave(Media sender, SaveEventArgs e)
		{
			library.ClearLibraryCacheForMedia(sender.Id);
		}

	    static void MemberAfterSave(Member sender, SaveEventArgs e)
		{
			library.ClearLibraryCacheForMember(sender.Id);
		}
	}
}