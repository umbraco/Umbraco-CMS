﻿using System.Collections.Generic;
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
using umbraco.cms.businesslogic.web;
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
            //bind to application tree events
            ApplicationTree.Deleted += ApplicationTreeDeleted;
            ApplicationTree.Updated += ApplicationTreeUpdated;
            ApplicationTree.New += ApplicationTreeNew;

            //bind to application events
            Application.Deleted += ApplicationDeleted;
            Application.New += ApplicationNew;

            //bind to user type events
            UserType.Deleted += UserTypeDeleted;
            UserType.New += UserTypeNew;
            UserType.Updated += UserTypeUpdated;

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

            //Bind to user events

            User.Saving += UserSaving;
            User.Deleting += UserDeleting;

            //Bind to permission events

            Permission.New += PermissionNew;
            Permission.Updated += PermissionUpdated;
            Permission.Deleted += PermissionDeleted;

            //Bind to template events
            //NOTE: we need to bind to legacy and new API events currently: http://issues.umbraco.org/issue/U4-1979

            Template.AfterSave += TemplateAfterSave;
            Template.AfterDelete += TemplateAfterDelete;
            FileService.SavedTemplate += FileServiceSavedTemplate;
            FileService.DeletedTemplate += FileServiceDeletedTemplate;

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
        static void ApplicationNew(Application sender, System.EventArgs e)
        {
            DistributedCache.Instance.RefreshAllApplicationCache();
        }

        static void ApplicationDeleted(Application sender, System.EventArgs e)
        {
            DistributedCache.Instance.RefreshAllApplicationCache();
        } 
        #endregion

        #region UserType event handlers
        static void UserTypeUpdated(UserType sender, System.EventArgs e)
        {
            DistributedCache.Instance.RefreshUserTypeCache(sender.Id);
        }

        static void UserTypeNew(UserType sender, System.EventArgs e)
        {
            DistributedCache.Instance.RefreshAllUserTypeCache();
        }

        static void UserTypeDeleted(UserType sender, System.EventArgs e)
        {
            DistributedCache.Instance.RemoveUserTypeCache(sender.Id);
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

        #region Content/media Type event handlers
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
        #endregion
        
        #region User event handlers

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
        
        static void UserDeleting(User sender, System.EventArgs e)
        {
            DistributedCache.Instance.RemoveUserCache(sender.Id);
        }

        static void UserSaving(User sender, System.EventArgs e)
        {
            DistributedCache.Instance.RefreshUserCache(sender.Id);
        }

        private static void InvalidateCacheForPermissionsChange(UserPermission sender)
        {
            if (sender.User != null)
            {
                DistributedCache.Instance.RefreshUserCache(sender.User.Id);
            }
            if (sender.UserId > -1)
            {
                DistributedCache.Instance.RefreshUserCache(sender.UserId);
            }
            if (sender.NodeIds.Any())
            {
                DistributedCache.Instance.RefreshAllUserCache();
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
        #endregion

        #region Member event handlers
        static void MemberBeforeDelete(Member sender, DeleteEventArgs e)
        {
            DistributedCache.Instance.RemoveMemberCache(sender.Id);
        }

        static void MemberAfterSave(Member sender, SaveEventArgs e)
        {
            DistributedCache.Instance.RefreshMemberCache(sender.Id);
        } 
        #endregion
    }
}