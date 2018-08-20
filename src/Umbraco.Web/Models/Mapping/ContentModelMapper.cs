using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Trees;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core.PropertyEditors;
using Content = Umbraco.Core.Models.Content;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Declares how model mappings for content
    /// </summary>
    internal class ContentModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {

            //FROM IContent TO ContentItemDisplay
            config.CreateMap<IContent, ContentItemDisplay>()
                .ForMember(display => display.Udi, expression => expression.MapFrom(content => Udi.Create(content.IsBlueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document, content.Key)))
                .ForMember(display => display.Owner, expression => expression.ResolveUsing(new OwnerResolver<IContent>()))
                .ForMember(display => display.Updater, expression => expression.ResolveUsing(new CreatorResolver()))
                .ForMember(display => display.Icon, expression => expression.MapFrom(content => content.ContentType.Icon))
                .ForMember(display => display.ContentTypeAlias, expression => expression.MapFrom(content => content.ContentType.Alias))
                .ForMember(display => display.ContentTypeName, expression => expression.MapFrom(content => content.ContentType.Name))
                .ForMember(display => display.IsContainer, expression => expression.MapFrom(content => content.ContentType.IsContainer))
                .ForMember(display => display.IsChildOfListView, expression => expression.ResolveUsing(new ChildOfListViewResolver(applicationContext.Services.ContentService, applicationContext.Services.ContentTypeService)))
                .ForMember(display => display.Trashed, expression => expression.MapFrom(content => content.Trashed))
                .ForMember(display => display.PublishDate, expression => expression.MapFrom(content => GetPublishedDate(content)))
                .ForMember(display => display.TemplateAlias, expression => expression.ResolveUsing<DefaultTemplateResolver>())
                .ForMember(display => display.HasPublishedVersion, expression => expression.MapFrom(content => content.HasPublishedVersion))
                .ForMember(display => display.Urls, expression => expression.ResolveUsing<ContentUrlResolver>())
                .ForMember(display => display.Properties, expression => expression.Ignore())
                .ForMember(display => display.AllowPreview, expression => expression.Ignore())
                .ForMember(display => display.TreeNodeUrl, opt => opt.ResolveUsing(new ContentTreeNodeUrlResolver<IContent, ContentTreeController>()))
                .ForMember(display => display.Notifications, expression => expression.Ignore())
                .ForMember(display => display.Errors, expression => expression.Ignore())
                .ForMember(display => display.Alias, expression => expression.Ignore())
                .ForMember(display => display.DocumentType, expression => expression.ResolveUsing<ContentTypeBasicResolver>())
                .ForMember(display => display.AllowedTemplates, expression =>
                    expression.MapFrom(content => content.ContentType.AllowedTemplates
                        .Where(t => t.Alias.IsNullOrWhiteSpace() == false && t.Name.IsNullOrWhiteSpace() == false)
                        .ToDictionary(t => t.Alias, t => t.Name)))
                .ForMember(display => display.Tabs, expression => expression.ResolveUsing(new TabsAndPropertiesResolver<IContent>(applicationContext.Services.TextService)))
                .ForMember(display => display.AllowedActions, expression => expression.ResolveUsing(
                    new ActionButtonsResolver(new Lazy<IUserService>(() => applicationContext.Services.UserService), new Lazy<IContentService>(() => applicationContext.Services.ContentService))))
                .AfterMap((content, display) =>
                {
                    if (content.ContentType.IsContainer)
                    {
                        TabsAndPropertiesResolver<IContent>.AddListView(display, "content", applicationContext.Services.DataTypeService, applicationContext.Services.TextService);
                    }
                });

            //FROM IContent TO ContentItemBasic<ContentPropertyBasic, IContent>
            config.CreateMap<IContent, ContentItemBasic<ContentPropertyBasic, IContent>>()
                .ForMember(display => display.Udi, expression => expression.MapFrom(content => Udi.Create(content.IsBlueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document, content.Key)))
                .ForMember(dto => dto.Owner, expression => expression.ResolveUsing(new OwnerResolver<IContent>()))
                .ForMember(dto => dto.Updater, expression => expression.ResolveUsing(new CreatorResolver()))
                .ForMember(dto => dto.Icon, expression => expression.MapFrom(content => content.ContentType.Icon))
                .ForMember(dto => dto.Trashed, expression => expression.MapFrom(content => content.Trashed))
                .ForMember(dto => dto.HasPublishedVersion, expression => expression.MapFrom(content => content.HasPublishedVersion))
                .ForMember(dto => dto.ContentTypeAlias, expression => expression.MapFrom(content => content.ContentType.Alias))
                .ForMember(dto => dto.Alias, expression => expression.Ignore());

            //FROM IContent TO ContentItemDto<IContent>
            config.CreateMap<IContent, ContentItemDto<IContent>>()
                .ForMember(display => display.Udi, expression => expression.MapFrom(content => Udi.Create(content.IsBlueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document, content.Key)))
                .ForMember(dto => dto.Owner, expression => expression.ResolveUsing(new OwnerResolver<IContent>()))
                .ForMember(dto => dto.HasPublishedVersion, expression => expression.MapFrom(content => content.HasPublishedVersion))
                .ForMember(dto => dto.Updater, expression => expression.Ignore())
                .ForMember(dto => dto.Icon, expression => expression.Ignore())
                .ForMember(dto => dto.Alias, expression => expression.Ignore());
        }

        private static DateTime? GetPublishedDate(IContent content)
        {
            var date = ((Content)content).PublishedDate;
            return date == default(DateTime) ? (DateTime?)null : date;
        }
        
        internal class ContentUrlResolver : IValueResolver
        {
            public ResolutionResult Resolve(ResolutionResult source)
            {
                var content = (IContent)source.Value;

                var umbCtx = source.Context.GetUmbracoContext();

                var urls = umbCtx == null
                    ? new[] {"Cannot generate urls without a current Umbraco Context"}
                    : content.GetContentUrls(umbCtx);

                return source.New(urls, typeof(string[]));
            }
        }

        internal class DefaultTemplateResolver : ValueResolver<IContent, string>
        {
            protected override string ResolveCore(IContent source)
            {
                if (source == null || source.Template == null) return null;

                var alias = source.Template.Alias;

                //set default template if template isn't set
                if (string.IsNullOrEmpty(alias))
                    alias = source.ContentType.DefaultTemplate == null
                        ? string.Empty
                        : source.ContentType.DefaultTemplate.Alias;

                return alias;
            }
        }

        private class ChildOfListViewResolver : ValueResolver<IContent, bool>
        {
            private readonly IContentService _contentService;
            private readonly IContentTypeService _contentTypeService;

            public ChildOfListViewResolver(IContentService contentService, IContentTypeService contentTypeService)
            {
                _contentService = contentService;
                _contentTypeService = contentTypeService;
            }

            protected override bool ResolveCore(IContent source)
            {
                // map the IsChildOfListView (this is actually if it is a descendant of a list view!)
                var parent = _contentService.GetParent(source);
                return parent != null && (parent.ContentType.IsContainer || _contentTypeService.HasContainerInPath(parent.Path));
            }
        }

        /// <summary>
        /// Resolves a <see cref="ContentTypeBasic"/> from the <see cref="IContent"/> item and checks if the current user
        /// has access to see this data
        /// </summary>
        private class ContentTypeBasicResolver : ValueResolver<IContent, ContentTypeBasic>
        {
            protected override ContentTypeBasic ResolveCore(IContent source)
            {
                //TODO: We can resolve the UmbracoContext from the IValueResolver options!
                if (HttpContext.Current != null && UmbracoContext.Current != null && UmbracoContext.Current.Security.CurrentUser != null
                    && UmbracoContext.Current.Security.CurrentUser.AllowedSections.Any(x => x.Equals(Constants.Applications.Settings)))
                {
                    var contentTypeBasic = Mapper.Map<ContentTypeBasic>(source.ContentType);
                    return contentTypeBasic;
                }
                //no access
                return null;
            }
        }

        /// <summary>
        /// Creates the list of action buttons allowed for this user - Publish, Send to publish, save, unpublish returned as the button's 'letter'
        /// </summary>
        private class ActionButtonsResolver : ValueResolver<IContent, IEnumerable<string>>
        {
            private readonly Lazy<IUserService> _userService;
            private readonly Lazy<IContentService> _contentService;

            public ActionButtonsResolver(Lazy<IUserService> userService, Lazy<IContentService> contentService)
            {
                if (userService == null) throw new ArgumentNullException("userService");
                if (contentService == null) throw new ArgumentNullException("contentService");
                _userService = userService;
                _contentService = contentService;
            }

            protected override IEnumerable<string> ResolveCore(IContent source)
            {
                if (UmbracoContext.Current == null)
                {
                    //cannot check permissions without a context
                    return Enumerable.Empty<string>();
                }
                var svc = _userService.Value;

                string path;
                if (source.HasIdentity)
                    path = source.Path;
                else
                {
                    var parent = _contentService.Value.GetById(source.ParentId);
                    path = parent == null ? "-1" : parent.Path;
                }

                var permissions = svc.GetPermissionsForPath(
                        //TODO: This is certainly not ideal usage here - perhaps the best way to deal with this in the future is
                        // with the IUmbracoContextAccessor. In the meantime, if used outside of a web app this will throw a null
                        // refrence exception :(
                        UmbracoContext.Current.Security.CurrentUser,
                        path)
                    .GetAllPermissions();

                return permissions;
            }
        }
    }
}
