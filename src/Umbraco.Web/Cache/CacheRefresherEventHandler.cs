using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
using System.Web;
using System.Web.Hosting;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Publishing;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
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
        public CacheRefresherEventHandler()
        { }

        public CacheRefresherEventHandler(bool supportUnbinding)
        {
            if (supportUnbinding)
                _unbinders = new List<Action>();
        }

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            LogHelper.Info<CacheRefresherEventHandler>("Initializing Umbraco internal event handlers for cache refreshing");

            // bind to application tree events
            Bind(() => ApplicationTreeService.Deleted += ApplicationTreeService_Deleted,
                 () => ApplicationTreeService.Deleted -= ApplicationTreeService_Deleted);
            Bind(() => ApplicationTreeService.Updated += ApplicationTreeService_Updated,
                 () => ApplicationTreeService.Updated -= ApplicationTreeService_Updated);
            Bind(() => ApplicationTreeService.New += ApplicationTreeService_New,
                 () => ApplicationTreeService.New -= ApplicationTreeService_New);

            // bind to application events
            Bind(() => SectionService.Deleted += SectionService_Deleted,
                 () => SectionService.Deleted -= SectionService_Deleted);
            Bind(() => SectionService.New += SectionService_New,
                 () => SectionService.New -= SectionService_New);
            
            // bind to user and user / user group events
            Bind(() => UserService.SavedUserGroup += UserService_SavedUserGroup,
                 () => UserService.SavedUserGroup -= UserService_SavedUserGroup);
            Bind(() => UserService.DeletedUserGroup += UserService_DeletedUserGroup,
                 () => UserService.DeletedUserGroup -= UserService_DeletedUserGroup);
            Bind(() => UserService.SavedUser += UserService_SavedUser,
                 () => UserService.SavedUser -= UserService_SavedUser);
            Bind(() => UserService.DeletedUser += UserService_DeletedUser,
                 () => UserService.DeletedUser -= UserService_DeletedUser);
            Bind(() => UserService.UserGroupPermissionsAssigned += UserService_UserGroupPermissionsAssigned,
                 () => UserService.UserGroupPermissionsAssigned -= UserService_UserGroupPermissionsAssigned);

            // bind to dictionary events
            Bind(() => LocalizationService.DeletedDictionaryItem += LocalizationService_DeletedDictionaryItem,
                 () => LocalizationService.DeletedDictionaryItem -= LocalizationService_DeletedDictionaryItem);
            Bind(() => LocalizationService.SavedDictionaryItem += LocalizationService_SavedDictionaryItem,
                 () => LocalizationService.SavedDictionaryItem -= LocalizationService_SavedDictionaryItem);

            // bind to data type events
            Bind(() => DataTypeService.Deleted += DataTypeService_Deleted,
                 () => DataTypeService.Deleted -= DataTypeService_Deleted);
            Bind(() => DataTypeService.Saved += DataTypeService_Saved,
                 () => DataTypeService.Saved -= DataTypeService_Saved);

            // bind to stylesheet events
            Bind(() => FileService.SavedStylesheet += FileService_SavedStylesheet,
                 () => FileService.SavedStylesheet -= FileService_SavedStylesheet);
            Bind(() => FileService.DeletedStylesheet += FileService_DeletedStylesheet,
                 () => FileService.DeletedStylesheet -= FileService_DeletedStylesheet);

            // bind to domain events
            Bind(() => DomainService.Saved += DomainService_Saved,
                 () => DomainService.Saved -= DomainService_Saved);
            Bind(() => DomainService.Deleted += DomainService_Deleted,
                 () => DomainService.Deleted -= DomainService_Deleted);

            // bind to language events
            Bind(() => LocalizationService.SavedLanguage += LocalizationService_SavedLanguage,
                 () => LocalizationService.SavedLanguage -= LocalizationService_SavedLanguage);
            Bind(() => LocalizationService.DeletedLanguage += LocalizationService_DeletedLanguage,
                 () => LocalizationService.DeletedLanguage -= LocalizationService_DeletedLanguage);

            // bind to content type events
            Bind(() => ContentTypeService.SavedContentType += ContentTypeService_SavedContentType,
                 () => ContentTypeService.SavedContentType -= ContentTypeService_SavedContentType);
            Bind(() => ContentTypeService.SavedMediaType += ContentTypeService_SavedMediaType,
                 () => ContentTypeService.SavedMediaType -= ContentTypeService_SavedMediaType);
            Bind(() => ContentTypeService.DeletedContentType += ContentTypeService_DeletedContentType,
                 () => ContentTypeService.DeletedContentType -= ContentTypeService_DeletedContentType);
            Bind(() => ContentTypeService.DeletedMediaType += ContentTypeService_DeletedMediaType,
                 () => ContentTypeService.DeletedMediaType -= ContentTypeService_DeletedMediaType);
            Bind(() => MemberTypeService.Saved += MemberTypeService_Saved,
                 () => MemberTypeService.Saved -= MemberTypeService_Saved);
            Bind(() => MemberTypeService.Deleted += MemberTypeService_Deleted,
                 () => MemberTypeService.Deleted -= MemberTypeService_Deleted);

           

            // bind to template events
            Bind(() => FileService.SavedTemplate += FileService_SavedTemplate,
                 () => FileService.SavedTemplate -= FileService_SavedTemplate);
            Bind(() => FileService.DeletedTemplate += FileService_DeletedTemplate,
                 () => FileService.DeletedTemplate -= FileService_DeletedTemplate);

            // bind to macro events
            Bind(() => MacroService.Saved += MacroService_Saved,
                 () => MacroService.Saved -= MacroService_Saved);
            Bind(() => MacroService.Deleted += MacroService_Deleted,
                 () => MacroService.Deleted -= MacroService_Deleted);

            // bind to member events
            Bind(() => MemberService.Saved += MemberService_Saved,
                 () => MemberService.Saved -= MemberService_Saved);
            Bind(() => MemberService.Deleted += MemberService_Deleted,
                 () => MemberService.Deleted -= MemberService_Deleted);
            Bind(() => MemberGroupService.Saved += MemberGroupService_Saved,
                 () => MemberGroupService.Saved -= MemberGroupService_Saved);
            Bind(() => MemberGroupService.Deleted += MemberGroupService_Deleted,
                 () => MemberGroupService.Deleted -= MemberGroupService_Deleted);

            // bind to media events
            Bind(() => MediaService.Saved += MediaService_Saved,
                 () => MediaService.Saved -= MediaService_Saved);
            Bind(() => MediaService.Deleted += MediaService_Deleted,
                 () => MediaService.Deleted -= MediaService_Deleted);
            Bind(() => MediaService.Moved += MediaService_Moved,
                 () => MediaService.Moved -= MediaService_Moved);
            Bind(() => MediaService.Trashed += MediaService_Trashed,
                 () => MediaService.Trashed -= MediaService_Trashed);
            Bind(() => MediaService.EmptiedRecycleBin += MediaService_EmptiedRecycleBin,
                 () => MediaService.EmptiedRecycleBin -= MediaService_EmptiedRecycleBin);

            // bind to content events
            // this is for unpublished content syncing across servers (primarily for examine)
            Bind(() => ContentService.Saved += ContentService_Saved,
                 () => ContentService.Saved -= ContentService_Saved);
            Bind(() => ContentService.Deleted += ContentService_Deleted,
                 () => ContentService.Deleted -= ContentService_Deleted);
            Bind(() => ContentService.Copied += ContentService_Copied,
                 () => ContentService.Copied -= ContentService_Copied);
            // the Move method of the content service fires Saved/Published events during its
            // execution so we don't need to listen to moved - this will probably change in due time
            //Bind(() => ContentService.Moved += ContentServiceMoved,
            //     () => ContentService.Moved -= ContentServiceMoved);
            Bind(() => ContentService.Trashed += ContentService_Trashed,
                 () => ContentService.Trashed -= ContentService_Trashed);
            Bind(() => ContentService.EmptiedRecycleBin += ContentService_EmptiedRecycleBin,
                 () => ContentService.EmptiedRecycleBin -= ContentService_EmptiedRecycleBin);
            Bind(() => ContentService.Published += ContentService_Published,
                 () => ContentService.Published -= ContentService_Published);
            Bind(() => ContentService.UnPublished += ContentService_UnPublished,
                 () => ContentService.UnPublished -= ContentService_UnPublished);

            Bind(() => ContentService.SavedBlueprint += ContentService_SavedBlueprint,
                 () => ContentService.SavedBlueprint -= ContentService_SavedBlueprint);
            Bind(() => ContentService.DeletedBlueprint += ContentService_DeletedBlueprint,
                 () => ContentService.DeletedBlueprint -= ContentService_DeletedBlueprint);

            // bind to public access events
            Bind(() => PublicAccessService.Saved += PublicAccessService_Saved,
                 () => PublicAccessService.Saved -= PublicAccessService_Saved);
            Bind(() => PublicAccessService.Deleted += PublicAccessService_Deleted,
                 () => PublicAccessService.Deleted -= PublicAccessService_Deleted);

            // bind to relation type events
            Bind(() => RelationService.SavedRelationType += RelationService_SavedRelationType,
                 () => RelationService.SavedRelationType -= RelationService_SavedRelationType);
            Bind(() => RelationService.DeletedRelationType += RelationService_DeletedRelationType,
                 () => RelationService.DeletedRelationType -= RelationService_DeletedRelationType);
        }

        private List<Action> _unbinders;

        private void Bind(Action binder, Action unbinder)
        {
            // bind now
            binder();

            // abd register unbinder for later, if needed
            if (_unbinders == null) return;
            _unbinders.Add(unbinder);
        }

        // for tests
        internal void Unbind()
        {
            if (_unbinders == null)
                throw new NotSupportedException();
            foreach (var unbinder in _unbinders)
                unbinder();
            _unbinders = null;
        }

        #region Publishing

        // IPublishingStrategy (obsolete) events are proxied into ContentService, which works fine when
        // events are actually raised, but not when they are handled by HandleEvents, so we have to have
        // these proxy methods that are *not* registered against any event *but* used by HandleEvents.

        // ReSharper disable once UnusedMember.Local
        static void PublishingStrategy_UnPublished(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            ContentService_UnPublished(sender, e);
        }

        // ReSharper disable once UnusedMember.Local
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

        static void ContentService_DeletedBlueprint(IContentService sender, DeleteEventArgs<IContent> e)
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
        /// </remarks>
        static void ContentService_Saved(IContentService sender, SaveEventArgs<IContent> e)
        {
            //filter out the entities that have only been saved (not newly published) since
            // newly published ones will be synced with the published page cache refresher
            var unpublished = e.SavedEntities.Where(x => x.JustPublished() == false);
            //run the un-published cache refresher
            DistributedCache.Instance.RefreshUnpublishedPageCache(unpublished.ToArray());
        }

        static void ContentService_SavedBlueprint(IContentService sender, SaveEventArgs<IContent> e)
        {
            DistributedCache.Instance.RefreshUnpublishedPageCache(e.SavedEntities.ToArray());
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

        static void UserService_UserGroupPermissionsAssigned(IUserService sender, SaveEventArgs<EntityPermission> e)
        {
            //TODO: Not sure if we need this yet depends if we start caching permissions
            //var groupIds = e.SavedEntities.Select(x => x.UserGroupId).Distinct();
            //foreach (var groupId in groupIds)
            //{
            //    DistributedCache.Instance.RefreshUserGroupPermissionsCache(groupId);
            //}
        }

        static void UserService_SavedUser(IUserService sender, SaveEventArgs<IUser> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshUserCache(x.Id));
        }

        static void UserService_DeletedUser(IUserService sender, DeleteEventArgs<IUser> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveUserCache(x.Id));
        }

        static void UserService_SavedUserGroup(IUserService sender, SaveEventArgs<IUserGroup> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshUserGroupCache(x.Id));
        }

        static void UserService_DeletedUserGroup(IUserService sender, DeleteEventArgs<IUserGroup> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveUserGroupCache(x.Id));
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
            foreach (var x in e.DeletedEntities)
            {
                DistributedCache.Instance.RemoveTemplateCache(x.Id);
            }
        }

        /// <summary>
        /// Refresh cache for template
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void FileService_SavedTemplate(IFileService sender, SaveEventArgs<ITemplate> e)
        {
            foreach (var x in e.SavedEntities)
            {
                DistributedCache.Instance.RefreshTemplateCache(x.Id);
            }
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
            //TODO: We should remove this in v8, this is a backwards compat hack and is needed because when we are using Deploy, the events will be raised on a background
            //thread which means that cache refreshers will also execute on a background thread and in many cases developers may be using UmbracoContext.Current in their
            //cache refresher handlers, so before we execute all of the events, we'll ensure a context
            UmbracoContext tempContext = null;
            if (UmbracoContext.Current == null)
            {
                var httpContext = new HttpContextWrapper(HttpContext.Current ?? new HttpContext(new SimpleWorkerRequest("temp.aspx", "", new StringWriter())));
                tempContext = UmbracoContext.EnsureContext(
                    httpContext,
                    ApplicationContext.Current,
                    new WebSecurity(httpContext, ApplicationContext.Current),
                    UmbracoConfig.For.UmbracoSettings(),
                    UrlProviderResolver.Current.Providers,
                    true);
            }

            try
            {
                foreach (var e in events)
                {
                    var handler = FindHandler(e);
                    if (handler == null) continue;
                    handler.Invoke(null, new[] { e.Sender, e.Args });
                }
            }
            finally
            {
                if (tempContext != null)
                    tempContext.Dispose(); // nulls the ThreadStatic context
            }
        }

        /// <summary>
        /// Used to cache all candidate handlers
        /// </summary>
        private static readonly Lazy<MethodInfo[]> CandidateHandlers = new Lazy<MethodInfo[]>(() =>
        {
            var underscore = new[] { '_' };

            return typeof (CacheRefresherEventHandler)
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(x =>
                {
                    if (x.Name.Contains("_") == false) return null;

                    var parts = x.Name.Split(underscore, StringSplitOptions.RemoveEmptyEntries).Length;
                    if (parts != 2) return null;

                    var parameters = x.GetParameters();
                    if (parameters.Length != 2) return null;
                    if (typeof (EventArgs).IsAssignableFrom(parameters[1].ParameterType) == false) return null;
                    return x;
                })
                .WhereNotNull()
                .ToArray();
        });

        /// <summary>
        /// Used to cache all found event handlers
        /// </summary>
        private static readonly ConcurrentDictionary<string, MethodInfo> FoundHandlers = new ConcurrentDictionary<string, MethodInfo>();

        internal static MethodInfo FindHandler(IEventDefinition eventDefinition)
        {
            var name = eventDefinition.Sender.GetType().Name + "_" + eventDefinition.EventName;

            return FoundHandlers.GetOrAdd(name, n => CandidateHandlers.Value.FirstOrDefault(x => x.Name == n));
        }
    }
}