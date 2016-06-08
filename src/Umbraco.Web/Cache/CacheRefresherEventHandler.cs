using System;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Services;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Services.Changes;
using Umbraco.Web.Services;
using Content = Umbraco.Core.Models.Content;
using ApplicationTree = Umbraco.Core.Models.ApplicationTree;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Installs listeners on service events in order to refresh our caches.
    /// </summary>
    public class CacheRefresherEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            LogHelper.Info<CacheRefresherEventHandler>("Initializing Umbraco internal event handlers for cache refreshing.");
            AddHandlers();
        }

        internal void AddHandlers()
        {
            // bind to application tree events
            ApplicationTreeService.Deleted += ApplicationTreeDeleted;
            ApplicationTreeService.Updated += ApplicationTreeUpdated;
            ApplicationTreeService.New += ApplicationTreeNew;

            // bind to application events
            SectionService.Deleted += ApplicationDeleted;
            SectionService.New += ApplicationNew;

            // bind to user / user type events
            UserService.SavedUserType += UserServiceSavedUserType;
            UserService.DeletedUserType += UserServiceDeletedUserType;
            UserService.SavedUser += UserServiceSavedUser;
            UserService.DeletedUser += UserServiceDeletedUser;

            // bind to dictionary events
            LocalizationService.DeletedDictionaryItem += LocalizationServiceDeletedDictionaryItem;
            LocalizationService.SavedDictionaryItem += LocalizationServiceSavedDictionaryItem;

            // bind to data type events
            // NOTE: we need to bind to legacy and new API events currently: http://issues.umbraco.org/issue/U4-1979
            DataTypeService.Deleted += DataTypeServiceDeleted;
            DataTypeService.Saved += DataTypeServiceSaved;

            // bind to stylesheet events
            //FileService.SavedStylesheet += FileServiceSavedStylesheet;
            //FileService.DeletedStylesheet += FileServiceDeletedStylesheet;

            // bind to domain events
            DomainService.Saved += DomainServiceSaved;
            DomainService.Deleted += DomainServiceDeleted;

            // bind to language events
            LocalizationService.SavedLanguage += LocalizationServiceSavedLanguage;
            LocalizationService.DeletedLanguage += LocalizationServiceDeletedLanguage;

            // bind to content type events
            ContentTypeService.Changed += ContentTypeServiceChanged;
            MediaTypeService.Changed += ContentTypeServiceChanged;
            MemberTypeService.Changed += ContentTypeServiceChanged;

            // bind to permission events
            PermissionRepository<IContent>.AssignedPermissions += PermissionRepositoryAssignedPermissions;

            // bind to template events
            FileService.SavedTemplate += FileServiceSavedTemplate;
            FileService.DeletedTemplate += FileServiceDeletedTemplate;

            // bind to macro events
            MacroService.Saved += MacroServiceSaved;
            MacroService.Deleted += MacroServiceDeleted;

            // bind to member events
            MemberService.Saved += MemberServiceSaved;
            MemberService.Deleted += MemberServiceDeleted;
            MemberGroupService.Saved += MemberGroupServiceSaved;
            MemberGroupService.Deleted += MemberGroupServiceDeleted;

            // bind to media events
            MediaService.TreeChanged += MediaServiceChanged; // handles all media changes

            // bind to content events
            ContentService.Saved += ContentServiceSaved; // needed for permissions
            ContentService.Copied += ContentServiceCopied; // needed for permissions
            ContentService.TreeChanged += ContentServiceChanged; // handles all content changes

            // bind to public access events
            PublicAccessService.Saved += PublicAccessServiceSaved;
            PublicAccessService.Deleted += PublicAccessServiceDeleted;
        }

        internal void RemoveHandlers()
        {
            // bind to application tree events
            ApplicationTreeService.Deleted -= ApplicationTreeDeleted;
            ApplicationTreeService.Updated -= ApplicationTreeUpdated;
            ApplicationTreeService.New -= ApplicationTreeNew;

            // bind to application events
            SectionService.Deleted -= ApplicationDeleted;
            SectionService.New -= ApplicationNew;

            // bind to user / user type events
            UserService.SavedUserType -= UserServiceSavedUserType;
            UserService.DeletedUserType -= UserServiceDeletedUserType;
            UserService.SavedUser -= UserServiceSavedUser;
            UserService.DeletedUser -= UserServiceDeletedUser;

            // bind to dictionary events
            LocalizationService.DeletedDictionaryItem -= LocalizationServiceDeletedDictionaryItem;
            LocalizationService.SavedDictionaryItem -= LocalizationServiceSavedDictionaryItem;

            // bind to data type events
            // NOTE: we need to bind to legacy and new API events currently: http://issues.umbraco.org/issue/U4-1979
            DataTypeService.Deleted -= DataTypeServiceDeleted;
            DataTypeService.Saved -= DataTypeServiceSaved;

            // bind to stylesheet events
            //FileService.SavedStylesheet -= FileServiceSavedStylesheet;
            //FileService.DeletedStylesheet -= FileServiceDeletedStylesheet;

            // bind to domain events
            DomainService.Saved -= DomainServiceSaved;
            DomainService.Deleted -= DomainServiceDeleted;

            // bind to language events
            LocalizationService.SavedLanguage -= LocalizationServiceSavedLanguage;
            LocalizationService.DeletedLanguage -= LocalizationServiceDeletedLanguage;

            // bind to content type events
            ContentTypeService.Changed -= ContentTypeServiceChanged;
            MediaTypeService.Changed -= ContentTypeServiceChanged;
            MemberTypeService.Changed -= ContentTypeServiceChanged;

            // bind to permission events
            PermissionRepository<IContent>.AssignedPermissions -= PermissionRepositoryAssignedPermissions;

            // bind to template events
            FileService.SavedTemplate -= FileServiceSavedTemplate;
            FileService.DeletedTemplate -= FileServiceDeletedTemplate;

            // bind to macro events
            MacroService.Saved -= MacroServiceSaved;
            MacroService.Deleted -= MacroServiceDeleted;

            // bind to member events
            MemberService.Saved -= MemberServiceSaved;
            MemberService.Deleted -= MemberServiceDeleted;
            MemberGroupService.Saved -= MemberGroupServiceSaved;
            MemberGroupService.Deleted -= MemberGroupServiceDeleted;

            // bind to media events
            MediaService.TreeChanged -= MediaServiceChanged; // handles all media changes

            // bind to content events
            ContentService.Saved -= ContentServiceSaved; // needed for permissions
            ContentService.Copied -= ContentServiceCopied; // needed for permissions
            ContentService.TreeChanged -= ContentServiceChanged; // handles all content changes

            // bind to public access events
            PublicAccessService.Saved -= PublicAccessServiceSaved;
            PublicAccessService.Deleted -= PublicAccessServiceDeleted;
        }

        #region PublicAccessService

        static void PublicAccessServiceSaved(IPublicAccessService sender, SaveEventArgs<PublicAccessEntry> e)
        {
            DistributedCache.Instance.RefreshPublicAccess();
        }

        private void PublicAccessServiceDeleted(IPublicAccessService sender, DeleteEventArgs<PublicAccessEntry> e)
        {
            DistributedCache.Instance.RefreshPublicAccess();
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
        static void ContentServiceCopied(IContentService sender, CopyEventArgs<IContent> e)
        {
            //check if permissions have changed
            var permissionsChanged = ((Content)e.Copy).WasPropertyDirty("PermissionsChanged");
            if (permissionsChanged)
            {
                DistributedCache.Instance.RefreshAllUserPermissionsCache();
            }
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
        }

        private static void ContentServiceChanged(IContentService sender, TreeChange<IContent>.EventArgs args)
        {
            DistributedCache.Instance.RefreshContentCache(args.Changes.ToArray());
        }

        #endregion

        #region ApplicationTreeService

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

        #region UserService / UserType

        static void UserServiceDeletedUserType(IUserService sender, DeleteEventArgs<IUserType> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveUserTypeCache(x.Id));
        }

        static void UserServiceSavedUserType(IUserService sender, SaveEventArgs<IUserType> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshUserTypeCache(x.Id));
        }

        #endregion

        #region LocalizationService / Dictionary

        static void LocalizationServiceSavedDictionaryItem(ILocalizationService sender, SaveEventArgs<IDictionaryItem> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshDictionaryCache(x.Id));
        }

        static void LocalizationServiceDeletedDictionaryItem(ILocalizationService sender, DeleteEventArgs<IDictionaryItem> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveDictionaryCache(x.Id));
        }

        #endregion

        #region DataTypeService
        static void DataTypeServiceSaved(IDataTypeService sender, SaveEventArgs<IDataTypeDefinition> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshDataTypeCache(x));
        }

        static void DataTypeServiceDeleted(IDataTypeService sender, DeleteEventArgs<IDataTypeDefinition> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveDataTypeCache(x));
        }

        #endregion

        #region DomainService

        static void DomainServiceSaved(IDomainService sender, SaveEventArgs<IDomain> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshDomainCache(x));
        }

        static void DomainServiceDeleted(IDomainService sender, DeleteEventArgs<IDomain> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveDomainCache(x));
        }

        #endregion

        #region LocalizationService / Language
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

        #region Content|Media|MemberTypeService

        private void ContentTypeServiceChanged(IContentTypeService sender, ContentTypeChange<IContentType>.EventArgs args)
        {
            DistributedCache.Instance.RefreshContentTypeCache(args.Changes.ToArray());
        }

        private void ContentTypeServiceChanged(IMediaTypeService sender, ContentTypeChange<IMediaType>.EventArgs args)
        {
            DistributedCache.Instance.RefreshContentTypeCache(args.Changes.ToArray());
        }

        private void ContentTypeServiceChanged(IMemberTypeService sender, ContentTypeChange<IMemberType>.EventArgs args)
        {
            DistributedCache.Instance.RefreshContentTypeCache(args.Changes.ToArray());
        }

        #endregion

        #region UserService & PermissionRepository

        static void PermissionRepositoryAssignedPermissions(PermissionRepository<IContent> sender, SaveEventArgs<EntityPermission> e)
        {
            var userIds = e.SavedEntities.Select(x => x.UserId).Distinct();
            userIds.ForEach(x => DistributedCache.Instance.RefreshUserPermissionsCache(x));
        }

        static void UserServiceSavedUser(IUserService sender, SaveEventArgs<IUser> e)
        {
            e.SavedEntities.ForEach(x => DistributedCache.Instance.RefreshUserCache(x.Id));
        }

        static void UserServiceDeletedUser(IUserService sender, DeleteEventArgs<IUser> e)
        {
            e.DeletedEntities.ForEach(x => DistributedCache.Instance.RemoveUserCache(x.Id));
        }

        #endregion

        #region FileService / Template

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

        #region MacroService

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

        #region MediaService

        private static void MediaServiceChanged(IMediaService sender, TreeChange<IMedia>.EventArgs args)
        {
            DistributedCache.Instance.RefreshMediaCache(args.Changes.ToArray());
        }

        #endregion

        #region MemberService

        static void MemberServiceDeleted(IMemberService sender, DeleteEventArgs<IMember> e)
        {
            DistributedCache.Instance.RemoveMemberCache(e.DeletedEntities.ToArray());
        }

        static void MemberServiceSaved(IMemberService sender, SaveEventArgs<IMember> e)
        {
            DistributedCache.Instance.RefreshMemberCache(e.SavedEntities.ToArray());
        }

        #endregion

        #region MemberGroupService

        static void MemberGroupServiceDeleted(IMemberGroupService sender, DeleteEventArgs<IMemberGroup> e)
        {
            foreach (var m in e.DeletedEntities.ToArray())
            {
                DistributedCache.Instance.RemoveMemberGroupCache(m.Id);
            }
        }

        static void MemberGroupServiceSaved(IMemberGroupService sender, SaveEventArgs<IMemberGroup> e)
        {
            foreach (var m in e.SavedEntities.ToArray())
            {
                DistributedCache.Instance.RemoveMemberGroupCache(m.Id);
            }
        }
        #endregion
    }
}