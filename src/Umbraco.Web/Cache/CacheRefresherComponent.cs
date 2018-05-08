﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core.Components;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services.Changes;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.Composing;
using Umbraco.Web.Security;
using Umbraco.Web.Services;
using LightInject;
using ApplicationTree = Umbraco.Core.Models.ApplicationTree;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Installs listeners on service events in order to refresh our caches.
    /// </summary>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    [RequiredComponent(typeof(IUmbracoCoreComponent))] // runs before every other IUmbracoCoreComponent!
    public class CacheRefresherComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        private static readonly ConcurrentDictionary<string, MethodInfo> FoundHandlers = new ConcurrentDictionary<string, MethodInfo>();
        private DistributedCache _distributedCache;
        private List<Action> _unbinders;

        public CacheRefresherComponent()
        { }

        // for tests
        public CacheRefresherComponent(bool supportUnbinding)
        {
            if (supportUnbinding)
                _unbinders = new List<Action>();
        }

        public void Initialize(DistributedCache distributedCache)
        {
            Current.Logger.Info<CacheRefresherComponent>("Initializing Umbraco internal event handlers for cache refreshing.");

            _distributedCache = distributedCache;

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

            // bind to user and user group events
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
            // fixme why not in v8?
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
            Bind(() => ContentTypeService.Changed += ContentTypeService_Changed,
                () => ContentTypeService.Changed -= ContentTypeService_Changed);
            Bind(() => MediaTypeService.Changed += MediaTypeService_Changed,
                () => MediaTypeService.Changed -= MediaTypeService_Changed);
            Bind(() => MemberTypeService.Changed += MemberTypeService_Changed,
                () => MemberTypeService.Changed -= MemberTypeService_Changed);

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

            // bind to media events - handles all media changes
            Bind(() => MediaService.TreeChanged += MediaService_Changed,
                () => MediaService.TreeChanged -= MediaService_Changed);

            // bind to content events
            Bind(() => ContentService.Saved += ContentService_Saved, // needed for permissions
                () => ContentService.Saved -= ContentService_Saved);
            Bind(() => ContentService.Copied += ContentService_Copied, // needed for permissions
                () => ContentService.Copied -= ContentService_Copied);
            Bind(() => ContentService.TreeChanged += ContentService_Changed,// handles all content changes
                () => ContentService.TreeChanged -= ContentService_Changed);

            // TreeChanged should also deal with this
            //Bind(() => ContentService.SavedBlueprint += ContentService_SavedBlueprint,
            //    () => ContentService.SavedBlueprint -= ContentService_SavedBlueprint);
            //Bind(() => ContentService.DeletedBlueprint += ContentService_DeletedBlueprint,
            //    () => ContentService.DeletedBlueprint -= ContentService_DeletedBlueprint);

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

        #region Events binding and handling

        private void Bind(Action binder, Action unbinder)
        {
            // bind now
            binder();

            // and register unbinder for later, if needed
            _unbinders?.Add(unbinder);
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

        internal static MethodInfo FindHandler(IEventDefinition eventDefinition)
        {
            var name = eventDefinition.Sender.GetType().Name + "_" + eventDefinition.EventName;

            return FoundHandlers.GetOrAdd(name, n => CandidateHandlers.Value.FirstOrDefault(x => x.Name == n));
        }

        private static readonly Lazy<MethodInfo[]> CandidateHandlers = new Lazy<MethodInfo[]>(() =>
        {
            var underscore = new[] { '_' };

            return typeof(CacheRefresherComponent)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(x =>
                {
                    if (x.Name.Contains("_") == false) return null;

                    var parts = x.Name.Split(underscore, StringSplitOptions.RemoveEmptyEntries).Length;
                    if (parts != 2) return null;

                    var parameters = x.GetParameters();
                    if (parameters.Length != 2) return null;
                    if (typeof(EventArgs).IsAssignableFrom(parameters[1].ParameterType) == false) return null;
                    return x;
                })
                .WhereNotNull()
                .ToArray();
        });

        internal static void HandleEvents(IEnumerable<IEventDefinition> events)
        {
            // fixme remove this in v8, this is a backwards compat hack and is needed because when we are using Deploy, the events will be raised on a background
            //thread which means that cache refreshers will also execute on a background thread and in many cases developers may be using UmbracoContext.Current in their
            //cache refresher handlers, so before we execute all of the events, we'll ensure a context
            UmbracoContext tempContext = null;
            if (UmbracoContext.Current == null)
            {
                var httpContext = new HttpContextWrapper(HttpContext.Current ?? new HttpContext(new SimpleWorkerRequest("temp.aspx", "", new StringWriter())));
                tempContext = UmbracoContext.EnsureContext(
                    Current.UmbracoContextAccessor,
                    httpContext,
                    null,
                    new WebSecurity(httpContext, Current.Services.UserService, UmbracoConfig.For.GlobalSettings()),
                    UmbracoConfig.For.UmbracoSettings(),
                    Current.UrlProviders,
                    UmbracoConfig.For.GlobalSettings(),
                    Current.Container.GetInstance<IVariationContextAccessor>(),
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
                tempContext?.Dispose();
            }
        }

        #endregion

        #region PublicAccessService

        private void PublicAccessService_Saved(IPublicAccessService sender, SaveEventArgs<PublicAccessEntry> e)
        {
            _distributedCache.RefreshPublicAccess();
        }

        private void PublicAccessService_Deleted(IPublicAccessService sender, DeleteEventArgs<PublicAccessEntry> e)
        {
            _distributedCache.RefreshPublicAccess();
        }

        #endregion

        #region ContentService

        /// <summary>
        /// Handles cache refreshing for when content is copied
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// When an entity is copied new permissions may be assigned to it based on it's parent, if that is the
        /// case then we need to clear all user permissions cache.
        /// </remarks>
        private void ContentService_Copied(IContentService sender, CopyEventArgs<IContent> e)
        { }

        /// <summary>
        /// Handles cache refreshing for when content is saved (not published)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// When an entity is saved we need to notify other servers about the change in order for the Examine indexes to
        /// stay up-to-date for unpublished content.
        /// </remarks>
        private void ContentService_Saved(IContentService sender, SaveEventArgs<IContent> e)
        { }

        private void ContentService_Changed(IContentService sender, TreeChange<IContent>.EventArgs args)
        {
            _distributedCache.RefreshContentCache(args.Changes.ToArray());
        }

        // fixme our weird events handling wants this for now
        private void ContentService_Deleted(IContentService sender, DeleteEventArgs<IContent> e) { }
        private void ContentService_Moved(IContentService sender, MoveEventArgs<IContent> e) { }
        private void ContentService_Trashed(IContentService sender, MoveEventArgs<IContent> e) { }
        private void ContentService_EmptiedRecycleBin(IContentService sender, RecycleBinEventArgs e) { }
        private void ContentService_Published(IContentService sender, PublishEventArgs<IContent> e) { }
        private void ContentService_UnPublished(IContentService sender, PublishEventArgs<IContent> e) { }

        //private void ContentService_SavedBlueprint(IContentService sender, SaveEventArgs<IContent> e)
        //{
        //    _distributedCache.RefreshUnpublishedPageCache(e.SavedEntities.ToArray());
        //}

        //private void ContentService_DeletedBlueprint(IContentService sender, DeleteEventArgs<IContent> e)
        //{
        //    _distributedCache.RemoveUnpublishedPageCache(e.DeletedEntities.ToArray());
        //}

        #endregion

        #region ApplicationTreeService

        private void ApplicationTreeService_New(ApplicationTree sender, EventArgs e)
        {
            _distributedCache.RefreshAllApplicationTreeCache();
        }

        private void ApplicationTreeService_Updated(ApplicationTree sender, EventArgs e)
        {
            _distributedCache.RefreshAllApplicationTreeCache();
        }

        private void ApplicationTreeService_Deleted(ApplicationTree sender, EventArgs e)
        {
            _distributedCache.RefreshAllApplicationTreeCache();
        }

        #endregion

        #region Application event handlers

        private void SectionService_New(Section sender, EventArgs e)
        {
            _distributedCache.RefreshAllApplicationCache();
        }

        private void SectionService_Deleted(Section sender, EventArgs e)
        {
            _distributedCache.RefreshAllApplicationCache();
        }

        #endregion

        #region LocalizationService / Dictionary

        private void LocalizationService_SavedDictionaryItem(ILocalizationService sender, SaveEventArgs<IDictionaryItem> e)
        {
            foreach (var entity in e.SavedEntities)
                _distributedCache.RefreshDictionaryCache(entity.Id);
        }

        private void LocalizationService_DeletedDictionaryItem(ILocalizationService sender, DeleteEventArgs<IDictionaryItem> e)
        {
            foreach (var entity in e.DeletedEntities)
                _distributedCache.RemoveDictionaryCache(entity.Id);
        }

        #endregion

        #region DataTypeService

        private void DataTypeService_Saved(IDataTypeService sender, SaveEventArgs<IDataType> e)
        {
            foreach (var entity in e.SavedEntities)
                _distributedCache.RefreshDataTypeCache(entity);
        }

        private void DataTypeService_Deleted(IDataTypeService sender, DeleteEventArgs<IDataType> e)
        {
            foreach (var entity in e.DeletedEntities)
                _distributedCache.RemoveDataTypeCache(entity);
        }

        #endregion

        #region DomainService

        private void DomainService_Saved(IDomainService sender, SaveEventArgs<IDomain> e)
        {
            foreach (var entity in e.SavedEntities)
                _distributedCache.RefreshDomainCache(entity);
        }

        private void DomainService_Deleted(IDomainService sender, DeleteEventArgs<IDomain> e)
        {
            foreach (var entity in e.DeletedEntities)
                _distributedCache.RemoveDomainCache(entity);
        }

        #endregion

        #region LocalizationService / Language

        /// <summary>
        /// Fires when a langauge is deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocalizationService_DeletedLanguage(ILocalizationService sender, DeleteEventArgs<ILanguage> e)
        {
            foreach (var entity in e.DeletedEntities)
                _distributedCache.RemoveLanguageCache(entity);
        }

        /// <summary>
        /// Fires when a langauge is saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocalizationService_SavedLanguage(ILocalizationService sender, SaveEventArgs<ILanguage> e)
        {
            foreach (var entity in e.SavedEntities)
                _distributedCache.RefreshLanguageCache(entity);
        }

        #endregion

        #region Content|Media|MemberTypeService

        private void ContentTypeService_Changed(IContentTypeService sender, ContentTypeChange<IContentType>.EventArgs args)
        {
            _distributedCache.RefreshContentTypeCache(args.Changes.ToArray());
        }

        private void MediaTypeService_Changed(IMediaTypeService sender, ContentTypeChange<IMediaType>.EventArgs args)
        {
            _distributedCache.RefreshContentTypeCache(args.Changes.ToArray());
        }

        private void MemberTypeService_Changed(IMemberTypeService sender, ContentTypeChange<IMemberType>.EventArgs args)
        {
            _distributedCache.RefreshContentTypeCache(args.Changes.ToArray());
        }

        // fixme our weird events handling wants this for now
        private void ContentTypeService_Saved(IContentTypeService sender, SaveEventArgs<IContentType> args) { }
        private void MediaTypeService_Saved(IMediaTypeService sender, SaveEventArgs<IMediaType> args) { }
        private void MemberTypeService_Saved(IMemberTypeService sender, SaveEventArgs<IMemberType> args) { }
        private void ContentTypeService_Deleted(IContentTypeService sender, DeleteEventArgs<IContentType> args) { }
        private void MediaTypeService_Deleted(IMediaTypeService sender, DeleteEventArgs<IMediaType> args) { }
        private void MemberTypeService_Deleted(IMemberTypeService sender, DeleteEventArgs<IMemberType> args) { }

        #endregion

        #region UserService

        static void UserService_UserGroupPermissionsAssigned(IUserService sender, SaveEventArgs<EntityPermission> e)
        {
            //TODO: Not sure if we need this yet depends if we start caching permissions
            //var groupIds = e.SavedEntities.Select(x => x.UserGroupId).Distinct();
            //foreach (var groupId in groupIds)
            //{
            //    DistributedCache.Instance.RefreshUserGroupPermissionsCache(groupId);
            //}
        }

        private void UserService_SavedUser(IUserService sender, SaveEventArgs<IUser> e)
        {
            foreach (var entity in e.SavedEntities)
                _distributedCache.RefreshUserCache(entity.Id);
        }

        private void UserService_DeletedUser(IUserService sender, DeleteEventArgs<IUser> e)
        {
            foreach (var entity in e.DeletedEntities)
                _distributedCache.RemoveUserCache(entity.Id);
        }

        private void UserService_SavedUserGroup(IUserService sender, SaveEventArgs<UserGroupWithUsers> e)
        {
            foreach (var entity in e.SavedEntities)
                _distributedCache.RefreshUserGroupCache(entity.UserGroup.Id);
        }

        private void UserService_DeletedUserGroup(IUserService sender, DeleteEventArgs<IUserGroup> e)
        {
            foreach (var entity in e.DeletedEntities)
                _distributedCache.RemoveUserGroupCache(entity.Id);
        }

        #endregion

        #region FileService

        /// <summary>
        /// Removes cache for template
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileService_DeletedTemplate(IFileService sender, DeleteEventArgs<ITemplate> e)
        {
            foreach (var entity in e.DeletedEntities)
                _distributedCache.RemoveTemplateCache(entity.Id);
        }

        /// <summary>
        /// Refresh cache for template
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileService_SavedTemplate(IFileService sender, SaveEventArgs<ITemplate> e)
        {
            foreach (var entity in e.SavedEntities)
                _distributedCache.RefreshTemplateCache(entity.Id);
        }

        // fixme our weird events handling wants this for now
        private void FileService_DeletedStylesheet(IFileService sender, DeleteEventArgs<Stylesheet> e) { }
        private void FileService_SavedStylesheet(IFileService sender, SaveEventArgs<Stylesheet> e) { }

        #endregion

        #region MacroService

        private void MacroService_Deleted(IMacroService sender, DeleteEventArgs<IMacro> e)
        {
            foreach (var entity in e.DeletedEntities)
                _distributedCache.RemoveMacroCache(entity);
        }

        private void MacroService_Saved(IMacroService sender, SaveEventArgs<IMacro> e)
        {
            foreach (var entity in e.SavedEntities)
                _distributedCache.RefreshMacroCache(entity);
        }

        #endregion

        #region MediaService

        private void MediaService_Changed(IMediaService sender, TreeChange<IMedia>.EventArgs args)
        {
            _distributedCache.RefreshMediaCache(args.Changes.ToArray());
        }

        // fixme our weird events handling wants this for now
        private void MediaService_Saved(IMediaService sender, SaveEventArgs<IMedia> e) { }
        private void MediaService_Deleted(IMediaService sender, DeleteEventArgs<IMedia> e) { }
        private void MediaService_Moved(IMediaService sender, MoveEventArgs<IMedia> e) { }
        private void MediaService_Trashed(IMediaService sender, MoveEventArgs<IMedia> e) { }
        private void MediaService_EmptiedRecycleBin(IMediaService sender, RecycleBinEventArgs e) { }

        #endregion

        #region MemberService

        private void MemberService_Deleted(IMemberService sender, DeleteEventArgs<IMember> e)
        {
            _distributedCache.RemoveMemberCache(e.DeletedEntities.ToArray());
        }

        private void MemberService_Saved(IMemberService sender, SaveEventArgs<IMember> e)
        {
            _distributedCache.RefreshMemberCache(e.SavedEntities.ToArray());
        }

        #endregion

        #region MemberGroupService

        private void MemberGroupService_Deleted(IMemberGroupService sender, DeleteEventArgs<IMemberGroup> e)
        {
            foreach (var m in e.DeletedEntities.ToArray())
            {
                _distributedCache.RemoveMemberGroupCache(m.Id);
            }
        }

        private void MemberGroupService_Saved(IMemberGroupService sender, SaveEventArgs<IMemberGroup> e)
        {
            foreach (var m in e.SavedEntities.ToArray())
            {
                _distributedCache.RemoveMemberGroupCache(m.Id);
            }
        }
        #endregion

        #region RelationType

        private void RelationService_SavedRelationType(IRelationService sender, SaveEventArgs<IRelationType> args)
        {
            var dc = _distributedCache;
            foreach (var e in args.SavedEntities)
                dc.RefreshRelationTypeCache(e.Id);
        }

        private void RelationService_DeletedRelationType(IRelationService sender, DeleteEventArgs<IRelationType> args)
        {
            var dc = _distributedCache;
            foreach (var e in args.DeletedEntities)
                dc.RemoveRelationTypeCache(e.Id);
        }

        #endregion
    }
}
