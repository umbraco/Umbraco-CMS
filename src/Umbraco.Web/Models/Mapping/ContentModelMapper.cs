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
                            ? new[] {"Cannot generate urls without a current Umbraco Context"}
                            : content.GetContentUrls(UmbracoContext.Current)))
                .ForMember(display => display.Properties, expression => expression.Ignore())
                .ForMember(display => display.AllowPreview, expression => expression.Ignore())
                .ForMember(display => display.TreeNodeUrl, expression => expression.Ignore())
                .ForMember(display => display.Notifications, expression => expression.Ignore())
                .ForMember(display => display.Errors, expression => expression.Ignore())
                .ForMember(display => display.Alias, expression => expression.Ignore())
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
            var date = ((Content) content).PublishedDate;
            return date == default (DateTime) ? (DateTime?) null : date;
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

            //fill in the template config to be passed to the template drop down.
            var templateItemConfig = new Dictionary<string, string>();
            foreach (var t in content.ContentType.AllowedTemplates
                .Where(t => t.Alias.IsNullOrWhiteSpace() == false && t.Name.IsNullOrWhiteSpace() == false))
            {
                templateItemConfig.Add(t.Alias, t.Name);
            }

            if (content.ContentType.IsContainer)
            {
                TabsAndPropertiesResolver.AddListView(display, "content", dataTypeService, localizedText);
            }

            var properties = new List<ContentPropertyDisplay>
            {
                new ContentPropertyDisplay
                {
                    Alias = string.Format("{0}doctype", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                    Label = localizedText.Localize("content/documentType"),
                    Value = localizedText.UmbracoDictionaryTranslate(display.ContentTypeName),
                    View = PropertyEditorResolver.Current.GetByAlias(Constants.PropertyEditors.NoEditAlias).ValueEditor.View
                },
                new ContentPropertyDisplay
                {
                    Alias = string.Format("{0}releasedate", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                    Label = localizedText.Localize("content/releaseDate"),
                    Value = display.ReleaseDate.HasValue ? display.ReleaseDate.Value.ToIsoString() : null,
                    //Not editible for people without publish permission (U4-287)
                    View = display.AllowedActions.Contains(ActionPublish.Instance.Letter.ToString(CultureInfo.InvariantCulture)) ? "datepicker" : PropertyEditorResolver.Current.GetByAlias(Constants.PropertyEditors.NoEditAlias).ValueEditor.View,
                    Config = new Dictionary<string, object>
                    {
                        {"offsetTime", "1"}
                    }
                    //TODO: Fix up hard coded datepicker
                },
                new ContentPropertyDisplay
                {
                    Alias = string.Format("{0}expiredate", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                    Label = localizedText.Localize("content/unpublishDate"),
                    Value = display.ExpireDate.HasValue ? display.ExpireDate.Value.ToIsoString() : null,
                    //Not editible for people without publish permission (U4-287)
                    View = display.AllowedActions.Contains(ActionPublish.Instance.Letter.ToString(CultureInfo.InvariantCulture)) ? "datepicker" : PropertyEditorResolver.Current.GetByAlias(Constants.PropertyEditors.NoEditAlias).ValueEditor.View,
                    Config = new Dictionary<string, object>
                    {
                        {"offsetTime", "1"}
                    }
                    //TODO: Fix up hard coded datepicker
                },
                new ContentPropertyDisplay
                {
                    Alias = string.Format("{0}template", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                    Label = localizedText.Localize("template/template"),
                    Value = string.IsNullOrEmpty(display.TemplateAlias)
                        ? (content.ContentType.DefaultTemplate == null ? "" : content.ContentType.DefaultTemplate.Alias)
                        : display.TemplateAlias,
                    View = "dropdown", //TODO: Hard coding until we make a real dropdown property editor to lookup
                    Config = new Dictionary<string, object>
                    {
                        {"items", templateItemConfig}
                    }
                }
            };


            TabsAndPropertiesResolver.MapGenericProperties(content, display, localizedText, properties.ToArray(),
                genericProperties =>
                {
                    //TODO: This would be much nicer with the IUmbracoContextAccessor so we don't use singletons
                    //If this is a web request and there's a user signed in and the 
                    // user has access to the settings section, we will 
                    if (HttpContext.Current != null && UmbracoContext.Current != null && UmbracoContext.Current.Security.CurrentUser != null
                        && UmbracoContext.Current.Security.CurrentUser.AllowedSections.Any(x => x.Equals(Constants.Applications.Settings)))
                    {
                        var currentDocumentType = contentTypeService.GetContentType(display.ContentTypeAlias);
                        var currentDocumentTypeName = currentDocumentType == null ? string.Empty : localizedText.UmbracoDictionaryTranslate(currentDocumentType.Name);

                        var currentDocumentTypeId = currentDocumentType == null ? string.Empty : currentDocumentType.Id.ToString(CultureInfo.InvariantCulture);
                        //TODO: Hard coding this is not good
                        var docTypeLink = string.Format("#/settings/documenttypes/edit/{0}", currentDocumentTypeId);

                        //Replace the doc type property
                        var docTypeProperty = genericProperties.First(x => x.Alias == string.Format("{0}doctype", Constants.PropertyEditors.InternalGenericPropertiesPrefix));
                        docTypeProperty.Value = new List<object>
                        {
                            new
                            {
                                linkText = currentDocumentTypeName,
                                url = docTypeLink,
                                target = "_self",
                                icon = "icon-item-arrangement"
                            }
                        };
                        //TODO: Hard coding this because the templatepicker doesn't necessarily need to be a resolvable (real) property editor
                        docTypeProperty.View = "urllist";
                    }

                    // inject 'Link to document' as the first generic property
                    genericProperties.Insert(0, new ContentPropertyDisplay
                    {
                        Alias = string.Format("{0}urls", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                        Label = localizedText.Localize("content/urls"),
                        Value = string.Join(",", display.Urls),
                        View = "urllist" //TODO: Hard coding this because the templatepicker doesn't necessarily need to be a resolvable (real) property editor
                    });
                });
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