using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Services;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using System.Linq;
using umbraco.cms.businesslogic.web;
using Content = Umbraco.Core.Models.Content;
using ApplicationTree = Umbraco.Core.Models.ApplicationTree;
using DeleteEventArgs = umbraco.cms.businesslogic.DeleteEventArgs;
using Macro = umbraco.cms.businesslogic.macro.Macro;
using Member = umbraco.cms.businesslogic.member.Member;
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
            //bind to application tree events
            ApplicationTreeService.Deleted += ApplicationTreeDeleted;
            ApplicationTreeService.Updated += ApplicationTreeUpdated;
            ApplicationTreeService.New += ApplicationTreeNew;

            //bind to application events
            SectionService.Deleted += ApplicationDeleted;
            SectionService.New += ApplicationNew;

            //bind to user / user type events
            UserService.SavedUserType += UserServiceSavedUserType;
            UserService.DeletedUserType += UserServiceDeletedUserType;
            UserService.SavedUser += UserServiceSavedUser;
            UserService.DeletedUser += UserServiceDeletedUser;

            //Bind to dictionary events
            //NOTE: we need to bind to legacy and new API events currently: http://issues.umbraco.org/issue/U4-1979

            global::umbraco.cms.businesslogic.Dictionary.DictionaryItem.New += DictionaryItemNew;
            global::umbraco.cms.businesslogic.Dictionary.DictionaryItem.Saving +=DictionaryItemSaving;
            global::umbraco.cms.businesslogic.Dictionary.DictionaryItem.Deleted +=DictionaryItemDeleted;
            LocalizationService.DeletedDictionaryItem += LocalizationServiceDeletedDictionaryItem;
            LocalizationService.SavedDictionaryItem += LocalizationServiceSavedDictionaryItem;

            //Bind to data type events
            //NOTE: we need to bind to legacy and new API events currently: http://issues.umbraco.org/issue/U4-1979

            global::umbraco.cms.businesslogic.datatype.DataTypeDefinition.AfterDelete += DataTypeDefinitionDeleting;
            global::umbraco.cms.businesslogic.datatype.DataTypeDefinition.Saving += DataTypeDefinitionSaving;
            DataTypeService.Deleted += DataTypeServiceDeleted;
            DataTypeService.Saved += DataTypeServiceSaved;

            //Bind to stylesheet events
            //NOTE: we need to bind to legacy and new API events currently: http://issues.umbraco.org/issue/U4-1979

            global::umbraco.cms.businesslogic.web.StylesheetProperty.AfterSave += StylesheetPropertyAfterSave;
            global::umbraco.cms.businesslogic.web.StylesheetProperty.AfterDelete += StylesheetPropertyAfterDelete;
            global::umbraco.cms.businesslogic.web.StyleSheet.AfterDelete += StyleSheetAfterDelete;
            global::umbraco.cms.businesslogic.web.StyleSheet.AfterSave += StyleSheetAfterSave;
            FileService.SavedStylesheet += FileServiceSavedStylesheet;
            FileService.DeletedStylesheet += FileServiceDeletedStylesheet;

            //Bind to domain events

            Domain.AfterSave += DomainAfterSave;
            Domain.AfterDelete += DomainAfterDelete;
            Domain.New += DomainNew;

            //Bind to language events
            //NOTE: we need to bind to legacy and new API events currently: http://issues.umbraco.org/issue/U4-1979

            global::umbraco.cms.businesslogic.language.Language.AfterDelete += LanguageAfterDelete;
            global::umbraco.cms.businesslogic.language.Language.New += LanguageNew;
            global::umbraco.cms.businesslogic.language.Language.AfterSave += LanguageAfterSave;
            LocalizationService.SavedLanguage += LocalizationServiceSavedLanguage;
            LocalizationService.DeletedLanguage += LocalizationServiceDeletedLanguage;

            //Bind to content type events

            ContentTypeService.SavedContentType += ContentTypeServiceSavedContentType;
            ContentTypeService.SavedMediaType += ContentTypeServiceSavedMediaType;
            ContentTypeService.DeletedContentType += ContentTypeServiceDeletedContentType;
            ContentTypeService.DeletedMediaType += ContentTypeServiceDeletedMediaType;
            MemberTypeService.Saved += MemberTypeServiceSaved;
            MemberTypeService.Deleted += MemberTypeServiceDeleted;

            //Bind to permission events

            Permission.New += PermissionNew;
            Permission.Updated += PermissionUpdated;
            Permission.Deleted += PermissionDeleted;
            PermissionRepository<IContent>.AssignedPermissions += CacheRefresherEventHandler_AssignedPermissions;

            //Bind to template events
            //NOTE: we need to bind to legacy and new API events currently: http://issues.umbraco.org/issue/U4-1979

            Template.AfterSave += TemplateAfterSave;
            Template.AfterDelete += TemplateAfterDelete;
            FileService.SavedTemplate += FileServiceSavedTemplate;
            FileService.DeletedTemplate += FileServiceDeletedTemplate;

            //Bind to macro events

            Macro.AfterSave += MacroAfterSave;
            Macro.AfterDelete += MacroAfterDelete;
            MacroService.Saved += MacroServiceSaved;
            MacroService.Deleted += MacroServiceDeleted;

            //Bind to member events

            MemberService.Saved += MemberServiceSaved;
            MemberService.Deleted += MemberServiceDeleted;
            MemberGroupService.Saved += MemberGroupService_Saved;
            MemberGroupService.Deleted += MemberGroupService_Deleted;

            //Bind to media events

            MediaService.Saved += MediaServiceSaved;
            //We need to perform all of the 'before' events here because we need a reference to the
            //media item's Path before it is moved/deleting/trashed
            //see: http://issues.umbraco.org/issue/U4-1653
            MediaService.Deleting += MediaServiceDeleting;
            MediaService.Moving += MediaServiceMoving;
            MediaService.Trashing += MediaServiceTrashing;

            //Bind to content events - this is for unpublished content syncing across servers (primarily for examine)

            ContentService.Saved += ContentServiceSaved;
            ContentService.Deleted += ContentServiceDeleted;
            ContentService.Copied += ContentServiceCopied;
            //NOTE: we do not listen for the trashed event because there is no cache to update for content in that case since
            // the unpublishing event handles that, and for examine with unpublished content indexes, we want to keep that data 
            // in the index, it's not until it's complete deleted that we want to remove it.
        }

        

        #region Content service event handlers

        /// <summary>
        /// Handles cache refreshgi for when content is copied
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// When an entity is copied new permissions may be assigned to it based on it's parent, if that is the 
        /// case then we need to clear all user permissions cache.
        /// </remarks>
        static void ContentServiceCopied(IContentService sender, CopyEventArgs<IContent> e)
        {
            //check if permissions have changed
            var permissionsChanged = ((Content)e.Copy).WasPropertyDirty("PermissionsChanged");
            if (permissionsChanged)
            {
                DistributedCache.Instance.RefreshAllUserPermissionsCache();
            }

            //run the un-published cache refresher
            DistributedCache.Instance.RefreshUnpublishedPageCache(e.Copy);
        }

        /// <summary>
        /// Handles cache refreshing for when content is deleted (not unpublished)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ContentServiceDeleted(IContentService sender, DeleteEventArgs<IContent> e)
        {
            DistributedCache.Instance.RemoveUnpublishedPageCache(e.DeletedEntities.ToArray());
        }

        /// <summary>
        /// Handles cache refreshing for when content is saved (not published)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// When an entity is saved we need to notify other servers about the change in order for the Examine indexes to 
        /// stay up-to-date for unpublished content.
        /// 
        /// When an entity is created new permissions may be assigned to it based on it's parent, if that is the 
        /// case then we need to clear all user permissions cache.
        /// </remarks>
        static void ContentServiceSaved(IContentService sender, SaveEventArgs<IContent> e)
        {
            var clearUserPermissions = false;
            e.SavedEntities.ForEach(x =>
            {
                //check if it is new
                if (x.IsNewEntity())
                {
                    //check if permissions have changed
                    var permissionsChanged = ((Content)x).WasPropertyDirty("PermissionsChanged");
                    if (permissionsChanged)
                    {
                        clearUserPermissions = true;                        
                    }    
                }
            });

            if (clearUserPermissions)
            {
                DistributedCache.Instance.RefreshAllUserPermissionsCache();
            }

            //filter out the entities that have only been saved (not newly published) since
            // newly published ones will be synced with the published page cache refresher
            var unpublished = e.SavedEntities.Where(x => x.JustPublished() == false);
            //run the un-published cache refresher
            DistributedCache.Instance.RefreshUnpublishedPageCache(unpublished.ToArray());
        }


        #endregion

        #region ApplicationTree event handlers
        static void ApplicationTreeNew(ApplicationTree sender, System.EventArgs e)
        {
            DistributedCache.Instance.RefreshAllApplicationTreeCache();
        }

        static void ApplicationTreeUpdated(ApplicationTree sender, System.EventArgs e)
        {
            DistributedCache.Instance.RefreshAllApplicationTreeCache();
        }

        static void ApplicationTreeDeleted(ApplicationTree sender, System.EventArgs e)
        {
            DistributedCache.Instance.RefreshAllApplicationTreeCache();
        } 
        #endregion

        #region Application event handlers
        static void ApplicationNew(Section sender, System.EventArgs e)
        {
            DistributedCache.Instance.RefreshAllApplicationCache();
        }

        static void ApplicationDeleted(Section sender, System.EventArgs e)
        {
            DistributedCache.Instance.RefreshAllApplicationCache();
        } 
        #endregion

        #region UserType event handlers
        static void UserServiceDeletedUserType(IUserService sender, Core.Events.DeleteEventArgs<Core.Models.Membership.IUserType> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveUserTypeCache(x.Id));
        }

        static void UserServiceSavedUserType(IUserService sender, Core.Events.SaveEventArgs<Core.Models.Membership.IUserType> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshUserTypeCache(x.Id));
        }
        
        #endregion
        
        #region Dictionary event handlers

        static void LocalizationServiceSavedDictionaryItem(ILocalizationService sender, Core.Events.SaveEventArgs<IDictionaryItem> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshDictionaryCache(x.Id));
        }

        static void LocalizationServiceDeletedDictionaryItem(ILocalizationService sender, Core.Events.DeleteEventArgs<IDictionaryItem> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveDictionaryCache(x.Id));
        }

        static void DictionaryItemDeleted(global::umbraco.cms.businesslogic.Dictionary.DictionaryItem sender, System.EventArgs e)
        {
            DistributedCache.Instance.RemoveDictionaryCache(sender.id);
        }

        static void DictionaryItemSaving(global::umbraco.cms.businesslogic.Dictionary.DictionaryItem sender, System.EventArgs e)
        {
            DistributedCache.Instance.RefreshDictionaryCache(sender.id);
        }

        static void DictionaryItemNew(global::umbraco.cms.businesslogic.Dictionary.DictionaryItem sender, System.EventArgs e)
        {
            DistributedCache.Instance.RefreshDictionaryCache(sender.id);
        } 

        #endregion

        #region DataType event handlers
        static void DataTypeServiceSaved(IDataTypeService sender, Core.Events.SaveEventArgs<IDataTypeDefinition> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshDataTypeCache(x));
        }

        static void DataTypeServiceDeleted(IDataTypeService sender, Core.Events.DeleteEventArgs<IDataTypeDefinition> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveDataTypeCache(x));
        }

        static void DataTypeDefinitionSaving(global::umbraco.cms.businesslogic.datatype.DataTypeDefinition sender, System.EventArgs e)
        {
            DistributedCache.Instance.RefreshDataTypeCache(sender);
        }

        static void DataTypeDefinitionDeleting(global::umbraco.cms.businesslogic.datatype.DataTypeDefinition sender, System.EventArgs e)
        {
            DistributedCache.Instance.RemoveDataTypeCache(sender);
        } 
        #endregion

        #region Stylesheet and stylesheet property event handlers
        static void StylesheetPropertyAfterSave(global::umbraco.cms.businesslogic.web.StylesheetProperty sender, SaveEventArgs e)
        {
            DistributedCache.Instance.RefreshStylesheetPropertyCache(sender);
        }

        static void StylesheetPropertyAfterDelete(global::umbraco.cms.businesslogic.web.StylesheetProperty sender, DeleteEventArgs e)
        {
            DistributedCache.Instance.RemoveStylesheetPropertyCache(sender);
        }

        static void FileServiceDeletedStylesheet(IFileService sender, Core.Events.DeleteEventArgs<Stylesheet> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveStylesheetCache(x));
        }

        static void FileServiceSavedStylesheet(IFileService sender, Core.Events.SaveEventArgs<Stylesheet> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshStylesheetCache(x));
        }

        static void StyleSheetAfterSave(StyleSheet sender, SaveEventArgs e)
        {
            DistributedCache.Instance.RefreshStylesheetCache(sender);
        }

        static void StyleSheetAfterDelete(StyleSheet sender, DeleteEventArgs e)
        {
            DistributedCache.Instance.RemoveStylesheetCache(sender);
        } 
        #endregion

        #region Domain event handlers
        static void DomainNew(Domain sender, NewEventArgs e)
        {
            DistributedCache.Instance.RefreshDomainCache(sender);
        }

        static void DomainAfterDelete(Domain sender, DeleteEventArgs e)
        {
            DistributedCache.Instance.RemoveDomainCache(sender);
        }

        static void DomainAfterSave(Domain sender, SaveEventArgs e)
        {
            DistributedCache.Instance.RefreshDomainCache(sender);
        } 
        #endregion

        #region Language event handlers
        /// <summary>
        /// Fires when a langauge is deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void LocalizationServiceDeletedLanguage(ILocalizationService sender, Core.Events.DeleteEventArgs<ILanguage> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveLanguageCache(x));
        }

        /// <summary>
        /// Fires when a langauge is saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void LocalizationServiceSavedLanguage(ILocalizationService sender, Core.Events.SaveEventArgs<ILanguage> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshLanguageCache(x));
        }

        /// <summary>
        /// Fires when a langauge is saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void LanguageAfterSave(global::umbraco.cms.businesslogic.language.Language sender, SaveEventArgs e)
        {
            DistributedCache.Instance.RefreshLanguageCache(sender);
        }

        /// <summary>
        /// Fires when a langauge is created
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void LanguageNew(global::umbraco.cms.businesslogic.language.Language sender, NewEventArgs e)
        {
            DistributedCache.Instance.RefreshLanguageCache(sender);
        }

        /// <summary>
        /// Fires when a langauge is deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void LanguageAfterDelete(global::umbraco.cms.businesslogic.language.Language sender, DeleteEventArgs e)
        {
            DistributedCache.Instance.RemoveLanguageCache(sender);
        } 
        #endregion

        #region Content/media/member Type event handlers
        /// <summary>
        /// Fires when a media type is deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ContentTypeServiceDeletedMediaType(IContentTypeService sender, Core.Events.DeleteEventArgs<IMediaType> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveMediaTypeCache(x));
        }

        /// <summary>
        /// Fires when a content type is deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ContentTypeServiceDeletedContentType(IContentTypeService sender, Core.Events.DeleteEventArgs<IContentType> e)
        {
            e.DeletedEntities.ForEach(contentType => DistributedCache.Instance.RemoveContentTypeCache(contentType));
        }

        /// <summary>
        /// Fires when a member type is deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void MemberTypeServiceDeleted(IMemberTypeService sender, Core.Events.DeleteEventArgs<IMemberType> e)
        {
            e.DeletedEntities.ForEach(contentType => DistributedCache.Instance.RemoveMemberTypeCache(contentType));
        }

        /// <summary>
        /// Fires when a media type is saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ContentTypeServiceSavedMediaType(IContentTypeService sender, Core.Events.SaveEventArgs<IMediaType> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshMediaTypeCache(x));
        }

        /// <summary>
        /// Fires when a content type is saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ContentTypeServiceSavedContentType(IContentTypeService sender, Core.Events.SaveEventArgs<IContentType> e)
        {
            e.SavedEntities.ForEach(contentType => DistributedCache.Instance.RefreshContentTypeCache(contentType));
        }

        /// <summary>
        /// Fires when a member type is saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void MemberTypeServiceSaved(IMemberTypeService sender, Core.Events.SaveEventArgs<IMemberType> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshMemberTypeCache(x));
        }

        
        #endregion
        
        #region User/permissions event handlers

        static void CacheRefresherEventHandler_AssignedPermissions(PermissionRepository<IContent> sender, SaveEventArgs<EntityPermission> e)
        {
            var userIds = e.SavedEntities.Select(x => x.UserId).Distinct();
            userIds.ForEach(x => DistributedCache.Instance.RefreshUserPermissionsCache(x));
        }

        static void PermissionDeleted(UserPermission sender, DeleteEventArgs e)
        {
            InvalidateCacheForPermissionsChange(sender);
        }

        static void PermissionUpdated(UserPermission sender, SaveEventArgs e)
        {
            InvalidateCacheForPermissionsChange(sender);
        }

        static void PermissionNew(UserPermission sender, NewEventArgs e)
        {
            InvalidateCacheForPermissionsChange(sender);
        }

        static void UserServiceSavedUser(IUserService sender, SaveEventArgs<IUser> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshUserCache(x.Id));
        }

        static void UserServiceDeletedUser(IUserService sender, DeleteEventArgs<IUser> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveUserCache(x.Id));
        }
        
        private static void InvalidateCacheForPermissionsChange(UserPermission sender)
        {
            if (sender.User != null)
            {
                DistributedCache.Instance.RefreshUserPermissionsCache(sender.User.Id);
            }
            else if (sender.UserId > -1)
            {
                DistributedCache.Instance.RefreshUserPermissionsCache(sender.UserId);
            }
            else if (sender.NodeIds.Any())
            {
                DistributedCache.Instance.RefreshAllUserPermissionsCache();
            }
        }

        #endregion

        #region Template event handlers

        /// <summary>
        /// Removes cache for template
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void FileServiceDeletedTemplate(IFileService sender, Core.Events.DeleteEventArgs<ITemplate> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveTemplateCache(x.Id));
        }

        /// <summary>
        /// Refresh cache for template
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void FileServiceSavedTemplate(IFileService sender, Core.Events.SaveEventArgs<ITemplate> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshTemplateCache(x.Id));
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
        #endregion

        #region Macro event handlers

        void MacroServiceDeleted(IMacroService sender, Core.Events.DeleteEventArgs<IMacro> e)
        {
            foreach (var entity in e.DeletedEntities)
            {
                DistributedCache.Instance.RemoveMacroCache(entity);
            }
        }

        void MacroServiceSaved(IMacroService sender, Core.Events.SaveEventArgs<IMacro> e)
        {
            foreach (var entity in e.SavedEntities)
            {
                DistributedCache.Instance.RefreshMacroCache(entity);
            }
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
        #endregion

        #region Media event handlers
        static void MediaServiceTrashing(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            DistributedCache.Instance.RemoveMediaCache(false, e.Entity);
        }

        static void MediaServiceMoving(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            DistributedCache.Instance.RefreshMediaCache(e.Entity);
        }

        static void MediaServiceDeleting(IMediaService sender, DeleteEventArgs<IMedia> e)
        {
            DistributedCache.Instance.RemoveMediaCache(true, e.DeletedEntities.ToArray());
        }

        static void MediaServiceSaved(IMediaService sender, SaveEventArgs<IMedia> e)
        {
            DistributedCache.Instance.RefreshMediaCache(e.SavedEntities.ToArray());
        } 
        #endregion

        #region Member event handlers

        static void MemberServiceDeleted(IMemberService sender, DeleteEventArgs<IMember> e)
        {
            DistributedCache.Instance.RemoveMemberCache(e.DeletedEntities.ToArray());    
        }

        static void MemberServiceSaved(IMemberService sender, SaveEventArgs<IMember> e)
        {
            DistributedCache.Instance.RefreshMemberCache(e.SavedEntities.ToArray());
        }

        #endregion

        #region Member group event handlers

        static void MemberGroupService_Deleted(IMemberGroupService sender, DeleteEventArgs<IMemberGroup> e)
        {
            foreach (var m in e.DeletedEntities.ToArray())
            {
                DistributedCache.Instance.RemoveMemberGroupCache(m.Id);
            }
        }

        static void MemberGroupService_Saved(IMemberGroupService sender, SaveEventArgs<IMemberGroup> e)
        {
            foreach (var m in e.SavedEntities.ToArray())
            {
                DistributedCache.Instance.RemoveMemberGroupCache(m.Id);
            }
        } 
        #endregion
    }
}