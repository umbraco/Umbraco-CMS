using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using System.Linq;
using Macro = umbraco.cms.businesslogic.macro.Macro;
using Template = umbraco.cms.businesslogic.template.Template;

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

            //Bind to content events - currently used for:
            // - macro clearing
            // - clearing the xslt cache (MS.Internal.Xml.XPath.XPathSelectionIterator)
            //NOTE: These are 'special' event handlers that will only clear cache for items on the current server
            // that is because this event will fire based on a distributed cache call, meaning this event fires on 
            // all servers based on the distributed cache call for updating content.
            content.AfterUpdateDocumentCache += ContentAfterUpdateDocumentCache;
            content.AfterClearDocumentCache += ContentAfterClearDocumentCache;

            //Bind to content type events

            ContentTypeService.SavedContentType += ContentTypeServiceSavedContentType;
            ContentTypeService.SavedMediaType += ContentTypeServiceSavedMediaType;

            //Bind to user events

            User.Saving += UserSaving;
            User.Deleting += UserDeleting;

            //Bind to template events

            Template.AfterSave += TemplateAfterSave;
            Template.AfterDelete += TemplateAfterDelete;

            //Bind to macro events

            Macro.AfterSave += MacroAfterSave;
            Macro.AfterDelete += MacroAfterDelete;

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
        /// Fires when a media type is saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ContentTypeServiceSavedMediaType(IContentTypeService sender, Core.Events.SaveEventArgs<IMediaType> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RemoveMediaTypeCache(x));
        }

        /// <summary>
        /// Fires when a content type is saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ContentTypeServiceSavedContentType(IContentTypeService sender, Core.Events.SaveEventArgs<IContentType> e)
        {           
            e.SavedEntities.ForEach(contentType => DistributedCache.Instance.RemoveContentTypeCache(contentType));
        }

        /// <summary>
        /// Fires after the document cache has been cleared for a particular document
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ContentAfterClearDocumentCache(global::umbraco.cms.businesslogic.web.Document sender, DocumentCacheEventArgs e)
        {
            DistributedCache.Instance.ClearAllMacroCacheOnCurrentServer();
            DistributedCache.Instance.ClearXsltCacheOnCurrentServer();
        }

        /// <summary>
        /// Fires after the document cache has been updated for a particular document
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ContentAfterUpdateDocumentCache(global::umbraco.cms.businesslogic.web.Document sender, DocumentCacheEventArgs e)
        {
            DistributedCache.Instance.ClearAllMacroCacheOnCurrentServer();
            DistributedCache.Instance.ClearXsltCacheOnCurrentServer();
        }

        static void UserDeleting(User sender, System.EventArgs e)
        {
            DistributedCache.Instance.RemoveUserCache(sender.Id);
        }

        static void UserSaving(User sender, System.EventArgs e)
        {
            DistributedCache.Instance.RefreshUserCache(sender.Id);
        }

        /// <summary>
        /// Removes cache for template
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void TemplateAfterDelete(Template sender, DeleteEventArgs e)
        {
            DistributedCache.Instance.RemoveTemplateCache(sender.Id);
        }

        /// <summary>
        /// Refresh cache for template
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void TemplateAfterSave(Template sender, SaveEventArgs e)
        {
            DistributedCache.Instance.RefreshTemplateCache(sender.Id);
        }

        /// <summary>
        /// Flush macro from cache
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void MacroAfterDelete(Macro sender, DeleteEventArgs e)
        {
            DistributedCache.Instance.RemoveMacroCache(sender);
        }

        /// <summary>
        /// Flush macro from cache
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void MacroAfterSave(Macro sender, SaveEventArgs e)
        {				
            DistributedCache.Instance.RefreshMacroCache(sender);
        }

        static void MediaServiceTrashing(IMediaService sender, Core.Events.MoveEventArgs<Core.Models.IMedia> e)
        {
            DistributedCache.Instance.RemoveMediaCache(e.Entity);            
        }

        static void MediaServiceMoving(IMediaService sender, Core.Events.MoveEventArgs<Core.Models.IMedia> e)
        {
            DistributedCache.Instance.RefreshMediaCache(e.Entity);
        }

        static void MediaServiceDeleting(IMediaService sender, Core.Events.DeleteEventArgs<Core.Models.IMedia> e)
        {
            DistributedCache.Instance.RemoveMediaCache(e.DeletedEntities.ToArray());
        }

        static void MediaServiceSaved(IMediaService sender, Core.Events.SaveEventArgs<Core.Models.IMedia> e)
        {
            DistributedCache.Instance.RefreshMediaCache(e.SavedEntities.ToArray());
        }

        static void MemberBeforeDelete(Member sender, DeleteEventArgs e)
        {
            DistributedCache.Instance.RemoveMemberCache(sender.Id);            
        }

        static void MemberAfterSave(Member sender, SaveEventArgs e)
        {
            DistributedCache.Instance.RefreshMemberCache(sender.Id);
        }
    }
}