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
                .ForMember(display => display.Udi, expression => expression.MapFrom(content => Udi.Create(content.IsBlueprint ? Constants.UdiEntityType.DocumentBluePrint : Constants.UdiEntityType.Document, content.Key)))
                .ForMember(display => display.Owner, expression => expression.ResolveUsing(new OwnerResolver<IContent>()))
                .ForMember(display => display.Updater, expression => expression.ResolveUsing(new CreatorResolver()))
                .ForMember(display => display.Icon, expression => expression.MapFrom(content => content.ContentType.Icon))
                .ForMember(display => display.ContentTypeAlias, expression => expression.MapFrom(content => content.ContentType.Alias))
                .ForMember(display => display.ContentTypeName, expression => expression.MapFrom(content => content.ContentType.Name))
                .ForMember(display => display.IsContainer, expression => expression.MapFrom(content => content.ContentType.IsContainer))
                .ForMember(display => display.IsChildOfListView, expression => expression.Ignore())
                .ForMember(display => display.Trashed, expression => expression.MapFrom(content => content.Trashed))
                .ForMember(display => display.PublishDate, expression => expression.MapFrom(content => GetPublishedDate(content)))
                .ForMember(display => display.TemplateAlias, expression => expression.MapFrom(content => content.Template.Alias))
                .ForMember(display => display.HasPublishedVersion, expression => expression.MapFrom(content => content.HasPublishedVersion))
                .ForMember(display => display.Urls,
                    expression => expression.MapFrom(content =>
                        UmbracoContext.Current == null
                            ? new[] { "Cannot generate urls without a current Umbraco Context" }
                            : content.GetContentUrls(UmbracoContext.Current)))
                .ForMember(display => display.Properties, expression => expression.Ignore())
                .ForMember(display => display.AllowPreview, expression => expression.Ignore())
                .ForMember(display => display.TreeNodeUrl, expression => expression.Ignore())
                .ForMember(display => display.Notifications, expression => expression.Ignore())
                .ForMember(display => display.Errors, expression => expression.Ignore())
                .ForMember(display => display.Alias, expression => expression.Ignore())
                .ForMember(display => display.DocumentType, expression => expression.ResolveUsing<ContentTypeBasicResolver>())
                .ForMember(display => display.AllowedTemplates, expression =>
                    expression.MapFrom(content => content.ContentType.AllowedTemplates
                        .Where(t => t.Alias.IsNullOrWhiteSpace() == false && t.Name.IsNullOrWhiteSpace() == false)
                        .ToDictionary(t => t.Alias, t => t.Name)))
                .ForMember(display => display.Tabs, expression => expression.ResolveUsing(new TabsAndPropertiesResolver(applicationContext.Services.TextService)))
                .ForMember(display => display.AllowedActions, expression => expression.ResolveUsing(
                    new ActionButtonsResolver(new Lazy<IUserService>(() => applicationContext.Services.UserService))))
                .AfterMap((content, display) => AfterMap(content, display, applicationContext.Services.DataTypeService, applicationContext.Services.TextService,
                    applicationContext.Services.ContentTypeService));

            //FROM IContent TO ContentItemBasic<ContentPropertyBasic, IContent>
            config.CreateMap<IContent, ContentItemBasic<ContentPropertyBasic, IContent>>()
                .ForMember(display => display.Udi, expression => expression.MapFrom(content => Udi.Create(content.IsBlueprint ? Constants.UdiEntityType.DocumentBluePrint : Constants.UdiEntityType.Document, content.Key)))
                .ForMember(dto => dto.Owner, expression => expression.ResolveUsing(new OwnerResolver<IContent>()))
                .ForMember(dto => dto.Updater, expression => expression.ResolveUsing(new CreatorResolver()))
                .ForMember(dto => dto.Icon, expression => expression.MapFrom(content => content.ContentType.Icon))
                .ForMember(dto => dto.Trashed, expression => expression.MapFrom(content => content.Trashed))
                .ForMember(dto => dto.HasPublishedVersion, expression => expression.MapFrom(content => content.HasPublishedVersion))
                .ForMember(dto => dto.ContentTypeAlias, expression => expression.MapFrom(content => content.ContentType.Alias))
                .ForMember(dto => dto.Alias, expression => expression.Ignore());

            //FROM IContent TO ContentItemDto<IContent>
            config.CreateMap<IContent, ContentItemDto<IContent>>()
                .ForMember(display => display.Udi, expression => expression.MapFrom(content => Udi.Create(content.IsBlueprint ? Constants.UdiEntityType.DocumentBluePrint : Constants.UdiEntityType.Document, content.Key)))
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

        /// <summary>
        /// Maps the generic tab with custom properties for content
        /// </summary>
        /// <param name="content"></param>
        /// <param name="display"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="localizedText"></param>
        /// <param name="contentTypeService"></param>
        private static void AfterMap(IContent content, ContentItemDisplay display, IDataTypeService dataTypeService,
            ILocalizedTextService localizedText, IContentTypeService contentTypeService)
        {
            // map the IsChildOfListView (this is actually if it is a descendant of a list view!)
            var parent = content.Parent();
            display.IsChildOfListView = parent != null && (parent.ContentType.IsContainer || contentTypeService.HasContainerInPath(parent.Path));

            //map the tree node url
            if (HttpContext.Current != null)
            {
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                var url = urlHelper.GetUmbracoApiService<ContentTreeController>(controller => controller.GetTreeNode(display.Id.ToString(), null));
                display.TreeNodeUrl = url;
            }

            //set default template if template isn't set
            if (string.IsNullOrEmpty(display.TemplateAlias))
                display.TemplateAlias = content.ContentType.DefaultTemplate == null
                    ? string.Empty
                    : content.ContentType.DefaultTemplate.Alias;

            if (content.ContentType.IsContainer)
            {
                TabsAndPropertiesResolver.AddListView(display, "content", dataTypeService, localizedText);
            }

            TabsAndPropertiesResolver.MapGenericProperties(content, display, localizedText);
        }

        /// <summary>
        /// Resolves a <see cref="ContentTypeBasic"/> from the <see cref="IContent"/> item and checks if the current user
        /// has access to see this data
        /// </summary>
        private class ContentTypeBasicResolver : ValueResolver<IContent, ContentTypeBasic>
        {
            protected override ContentTypeBasic ResolveCore(IContent source)
            {
                //TODO: This would be much nicer with the IUmbracoContextAccessor so we don't use singletons
                //If this is a web request and there's a user signed in and the 
                // user has access to the settings section, we will 
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

            public ActionButtonsResolver(Lazy<IUserService> userService)
            {
                _userService = userService;
            }

            protected override IEnumerable<string> ResolveCore(IContent source)
            {
                if (UmbracoContext.Current == null)
                {
                    //cannot check permissions without a context
                    return Enumerable.Empty<string>();
                }
                var svc = _userService.Value;

                var permissions = svc.GetPermissions(
                        //TODO: This is certainly not ideal usage here - perhaps the best way to deal with this in the future is
                        // with the IUmbracoContextAccessor. In the meantime, if used outside of a web app this will throw a null
                        // refrence exception :(
                        UmbracoContext.Current.Security.CurrentUser,
                        // Here we need to do a special check since this could be new content, in which case we need to get the permissions
                        // from the parent, not the existing one otherwise permissions would be coming from the root since Id is 0.
                        source.HasIdentity ? source.Id : source.ParentId)
                    .GetAllPermissions();

                return permissions;
            }
        }
    }
}