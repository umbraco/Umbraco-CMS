using System;
using System.Collections.Concurrent;
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
using System.Reflection;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Publishing;
using Content = Umbraco.Core.Models.Content;
using ApplicationTree = Umbraco.Core.Models.ApplicationTree;
using DeleteEventArgs = umbraco.cms.businesslogic.DeleteEventArgs;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Class which listens to events on business level objects in order to invalidate the cache amongst servers when data changes
    /// </summary>
    [Weight(int.MinValue)]
    public class CacheRefresherEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            LogHelper.Info<CacheRefresherEventHandler>("Initializing Umbraco internal event handlers for cache refreshing");

            //bind to application tree events
            ApplicationTreeService.Deleted += ApplicationTreeService_Deleted;
            ApplicationTreeService.Updated += ApplicationTreeService_Updated;
            ApplicationTreeService.New += ApplicationTreeService_New;

            //bind to application events
            SectionService.Deleted += SectionService_Deleted;
            SectionService.New += SectionService_New;

            //bind to user / user type events
            UserService.SavedUserType += UserService_SavedUserType;
            UserService.DeletedUserType += UserService_DeletedUserType;
            UserService.SavedUser += UserService_SavedUser;
            UserService.DeletedUser += UserService_DeletedUser;

            //Bind to dictionary events

            LocalizationService.DeletedDictionaryItem += LocalizationService_DeletedDictionaryItem;
            LocalizationService.SavedDictionaryItem += LocalizationService_SavedDictionaryItem;

            //Bind to data type events
            //NOTE: we need to bind to legacy and new API events currently: http://issues.umbraco.org/issue/U4-1979

            DataTypeService.Deleted += DataTypeService_Deleted;
            DataTypeService.Saved += DataTypeService_Saved;

            //Bind to stylesheet events

            FileService.SavedStylesheet += FileService_SavedStylesheet;
            FileService.DeletedStylesheet += FileService_DeletedStylesheet;

            //Bind to domain events

            DomainService.Saved += DomainService_Saved;
            DomainService.Deleted += DomainService_Deleted;

            //Bind to language events

            LocalizationService.SavedLanguage += LocalizationService_SavedLanguage;
            LocalizationService.DeletedLanguage += LocalizationService_DeletedLanguage;

            //Bind to content type events

            ContentTypeService.SavedContentType += ContentTypeService_SavedContentType;
            ContentTypeService.SavedMediaType += ContentTypeService_SavedMediaType;
            ContentTypeService.DeletedContentType += ContentTypeService_DeletedContentType;
            ContentTypeService.DeletedMediaType += ContentTypeService_DeletedMediaType;
            MemberTypeService.Saved += MemberTypeService_Saved;
            MemberTypeService.Deleted += MemberTypeService_Deleted;

            //Bind to permission events

            //TODO: Wrap legacy permissions so we can get rid of this
            Permission.New += PermissionNew;
            Permission.Updated += PermissionUpdated;
            Permission.Deleted += PermissionDeleted;
            PermissionRepository<IContent>.AssignedPermissions += CacheRefresherEventHandler_AssignedPermissions;

            //Bind to template events

            FileService.SavedTemplate += FileService_SavedTemplate;
            FileService.DeletedTemplate += FileService_DeletedTemplate;

            //Bind to macro events

            MacroService.Saved += MacroService_Saved;
            MacroService.Deleted += MacroService_Deleted;

            //Bind to member events

            MemberService.Saved += MemberService_Saved;
            MemberService.Deleted += MemberService_Deleted;
            MemberGroupService.Saved += MemberGroupService_Saved;
            MemberGroupService.Deleted += MemberGroupService_Deleted;

            //Bind to media events

            MediaService.Saved += MediaService_Saved;
            MediaService.Deleted += MediaService_Deleted;
            MediaService.Moved += MediaService_Moved;
            MediaService.Trashed += MediaService_Trashed;
            MediaService.EmptiedRecycleBin += MediaService_EmptiedRecycleBin;

            //Bind to content events - this is for unpublished content syncing across servers (primarily for examine)

            ContentService.Saved += ContentService_Saved;
            ContentService.Deleted += ContentService_Deleted;
            ContentService.Copied += ContentService_Copied;
            //TODO: The Move method of the content service fires Saved/Published events during its execution so we don't need to listen to moved
            //ContentService.Moved += ContentServiceMoved;
            ContentService.Trashed += ContentService_Trashed;
            ContentService.EmptiedRecycleBin += ContentService_EmptiedRecycleBin;

            ContentService.Published += ContentService_Published;
            ContentService.UnPublished += ContentService_UnPublished;

            //public access events
            PublicAccessService.Saved += PublicAccessService_Saved;
            PublicAccessService.Deleted += PublicAccessService_Deleted;

            RelationService.SavedRelationType += RelationService_SavedRelationType;
            RelationService.DeletedRelationType += RelationService_DeletedRelationType;
        }

        // for tests
        internal void Destroy()
        {
            //bind to application tree events
            ApplicationTreeService.Deleted -= ApplicationTreeService_Deleted;
            ApplicationTreeService.Updated -= ApplicationTreeService_Updated;
            ApplicationTreeService.New -= ApplicationTreeService_New;

            //bind to application events
            SectionService.Deleted -= SectionService_Deleted;
            SectionService.New -= SectionService_New;

            //bind to user / user type events
            UserService.SavedUserType -= UserService_SavedUserType;
            UserService.DeletedUserType -= UserService_DeletedUserType;
            UserService.SavedUser -= UserService_SavedUser;
            UserService.DeletedUser -= UserService_DeletedUser;

            //Bind to dictionary events

            LocalizationService.DeletedDictionaryItem -= LocalizationService_DeletedDictionaryItem;
            LocalizationService.SavedDictionaryItem -= LocalizationService_SavedDictionaryItem;

            //Bind to data type events
            //NOTE: we need to bind to legacy and new API events currently: http://issues.umbraco.org/issue/U4-1979

            DataTypeService.Deleted -= DataTypeService_Deleted;
            DataTypeService.Saved -= DataTypeService_Saved;

            //Bind to stylesheet events

            FileService.SavedStylesheet -= FileService_SavedStylesheet;
            FileService.DeletedStylesheet -= FileService_DeletedStylesheet;

            //Bind to domain events

            DomainService.Saved -= DomainService_Saved;
            DomainService.Deleted -= DomainService_Deleted;

            //Bind to language events

            LocalizationService.SavedLanguage -= LocalizationService_SavedLanguage;
            LocalizationService.DeletedLanguage -= LocalizationService_DeletedLanguage;

            //Bind to content type events

            ContentTypeService.SavedContentType -= ContentTypeService_SavedContentType;
            ContentTypeService.SavedMediaType -= ContentTypeService_SavedMediaType;
            ContentTypeService.DeletedContentType -= ContentTypeService_DeletedContentType;
            ContentTypeService.DeletedMediaType -= ContentTypeService_DeletedMediaType;
            MemberTypeService.Saved -= MemberTypeService_Saved;
            MemberTypeService.Deleted -= MemberTypeService_Deleted;

            //Bind to permission events

            //TODO: Wrap legacy permissions so we can get rid of this
            Permission.New -= PermissionNew;
            Permission.Updated -= PermissionUpdated;
            Permission.Deleted -= PermissionDeleted;
            PermissionRepository<IContent>.AssignedPermissions -= CacheRefresherEventHandler_AssignedPermissions;

            //Bind to template events

            FileService.SavedTemplate -= FileService_SavedTemplate;
            FileService.DeletedTemplate -= FileService_DeletedTemplate;

            //Bind to macro events

            MacroService.Saved -= MacroService_Saved;
            MacroService.Deleted -= MacroService_Deleted;

            //Bind to member events

            MemberService.Saved -= MemberService_Saved;
            MemberService.Deleted -= MemberService_Deleted;
            MemberGroupService.Saved -= MemberGroupService_Saved;
            MemberGroupService.Deleted -= MemberGroupService_Deleted;

            //Bind to media events

            MediaService.Saved -= MediaService_Saved;
            MediaService.Deleted -= MediaService_Deleted;
            MediaService.Moved -= MediaService_Moved;
            MediaService.Trashed -= MediaService_Trashed;
            MediaService.EmptiedRecycleBin -= MediaService_EmptiedRecycleBin;

            //Bind to content events - this is for unpublished content syncing across servers (primarily for examine)

            ContentService.Saved -= ContentService_Saved;
            ContentService.Deleted -= ContentService_Deleted;
            ContentService.Copied -= ContentService_Copied;
            //TODO: The Move method of the content service fires Saved/Published events during its execution so we don't need to listen to moved
            //ContentService.Moved -= ContentServiceMoved;
            ContentService.Trashed -= ContentService_Trashed;
            ContentService.EmptiedRecycleBin -= ContentService_EmptiedRecycleBin;

            ContentService.Published -= ContentService_Published;
            ContentService.UnPublished -= ContentService_UnPublished;

            //public access events
            PublicAccessService.Saved -= PublicAccessService_Saved;
            PublicAccessService.Deleted -= PublicAccessService_Deleted;

            RelationService.SavedRelationType -= RelationService_SavedRelationType;
            RelationService.DeletedRelationType -= RelationService_DeletedRelationType;
        }

        #region Publishing

        // IPublishingStrategy (obsolete) events are proxied into ContentService, which works fine when
        // events are actually raised, but not when they are handled by HandleEvents, so we have to have
        // these proxy methods that are *not* registered against any event *but* used by HandleEvents.

        static void PublishingStrategy_UnPublished(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            ContentService_UnPublished(sender, e);
        }

        static void PublishingStrategy_Published(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            ContentService_Published(sender, e);
        }

        static void ContentService_UnPublished(IPublishingStrategy sender, PublishEventArgs<IContent> e)
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
        private static void UnPublishSingle(IContent content)
        {
            DistributedCache.Instance.RemovePageCache(content);
        }

        static void ContentService_Published(IPublishingStrategy sender, PublishEventArgs<IContent> e)
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
        private static void UpdateEntireCache()
        {
            DistributedCache.Instance.RefreshAllPageCache();
        }

        /// <summary>
        /// Refreshes the xml cache for nodes in list
        /// </summary>
        private static void UpdateMultipleContentCache(IEnumerable<IContent> content)
        {
            DistributedCache.Instance.RefreshPageCache(content.ToArray());
        }

        /// <summary>
        /// Refreshes the xml cache for a single node
        /// </summary>
        private static void UpdateSingleContentCache(IContent content)
        {
            DistributedCache.Instance.RefreshPageCache(content);
        }

        #endregion

        #region Public access event handlers

        static void PublicAccessService_Saved(IPublicAccessService sender, SaveEventArgs<PublicAccessEntry> e)
        {
            DistributedCache.Instance.RefreshPublicAccess();
        }

        static void PublicAccessService_Deleted(IPublicAccessService sender, DeleteEventArgs<PublicAccessEntry> e)
        {
            DistributedCache.Instance.RefreshPublicAccess();
        }

        #endregion

        #region Content service event handlers

        static void ContentService_EmptiedRecycleBin(IContentService sender, RecycleBinEventArgs e)
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
        static void ContentService_Trashed(IContentService sender, MoveEventArgs<IContent> e)
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
        static void ContentService_Copied(IContentService sender, CopyEventArgs<IContent> e)
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
        static void ContentService_Deleted(IContentService sender, DeleteEventArgs<IContent> e)
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
        static void ContentService_Saved(IContentService sender, SaveEventArgs<IContent> e)
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
        static void ApplicationTreeService_New(ApplicationTree sender, EventArgs e)
        {
            DistributedCache.Instance.RefreshAllApplicationTreeCache();
        }

        static void ApplicationTreeService_Updated(ApplicationTree sender, EventArgs e)
        {
            DistributedCache.Instance.RefreshAllApplicationTreeCache();
        }

        static void ApplicationTreeService_Deleted(ApplicationTree sender, EventArgs e)
        {
            DistributedCache.Instance.RefreshAllApplicationTreeCache();
        }
        #endregion

        #region Application event handlers
        static void SectionService_New(Section sender, EventArgs e)
        {
            DistributedCache.Instance.RefreshAllApplicationCache();
        }

        static void SectionService_Deleted(Section sender, EventArgs e)
        {
            DistributedCache.Instance.RefreshAllApplicationCache();
        }
        #endregion

        #region UserType event handlers
        static void UserService_DeletedUserType(IUserService sender, DeleteEventArgs<IUserType> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveUserTypeCache(x.Id));
        }

        static void UserService_SavedUserType(IUserService sender, SaveEventArgs<IUserType> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshUserTypeCache(x.Id));
        }

        #endregion

        #region Dictionary event handlers

        static void LocalizationService_SavedDictionaryItem(ILocalizationService sender, SaveEventArgs<IDictionaryItem> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshDictionaryCache(x.Id));
        }

        static void LocalizationService_DeletedDictionaryItem(ILocalizationService sender, DeleteEventArgs<IDictionaryItem> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveDictionaryCache(x.Id));
        }

        #endregion

        #region DataType event handlers
        static void DataTypeService_Saved(IDataTypeService sender, SaveEventArgs<IDataTypeDefinition> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshDataTypeCache(x));
        }

        static void DataTypeService_Deleted(IDataTypeService sender, DeleteEventArgs<IDataTypeDefinition> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveDataTypeCache(x));
        }


        #endregion

        #region Stylesheet and stylesheet property event handlers

        static void FileService_DeletedStylesheet(IFileService sender, DeleteEventArgs<Stylesheet> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveStylesheetCache(x));
        }

        static void FileService_SavedStylesheet(IFileService sender, SaveEventArgs<Stylesheet> e)
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
        static void LocalizationService_DeletedLanguage(ILocalizationService sender, DeleteEventArgs<ILanguage> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveLanguageCache(x));
        }

        /// <summary>
        /// Fires when a langauge is saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void LocalizationService_SavedLanguage(ILocalizationService sender, SaveEventArgs<ILanguage> e)
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
        static void ContentTypeService_DeletedMediaType(IContentTypeService sender, DeleteEventArgs<IMediaType> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveMediaTypeCache(x));
        }

        /// <summary>
        /// Fires when a content type is deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ContentTypeService_DeletedContentType(IContentTypeService sender, DeleteEventArgs<IContentType> e)
        {
            e.DeletedEntities.ForEach(contentType => DistributedCache.Instance.RemoveContentTypeCache(contentType));
        }

        /// <summary>
        /// Fires when a member type is deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void MemberTypeService_Deleted(IMemberTypeService sender, DeleteEventArgs<IMemberType> e)
        {
            e.DeletedEntities.ForEach(contentType => DistributedCache.Instance.RemoveMemberTypeCache(contentType));
        }

        /// <summary>
        /// Fires when a media type is saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ContentTypeService_SavedMediaType(IContentTypeService sender, SaveEventArgs<IMediaType> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshMediaTypeCache(x));
        }

        /// <summary>
        /// Fires when a content type is saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ContentTypeService_SavedContentType(IContentTypeService sender, SaveEventArgs<IContentType> e)
        {
            e.SavedEntities.ForEach(contentType => DistributedCache.Instance.RefreshContentTypeCache(contentType));
        }

        /// <summary>
        /// Fires when a member type is saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void MemberTypeService_Saved(IMemberTypeService sender, SaveEventArgs<IMemberType> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshMemberTypeCache(x));
        }


        #endregion

        #region User/permissions event handlers
        
        //fixme: this isn't named correct
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

        static void UserService_SavedUser(IUserService sender, SaveEventArgs<IUser> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshUserCache(x.Id));
        }

        static void UserService_DeletedUser(IUserService sender, DeleteEventArgs<IUser> e)
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
        static void FileService_DeletedTemplate(IFileService sender, DeleteEventArgs<ITemplate> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveTemplateCache(x.Id));
        }

        /// <summary>
        /// Refresh cache for template
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void FileService_SavedTemplate(IFileService sender, SaveEventArgs<ITemplate> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshTemplateCache(x.Id));
        }

        #endregion

        #region Macro event handlers

        static void MacroService_Deleted(IMacroService sender, DeleteEventArgs<IMacro> e)
        {
            foreach (var entity in e.DeletedEntities)
            {
                DistributedCache.Instance.RemoveMacroCache(entity);
            }
        }

        static void MacroService_Saved(IMacroService sender, SaveEventArgs<IMacro> e)
        {
            foreach (var entity in e.SavedEntities)
            {
                DistributedCache.Instance.RefreshMacroCache(entity);
            }
        }

        #endregion

        #region Media event handlers

        static void MediaService_EmptiedRecycleBin(IMediaService sender, RecycleBinEventArgs e)
        {
            if (e.RecycleBinEmptiedSuccessfully && e.IsMediaRecycleBin)
            {
                DistributedCache.Instance.RemoveMediaCachePermanently(e.Ids.ToArray());
            }
        }

        static void MediaService_Trashed(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            DistributedCache.Instance.RemoveMediaCacheAfterRecycling(e.MoveInfoCollection.ToArray());
        }

        static void MediaService_Moved(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            DistributedCache.Instance.RefreshMediaCacheAfterMoving(e.MoveInfoCollection.ToArray());
        }

        static void MediaService_Deleted(IMediaService sender, DeleteEventArgs<IMedia> e)
        {
            DistributedCache.Instance.RemoveMediaCachePermanently(e.DeletedEntities.Select(x => x.Id).ToArray());
        }

        static void MediaService_Saved(IMediaService sender, SaveEventArgs<IMedia> e)
        {
            DistributedCache.Instance.RefreshMediaCache(e.SavedEntities.ToArray());
        }
        #endregion

        #region Member event handlers

        static void MemberService_Deleted(IMemberService sender, DeleteEventArgs<IMember> e)
        {
            DistributedCache.Instance.RemoveMemberCache(e.DeletedEntities.ToArray());
        }

        static void MemberService_Saved(IMemberService sender, SaveEventArgs<IMember> e)
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

        #region Relation type event handlers

        static void RelationService_SavedRelationType(IRelationService sender, SaveEventArgs<IRelationType> args)
        {
            var dc = DistributedCache.Instance;
            foreach (var e in args.SavedEntities)
                dc.RefreshRelationTypeCache(e.Id);
        }

        static void RelationService_DeletedRelationType(IRelationService sender, DeleteEventArgs<IRelationType> args)
        {
            var dc = DistributedCache.Instance;
            foreach (var e in args.DeletedEntities)
                dc.RemoveRelationTypeCache(e.Id);
        }

        #endregion


        /// <summary>
        /// This will inspect the event metadata and execute it's affiliated handler if one is found
        /// </summary>
        /// <param name="events"></param>
        internal static void HandleEvents(IEnumerable<IEventDefinition> events)
        {
            foreach (var e in events)
            {
                var handler = FindHandler(e);
                if (handler == null) continue;

                handler.Invoke(null, new object[] { e.Sender, e.Args });
            }
        }

        /// <summary>
        /// Used to cache all candidate handlers
        /// </summary>
        private static readonly Lazy<MethodInfo[]> CandidateHandlers = new Lazy<MethodInfo[]>(() =>
        {
            var candidates =

               typeof(CacheRefresherEventHandler).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
               .Where(x => x.Name.Contains("_"))
               .Select(x => new
               {
                   method = x,
                   nameParts = x.Name.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries),
                   methodParams = x.GetParameters()
               })
               .Where(x => x.nameParts.Length == 2 && x.methodParams.Length == 2 && typeof(EventArgs).IsAssignableFrom(x.methodParams[1].ParameterType))
               .Select(x => x.method)
               .ToArray();

            return candidates;
        });

        /// <summary>
        /// Used to cache all found event handlers
        /// </summary>
        private static readonly ConcurrentDictionary<IEventDefinition, MethodInfo> FoundHandlers = new ConcurrentDictionary<IEventDefinition, MethodInfo>();

        internal static MethodInfo FindHandler(IEventDefinition eventDefinition)
        {
            return FoundHandlers.GetOrAdd(eventDefinition, definition =>
            {
                var candidates = CandidateHandlers.Value;

                var found = candidates.FirstOrDefault(x => x.Name == string.Format("{0}_{1}", eventDefinition.Sender.GetType().Name, eventDefinition.EventName));

                return found;
            });
        }
    }
}