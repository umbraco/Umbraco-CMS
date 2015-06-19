using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Services;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using System.Linq;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Publishing;
using Content = Umbraco.Core.Models.Content;
using ApplicationTree = Umbraco.Core.Models.ApplicationTree;
using DeleteEventArgs = umbraco.cms.businesslogic.DeleteEventArgs;

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

            LocalizationService.DeletedDictionaryItem += LocalizationServiceDeletedDictionaryItem;
            LocalizationService.SavedDictionaryItem += LocalizationServiceSavedDictionaryItem;

            //Bind to data type events
            //NOTE: we need to bind to legacy and new API events currently: http://issues.umbraco.org/issue/U4-1979

            DataTypeService.Deleted += DataTypeServiceDeleted;
            DataTypeService.Saved += DataTypeServiceSaved;

            //Bind to stylesheet events

            FileService.SavedStylesheet += FileServiceSavedStylesheet;
            FileService.DeletedStylesheet += FileServiceDeletedStylesheet;

            //Bind to domain events

            DomainService.Saved += DomainService_Saved;
            DomainService.Deleted += DomainService_Deleted;

            //Bind to language events

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

            //TODO: Wrap legacy permissions so we can get rid of this
            Permission.New += PermissionNew;
            Permission.Updated += PermissionUpdated;
            Permission.Deleted += PermissionDeleted;
            PermissionRepository<IContent>.AssignedPermissions += CacheRefresherEventHandler_AssignedPermissions;

            //Bind to template events

            FileService.SavedTemplate += FileServiceSavedTemplate;
            FileService.DeletedTemplate += FileServiceDeletedTemplate;

            //Bind to macro events

            MacroService.Saved += MacroServiceSaved;
            MacroService.Deleted += MacroServiceDeleted;

            //Bind to member events

            MemberService.Saved += MemberServiceSaved;
            MemberService.Deleted += MemberServiceDeleted;
            MemberGroupService.Saved += MemberGroupService_Saved;
            MemberGroupService.Deleted += MemberGroupService_Deleted;

            //Bind to media events

            MediaService.Saved += MediaServiceSaved;            
            MediaService.Deleted += MediaServiceDeleted;
            MediaService.Moved += MediaServiceMoved;
            MediaService.Trashed += MediaServiceTrashed;
            MediaService.EmptiedRecycleBin += MediaServiceEmptiedRecycleBin;

            //Bind to content events - this is for unpublished content syncing across servers (primarily for examine)
            
            ContentService.Saved += ContentServiceSaved;
            ContentService.Deleted += ContentServiceDeleted;
            ContentService.Copied += ContentServiceCopied;
            //TODO: The Move method of the content service fires Saved/Published events during its execution so we don't need to listen to moved
            //ContentService.Moved += ContentServiceMoved;
            ContentService.Trashed += ContentServiceTrashed;
            ContentService.EmptiedRecycleBin += ContentServiceEmptiedRecycleBin;

            PublishingStrategy.Published += PublishingStrategy_Published;
            PublishingStrategy.UnPublished += PublishingStrategy_UnPublished;

            //public access events
            PublicAccessService.Saved += PublicAccessService_Saved;
        }

        #region Publishing

        void PublishingStrategy_UnPublished(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            if (e.PublishedEntities.Any())
            {
                if (e.PublishedEntities.Count() > 1)
                {
                    foreach (var c in e.PublishedEntities)
                    {
                        UnPublishSingle(c);
                    }
                }
                else
                {
                    var content = e.PublishedEntities.FirstOrDefault();
                    UnPublishSingle(content);
                }
            }
        }

        /// <summary>
        /// Refreshes the xml cache for a single node by removing it
        /// </summary>
        private void UnPublishSingle(IContent content)
        {
            DistributedCache.Instance.RemovePageCache(content);
        }

        void PublishingStrategy_Published(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            if (e.PublishedEntities.Any())
            {
                if (e.IsAllRepublished)
                {
                    UpdateEntireCache();
                    return;
                }

                if (e.PublishedEntities.Count() > 1)
                {
                    UpdateMultipleContentCache(e.PublishedEntities);
                }
                else
                {
                    var content = e.PublishedEntities.FirstOrDefault();
                    UpdateSingleContentCache(content);
                }
            }
        }

        /// <summary>
        /// Refreshes the xml cache for all nodes
        /// </summary>
        private void UpdateEntireCache()
        {
            DistributedCache.Instance.RefreshAllPageCache();
        }

        /// <summary>
        /// Refreshes the xml cache for nodes in list
        /// </summary>
        private void UpdateMultipleContentCache(IEnumerable<IContent> content)
        {
            DistributedCache.Instance.RefreshPageCache(content.ToArray());
        }

        /// <summary>
        /// Refreshes the xml cache for a single node
        /// </summary>
        private void UpdateSingleContentCache(IContent content)
        {
            DistributedCache.Instance.RefreshPageCache(content);
        }

        #endregion

        #region Public access event handlers

        static void PublicAccessService_Saved(IPublicAccessService sender, SaveEventArgs<PublicAccessEntry> e)
        {
            DistributedCache.Instance.RefreshPublicAccess();
        }

        #endregion

        #region Content service event handlers

        static void ContentServiceEmptiedRecycleBin(IContentService sender, RecycleBinEventArgs e)
        {
            if (e.RecycleBinEmptiedSuccessfully && e.IsContentRecycleBin)
            {
                DistributedCache.Instance.RemoveUnpublishedCachePermanently(e.Ids.ToArray());
            }
        }
        
        /// <summary>
        /// Handles cache refreshing for when content is trashed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// This is for the unpublished page refresher - the entity will be unpublished before being moved to the trash
        /// and the unpublished event will take care of remove it from any published caches
        /// </remarks>
        static void ContentServiceTrashed(IContentService sender, MoveEventArgs<IContent> e)
        {
            DistributedCache.Instance.RefreshUnpublishedPageCache(
                e.MoveInfoCollection.Select(x => x.Entity).ToArray());
        }

        /// <summary>
        /// Handles cache refreshing for when content is copied
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

            //run the un-published cache refresher since copied content is not published
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
        static void ApplicationTreeNew(ApplicationTree sender, EventArgs e)
        {
            DistributedCache.Instance.RefreshAllApplicationTreeCache();
        }

        static void ApplicationTreeUpdated(ApplicationTree sender, EventArgs e)
        {
            DistributedCache.Instance.RefreshAllApplicationTreeCache();
        }

        static void ApplicationTreeDeleted(ApplicationTree sender, EventArgs e)
        {
            DistributedCache.Instance.RefreshAllApplicationTreeCache();
        } 
        #endregion

        #region Application event handlers
        static void ApplicationNew(Section sender, EventArgs e)
        {
            DistributedCache.Instance.RefreshAllApplicationCache();
        }

        static void ApplicationDeleted(Section sender, EventArgs e)
        {
            DistributedCache.Instance.RefreshAllApplicationCache();
        } 
        #endregion

        #region UserType event handlers
        static void UserServiceDeletedUserType(IUserService sender, DeleteEventArgs<IUserType> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveUserTypeCache(x.Id));
        }

        static void UserServiceSavedUserType(IUserService sender, SaveEventArgs<IUserType> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshUserTypeCache(x.Id));
        }
        
        #endregion
        
        #region Dictionary event handlers

        static void LocalizationServiceSavedDictionaryItem(ILocalizationService sender, SaveEventArgs<IDictionaryItem> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshDictionaryCache(x.Id));
        }

        static void LocalizationServiceDeletedDictionaryItem(ILocalizationService sender, DeleteEventArgs<IDictionaryItem> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveDictionaryCache(x.Id));
        }

        #endregion

        #region DataType event handlers
        static void DataTypeServiceSaved(IDataTypeService sender, SaveEventArgs<IDataTypeDefinition> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshDataTypeCache(x));
        }

        static void DataTypeServiceDeleted(IDataTypeService sender, DeleteEventArgs<IDataTypeDefinition> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveDataTypeCache(x));
        }

   
        #endregion

        #region Stylesheet and stylesheet property event handlers
     
        static void FileServiceDeletedStylesheet(IFileService sender, DeleteEventArgs<Stylesheet> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveStylesheetCache(x));
        }

        static void FileServiceSavedStylesheet(IFileService sender, SaveEventArgs<Stylesheet> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshStylesheetCache(x));
        }

        #endregion

        #region Domain event handlers

        static void DomainService_Saved(IDomainService sender, SaveEventArgs<IDomain> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshDomainCache(x));
        }

        static void DomainService_Deleted(IDomainService sender, DeleteEventArgs<IDomain> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveDomainCache(x));
        }

        #endregion

        #region Language event handlers
        /// <summary>
        /// Fires when a langauge is deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void LocalizationServiceDeletedLanguage(ILocalizationService sender, DeleteEventArgs<ILanguage> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveLanguageCache(x));
        }

        /// <summary>
        /// Fires when a langauge is saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void LocalizationServiceSavedLanguage(ILocalizationService sender, SaveEventArgs<ILanguage> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshLanguageCache(x));
        }
      
        #endregion

        #region Content/media/member Type event handlers
        /// <summary>
        /// Fires when a media type is deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ContentTypeServiceDeletedMediaType(IContentTypeService sender, DeleteEventArgs<IMediaType> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveMediaTypeCache(x));
        }

        /// <summary>
        /// Fires when a content type is deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ContentTypeServiceDeletedContentType(IContentTypeService sender, DeleteEventArgs<IContentType> e)
        {
            e.DeletedEntities.ForEach(contentType => DistributedCache.Instance.RemoveContentTypeCache(contentType));
        }

        /// <summary>
        /// Fires when a member type is deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void MemberTypeServiceDeleted(IMemberTypeService sender, DeleteEventArgs<IMemberType> e)
        {
            e.DeletedEntities.ForEach(contentType => DistributedCache.Instance.RemoveMemberTypeCache(contentType));
        }

        /// <summary>
        /// Fires when a media type is saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ContentTypeServiceSavedMediaType(IContentTypeService sender, SaveEventArgs<IMediaType> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshMediaTypeCache(x));
        }

        /// <summary>
        /// Fires when a content type is saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ContentTypeServiceSavedContentType(IContentTypeService sender, SaveEventArgs<IContentType> e)
        {
            e.SavedEntities.ForEach(contentType => DistributedCache.Instance.RefreshContentTypeCache(contentType));
        }

        /// <summary>
        /// Fires when a member type is saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void MemberTypeServiceSaved(IMemberTypeService sender, SaveEventArgs<IMemberType> e)
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
        static void FileServiceDeletedTemplate(IFileService sender, DeleteEventArgs<ITemplate> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveTemplateCache(x.Id));
        }

        /// <summary>
        /// Refresh cache for template
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void FileServiceSavedTemplate(IFileService sender, SaveEventArgs<ITemplate> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshTemplateCache(x.Id));
        }
      
        #endregion

        #region Macro event handlers

        void MacroServiceDeleted(IMacroService sender, DeleteEventArgs<IMacro> e)
        {
            foreach (var entity in e.DeletedEntities)
            {
                DistributedCache.Instance.RemoveMacroCache(entity);
            }
        }

        void MacroServiceSaved(IMacroService sender, SaveEventArgs<IMacro> e)
        {
            foreach (var entity in e.SavedEntities)
            {
                DistributedCache.Instance.RefreshMacroCache(entity);
            }
        }
  
        #endregion

        #region Media event handlers

        static void MediaServiceEmptiedRecycleBin(IMediaService sender, RecycleBinEventArgs e)
        {
            if (e.RecycleBinEmptiedSuccessfully && e.IsMediaRecycleBin)
            {
                DistributedCache.Instance.RemoveMediaCachePermanently(e.Ids.ToArray());
            }
        }

        static void MediaServiceTrashed(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            DistributedCache.Instance.RemoveMediaCacheAfterRecycling(e.MoveInfoCollection.ToArray());
        }

        static void MediaServiceMoved(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            DistributedCache.Instance.RefreshMediaCacheAfterMoving(e.MoveInfoCollection.ToArray());
        }

        static void MediaServiceDeleted(IMediaService sender, DeleteEventArgs<IMedia> e)
        {
            DistributedCache.Instance.RemoveMediaCachePermanently(e.DeletedEntities.Select(x => x.Id).ToArray());
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