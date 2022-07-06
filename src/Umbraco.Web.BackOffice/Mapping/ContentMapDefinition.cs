using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HeyRed.MarkdownSharp;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Mapping;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Mapping
{
    /// <summary>
    /// Declares how model mappings for content
    /// </summary>
    internal class ContentMapDefinition : IMapDefinition
    {
        private readonly CommonMapper _commonMapper;
        private readonly CommonTreeNodeMapper _commonTreeNodeMapper;
        private readonly ICultureDictionary _cultureDictionary;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IFileService _fileService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IPublishedRouter _publishedRouter;
        private readonly ILocalizationService _localizationService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IUserService _userService;
        private readonly IEntityService _entityService;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly IPublishedUrlProvider _publishedUrlProvider;
        private readonly UriUtility _uriUtility;
        private readonly AppCaches _appCaches;
        private readonly TabsAndPropertiesMapper<IContent> _tabsAndPropertiesMapper;
        private readonly ContentSavedStateMapper<ContentPropertyDisplay> _stateMapper;
        private readonly ContentBasicSavedStateMapper<ContentPropertyBasic> _basicStateMapper;
        private readonly ContentVariantMapper _contentVariantMapper;


        public ContentMapDefinition(
            CommonMapper commonMapper,
            CommonTreeNodeMapper commonTreeNodeMapper,
            ICultureDictionary cultureDictionary,
            ILocalizedTextService localizedTextService,
            IContentService contentService,
            IContentTypeService contentTypeService,
            IFileService fileService,
            IUmbracoContextAccessor umbracoContextAccessor,
            IPublishedRouter publishedRouter,
            ILocalizationService localizationService,
            ILoggerFactory loggerFactory,
            IUserService userService,
            IVariationContextAccessor variationContextAccessor,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            UriUtility uriUtility,
            IPublishedUrlProvider publishedUrlProvider,
            IEntityService entityService,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            AppCaches appCaches)
        {
            _commonMapper = commonMapper;
            _commonTreeNodeMapper = commonTreeNodeMapper;
            _cultureDictionary = cultureDictionary;
            _localizedTextService = localizedTextService;
            _contentService = contentService;
            _contentTypeService = contentTypeService;
            _fileService = fileService;
            _umbracoContextAccessor = umbracoContextAccessor;
            _publishedRouter = publishedRouter;
            _localizationService = localizationService;
            _loggerFactory = loggerFactory;
            _userService = userService;
            _entityService = entityService;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
            _variationContextAccessor = variationContextAccessor;
            _uriUtility = uriUtility;
            _publishedUrlProvider = publishedUrlProvider;
            _appCaches = appCaches;

            _tabsAndPropertiesMapper = new TabsAndPropertiesMapper<IContent>(cultureDictionary, localizedTextService, contentTypeBaseServiceProvider);
            _stateMapper = new ContentSavedStateMapper<ContentPropertyDisplay>();
            _basicStateMapper = new ContentBasicSavedStateMapper<ContentPropertyBasic>();
            _contentVariantMapper = new ContentVariantMapper(_localizationService, localizedTextService);
        }

        public void DefineMaps(IUmbracoMapper mapper)
        {
            mapper.Define<IContent, ContentPropertyCollectionDto>((source, context) => new ContentPropertyCollectionDto(), Map);
            mapper.Define<IContent, ContentItemDisplay>((source, context) => new ContentItemDisplay(), Map);
            mapper.Define<IContent, ContentVariantDisplay>((source, context) => new ContentVariantDisplay(), Map);
            mapper.Define<IContent, ContentItemBasic<ContentPropertyBasic>>((source, context) => new ContentItemBasic<ContentPropertyBasic>(), Map);
        }

        // Umbraco.Code.MapAll
        private static void Map(IContent source, ContentPropertyCollectionDto target, MapperContext context)
        {
            target.Properties = context.MapEnumerable<IProperty, ContentPropertyDto>(source.Properties);
        }

        // Umbraco.Code.MapAll -AllowPreview -Errors -PersistedContent
        private void Map(IContent source, ContentItemDisplay target, MapperContext context)
        {
            // Both GetActions and DetermineIsChildOfListView use parent, so get it once here
            // Parent might already be in context, so check there before using content service
            IContent parent;
            if (context.Items.TryGetValue("Parent", out var parentObj) &&
                parentObj is IContent typedParent)
            {
                parent = typedParent;
            }
            else
            {
                parent = _contentService.GetParent(source);
            }

            target.AllowedActions = GetActions(source, parent, context);
            target.AllowedTemplates = GetAllowedTemplates(source);
            target.ContentApps = _commonMapper.GetContentAppsForEntity(source);
            target.ContentTypeId = source.ContentType.Id;
            target.ContentTypeKey = source.ContentType.Key;
            target.ContentTypeAlias = source.ContentType.Alias;
            target.ContentTypeName = _localizedTextService.UmbracoDictionaryTranslate(_cultureDictionary, source.ContentType.Name);
            target.DocumentType = _commonMapper.GetContentType(source, context);
            target.Icon = source.ContentType.Icon;
            target.Id = source.Id;
            target.IsBlueprint = source.Blueprint;
            target.IsChildOfListView = DetermineIsChildOfListView(source, parent, context);
            target.IsContainer = source.ContentType.IsContainer;
            target.IsElement = source.ContentType.IsElement;
            target.Key = source.Key;
            target.Owner = _commonMapper.GetOwner(source, context);
            target.ParentId = source.ParentId;
            target.Path = source.Path;
            target.SortOrder = source.SortOrder;
            target.TemplateAlias = GetDefaultTemplate(source);
            target.TemplateId = source.TemplateId ?? default;
            target.Trashed = source.Trashed;
            target.TreeNodeUrl = _commonTreeNodeMapper.GetTreeNodeUrl<ContentTreeController>(source);
            target.Udi = Udi.Create(source.Blueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document, source.Key);
            target.UpdateDate = source.UpdateDate;
            target.Updater = _commonMapper.GetCreator(source, context);
            target.Urls = GetUrls(source);
            target.Variants = _contentVariantMapper.Map(source, context);

            target.ContentDto = new ContentPropertyCollectionDto();
            target.ContentDto.Properties = context.MapEnumerable<IProperty, ContentPropertyDto>(source.Properties);
            var markdown = new Markdown();
            var linkCheck = new Regex("<a[^>]+>", RegexOptions.IgnoreCase);
            var evaluator = new MatchEvaluator(AddRelNoReferrer);
            foreach (ContentVariantDisplay variant in target.Variants)
            {
                foreach (Tab<ContentPropertyDisplay> tab in variant.Tabs)
                {
                    foreach (ContentPropertyDisplay property in tab.Properties)
                    {
                        if (!string.IsNullOrEmpty(property.Description))
                        {
                            var description = markdown.Transform(property.Description);
                            property.Description = linkCheck.Replace(description, evaluator);
                        }
                    }
                }
            }
        }

        private string AddRelNoReferrer(Match m)
        {
            string result = m.Value;
            if (result.IndexOf("rel=") == -1)
            {
                result = result.Replace(">", " rel=\"noreferrer\">");
            }
            if (result.IndexOf("class=") == -1)
            {
                result = result.Replace(">", " class=\"underline\">");
            }
            if (result.IndexOf("target=") == -1)
            {
                result = result.Replace(">", " target=\"_blank\">");
            }
            return result;
        }

        // Umbraco.Code.MapAll -Segment -Language -DisplayName
        private void Map(IContent source, ContentVariantDisplay target, MapperContext context)
        {
            target.CreateDate = source.CreateDate;
            target.ExpireDate = GetScheduledDate(source, ContentScheduleAction.Expire, context);
            target.Name = source.Name;
            target.PublishDate = source.PublishDate;
            target.ReleaseDate = GetScheduledDate(source, ContentScheduleAction.Release, context);
            target.State = _stateMapper.Map(source, context);
            target.Tabs = _tabsAndPropertiesMapper.Map(source, context);
            target.UpdateDate = source.UpdateDate;
        }

        // Umbraco.Code.MapAll -Alias
        private void Map(IContent source, ContentItemBasic<ContentPropertyBasic> target, MapperContext context)
        {
            target.ContentTypeId = source.ContentType.Id;
            target.ContentTypeAlias = source.ContentType.Alias;
            target.CreateDate = source.CreateDate;
            target.Edited = source.Edited;
            target.Icon = source.ContentType.Icon;
            target.Id = source.Id;
            target.Key = source.Key;
            target.Name = GetName(source, context);
            target.Owner = _commonMapper.GetOwner(source, context);
            target.ParentId = source.ParentId;
            target.Path = source.Path;
            target.Properties = context.MapEnumerable<IProperty, ContentPropertyBasic>(source.Properties);
            target.SortOrder = source.SortOrder;
            target.State = _basicStateMapper.Map(source, context);
            target.Trashed = source.Trashed;
            target.Udi = Udi.Create(source.Blueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document, source.Key);
            target.UpdateDate = GetUpdateDate(source, context);
            target.Updater = _commonMapper.GetCreator(source, context);
            target.VariesByCulture = source.ContentType.VariesByCulture();
        }

        private IEnumerable<string> GetActions(IContent source, IContent parent, MapperContext context)
        {
            var backOfficeSecurity = _backOfficeSecurityAccessor.BackOfficeSecurity;

            //cannot check permissions without a context
            if (backOfficeSecurity is null)
                return Enumerable.Empty<string>();

            string path;
            if (source.HasIdentity)
                path = source.Path;
            else
            {
                path = parent == null ? "-1" : parent.Path;
            }

            // A bit of a mess, but we need to ensure that all the required values are here AND that they're the right type.
            if (context.Items.TryGetValue("CurrentUser", out var userObject) &&
                context.Items.TryGetValue("Permissions", out var permissionsObject) &&
                userObject is IUser currentUser &&
                permissionsObject is Dictionary<string, EntityPermissionSet> permissionsDict)
            {
                // If we already have permissions for a given path,
                // and the current user is the same as was used to generate the permissions, return the stored permissions.
                if (backOfficeSecurity.CurrentUser.Id == currentUser.Id &&
                    permissionsDict.TryGetValue(path, out var permissions))
                {
                    return permissions.GetAllPermissions();
                }
            }

            // TODO: This is certainly not ideal usage here - perhaps the best way to deal with this in the future is
            // with the IUmbracoContextAccessor. In the meantime, if used outside of a web app this will throw a null
            // reference exception :(

            return _userService.GetPermissionsForPath(backOfficeSecurity.CurrentUser, path).GetAllPermissions();
        }

        private UrlInfo[] GetUrls(IContent source)
        {
            if (source.ContentType.IsElement)
            {
                return Array.Empty<UrlInfo>();
            }
            if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
            {
                return new[] { UrlInfo.Message("Cannot generate URLs without a current Umbraco Context") };
            }

            // NOTE: unfortunately we're not async, we'll use .Result and hope this won't cause a deadlock anywhere for now
            var urls = source.GetContentUrlsAsync(_publishedRouter, umbracoContext, _localizationService, _localizedTextService, _contentService, _variationContextAccessor, _loggerFactory.CreateLogger<IContent>(), _uriUtility, _publishedUrlProvider)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult()
                .ToArray();

            return urls;
        }

        private DateTime GetUpdateDate(IContent source, MapperContext context)
        {
            // invariant = global date
            if (!source.ContentType.VariesByCulture())
                return source.UpdateDate;

            // variant = depends on culture
            var culture = context.GetCulture();

            // if there's no culture here, the issue is somewhere else (UI, whatever) - throw!
            if (culture == null)
                throw new InvalidOperationException("Missing culture in mapping options.");

            // if we don't have a date for a culture, it means the culture is not available, and
            // hey we should probably not be mapping it, but it's too late, return a fallback date
            var date = source.GetUpdateDate(culture);
            return date ?? source.UpdateDate;
        }

        private string GetName(IContent source, MapperContext context)
        {
            // invariant = only 1 name
            if (!source.ContentType.VariesByCulture())
                return source.Name;

            // variant = depends on culture
            var culture = context.GetCulture();

            // if there's no culture here, the issue is somewhere else (UI, whatever) - throw!
            if (culture == null)
                throw new InvalidOperationException("Missing culture in mapping options.");

            // if we don't have a name for a culture, it means the culture is not available, and
            // hey we should probably not be mapping it, but it's too late, return a fallback name
            return source.CultureInfos.TryGetValue(culture, out var name) && !name.Name.IsNullOrWhiteSpace() ? name.Name : $"({source.Name})";
        }

        /// <summary>
        /// Checks if the content item is a descendant of a list view
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parent"></param>
        /// <param name="context"></param>
        /// <returns>
        /// Returns true if the content item is a descendant of a list view and where the content is
        /// not a current user's start node.
        /// </returns>
        /// <remarks>
        /// We must check if it's the current user's start node because in that case we will actually be
        /// rendering the tree node underneath the list view to visually show context. In this case we return
        /// false because the item is technically not being rendered as part of a list view but instead as a
        /// real tree node. If we didn't perform this check then tree syncing wouldn't work correctly.
        /// </remarks>
        private bool DetermineIsChildOfListView(IContent source, IContent parent, MapperContext context)
        {
            var userStartNodes = Array.Empty<int>();

            // In cases where a user's start node is below a list view, we will actually render
            // out the tree to that start node and in that case for that start node, we want to return
            // false here.
            if (context.HasItems && context.Items.TryGetValue("CurrentUser", out var usr) && usr is IUser currentUser)
            {
                userStartNodes = currentUser.CalculateContentStartNodeIds(_entityService, _appCaches);
                if (!userStartNodes.Contains(Constants.System.Root))
                {
                    // return false if this is the user's actual start node, the node will be rendered in the tree
                    // regardless of if it's a list view or not
                    if (userStartNodes.Contains(source.Id))
                        return false;
                }
            }

            if (parent == null)
                return false;

            var pathParts = parent.Path.Split(Constants.CharArrays.Comma).Select(x => int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : 0).ToList();

            // reduce the path parts so we exclude top level content items that
            // are higher up than a user's start nodes
            foreach (var n in userStartNodes)
            {
                var index = pathParts.IndexOf(n);
                if (index != -1)
                {
                    // now trim all top level start nodes to the found index
                    for (var i = 0; i < index; i++)
                    {
                        pathParts.RemoveAt(0);
                    }
                }
            }

            return parent.ContentType.IsContainer || _contentTypeService.HasContainerInPath(pathParts.ToArray());
        }


        private DateTime? GetScheduledDate(IContent source, ContentScheduleAction action, MapperContext context)
        {
            var culture = context.GetCulture() ?? string.Empty;
            var schedule = source.ContentSchedule.GetSchedule(culture, action);
            return schedule.FirstOrDefault()?.Date; // take the first, it's ordered by date
        }

        private IDictionary<string, string> GetAllowedTemplates(IContent source)
        {
            // Element types can't have templates, so no need to query to get the content type
            if (source.ContentType.IsElement)
            {
                return new Dictionary<string, string>();
            }

            var contentType = _contentTypeService.Get(source.ContentTypeId);

            return contentType.AllowedTemplates
                .Where(t => t.Alias.IsNullOrWhiteSpace() == false && t.Name.IsNullOrWhiteSpace() == false)
                .ToDictionary(t => t.Alias, t => _localizedTextService.UmbracoDictionaryTranslate(_cultureDictionary, t.Name));
        }

        private string GetDefaultTemplate(IContent source)
        {
            if (source == null)
                return null;

            // If no template id was set...
            if (!source.TemplateId.HasValue)
            {
                // ... and no default template is set, return null...
                // ... otherwise return the content type default template alias.
                return string.IsNullOrWhiteSpace(source.ContentType.DefaultTemplate?.Alias)
                    ? null
                    : source.ContentType.DefaultTemplate?.Alias;
            }

            var template = _fileService.GetTemplate(source.TemplateId.Value);
            return template.Alias;
        }
    }
}
