using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Trees;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic.Actions;

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
                .ForMember(
                    dto => dto.Owner,
                    expression => expression.ResolveUsing<OwnerResolver<IContent>>())
                .ForMember(
                    dto => dto.Updater,
                    expression => expression.ResolveUsing<CreatorResolver>())
                .ForMember(
                    dto => dto.Icon,
                    expression => expression.MapFrom(content => content.ContentType.Icon))
                .ForMember(
                    dto => dto.ContentTypeAlias,
                    expression => expression.MapFrom(content => content.ContentType.Alias))
                .ForMember(
                    dto => dto.ContentTypeName,
                    expression => expression.MapFrom(content => content.ContentType.Name))
                .ForMember(
                    dto => dto.IsContainer,
                    expression => expression.MapFrom(content => content.ContentType.IsContainer))
                .ForMember(display => display.IsChildOfListView, expression => expression.Ignore())                
                .ForMember(
                    dto => dto.Trashed,
                    expression => expression.MapFrom(content => content.Trashed))
                .ForMember(
                    dto => dto.PublishDate,
                    expression => expression.MapFrom(content => GetPublishedDate(content, applicationContext)))
                .ForMember(
                    dto => dto.TemplateAlias, expression => expression.MapFrom(content => content.Template.Alias))
                .ForMember(
                    dto => dto.Urls,
                    expression => expression.MapFrom(content =>
                        UmbracoContext.Current == null
                            ? new[] {"Cannot generate urls without a current Umbraco Context"}
                            : content.GetContentUrls(UmbracoContext.Current)))
                .ForMember(display => display.Properties, expression => expression.Ignore())
                .ForMember(display => display.TreeNodeUrl, expression => expression.Ignore())
                .ForMember(display => display.Notifications, expression => expression.Ignore())
                .ForMember(display => display.Errors, expression => expression.Ignore())
                .ForMember(display => display.Alias, expression => expression.Ignore())
                .ForMember(display => display.Tabs, expression => expression.ResolveUsing<TabsAndPropertiesResolver>())
                .ForMember(display => display.AllowedActions, expression => expression.ResolveUsing(
                    new ActionButtonsResolver(new Lazy<IUserService>(() => applicationContext.Services.UserService))))
                .AfterMap((media, display) => AfterMap(media, display, applicationContext.Services.DataTypeService, applicationContext.Services.TextService,
                    applicationContext.Services.ContentTypeService));

            //FROM IContent TO ContentItemBasic<ContentPropertyBasic, IContent>
            config.CreateMap<IContent, ContentItemBasic<ContentPropertyBasic, IContent>>()
                .ForMember(
                    dto => dto.Owner,
                    expression => expression.ResolveUsing<OwnerResolver<IContent>>())
                .ForMember(
                    dto => dto.Updater,
                    expression => expression.ResolveUsing<CreatorResolver>())
                .ForMember(
                    dto => dto.Icon,
                    expression => expression.MapFrom(content => content.ContentType.Icon))
                .ForMember(
                    dto => dto.Trashed,
                    expression => expression.MapFrom(content => content.Trashed))
                .ForMember(
                    dto => dto.ContentTypeAlias,
                    expression => expression.MapFrom(content => content.ContentType.Alias))
                .ForMember(display => display.Alias, expression => expression.Ignore());

            //FROM IContent TO ContentItemDto<IContent>
            config.CreateMap<IContent, ContentItemDto<IContent>>()
                .ForMember(
                    dto => dto.Owner,
                    expression => expression.ResolveUsing<OwnerResolver<IContent>>())
                .ForMember(display => display.Updater, expression => expression.Ignore())
                .ForMember(display => display.Icon, expression => expression.Ignore())
                .ForMember(display => display.Alias, expression => expression.Ignore());


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
            //map the IsChildOfListView (this is actually if it is a descendant of a list view!)
            //TODO: Fix this shorthand .Ancestors() lookup, at least have an overload to use the current
            if (content.HasIdentity)
            {
                var ancesctorListView = content.Ancestors().FirstOrDefault(x => x.ContentType.IsContainer);
                display.IsChildOfListView = ancesctorListView != null;
            }
            else
            {
                //it's new so it doesn't have a path, so we need to look this up by it's parent + ancestors
                var parent = content.Parent();
                if (parent == null)
                {
                    display.IsChildOfListView = false;
                }
                else if (parent.ContentType.IsContainer)
                {
                    display.IsChildOfListView = true;
                }
                else
                {
                    var ancesctorListView = parent.Ancestors().FirstOrDefault(x => x.ContentType.IsContainer);
                    display.IsChildOfListView = ancesctorListView != null;
                }
            }
            

            //map the tree node url
            if (HttpContext.Current != null)
            {
                var urlHelper = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));
                var url = urlHelper.GetUmbracoApiService<ContentTreeController>(controller => controller.GetTreeNode(display.Id.ToString(), null));
                display.TreeNodeUrl = url;
            }
            
            //fill in the template config to be passed to the template drop down.
            var templateItemConfig = new Dictionary<string, string> { { "", "Choose..." } };
            foreach (var t in content.ContentType.AllowedTemplates
                .Where(t => t.Alias.IsNullOrWhiteSpace() == false && t.Name.IsNullOrWhiteSpace() == false))
            {
                templateItemConfig.Add(t.Alias, t.Name);
            }

            if (content.ContentType.IsContainer)
            {
                TabsAndPropertiesResolver.AddListView(display, "content", dataTypeService);
            }

            var properties = new List<ContentPropertyDisplay>
            {
                new ContentPropertyDisplay
                {
                    Alias = string.Format("{0}releasedate", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                    Label = localizedText.Localize("content/releaseDate"),
                    Value = display.ReleaseDate.HasValue ? display.ReleaseDate.Value.ToIsoString() : null,
                    View = "datepicker" //TODO: Hard coding this because the templatepicker doesn't necessarily need to be a resolvable (real) property editor
                },
                new ContentPropertyDisplay
                {
                    Alias = string.Format("{0}expiredate", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                    Label = localizedText.Localize("content/unpublishDate"),
                    Value = display.ExpireDate.HasValue ? display.ExpireDate.Value.ToIsoString() : null,
                    View = "datepicker" //TODO: Hard coding this because the templatepicker doesn't necessarily need to be a resolvable (real) property editor
                },
                new ContentPropertyDisplay
                {
                    Alias = string.Format("{0}template", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                    Label = "Template", //TODO: localize this?
                    Value = display.TemplateAlias,
                    View = "dropdown", //TODO: Hard coding until we make a real dropdown property editor to lookup
                    Config = new Dictionary<string, object>
                    {
                        {"items", templateItemConfig}
                    }
                },
                new ContentPropertyDisplay
                {
                    Alias = string.Format("{0}urls", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                    Label = localizedText.Localize("content/urls"),
                    Value = string.Join(",", display.Urls),
                    View = "urllist" //TODO: Hard coding this because the templatepicker doesn't necessarily need to be a resolvable (real) property editor
                }
            };

            TabsAndPropertiesResolver.MapGenericProperties(content, display, properties.ToArray(),
                genericProperties =>
                {
                    //TODO: This would be much nicer with the IUmbracoContextAccessor so we don't use singletons
                    //If this is a web request and there's a user signed in and the 
                    // user has access to the settings section, we will 
                    if (HttpContext.Current != null && UmbracoContext.Current != null && UmbracoContext.Current.Security.CurrentUser != null
                        && UmbracoContext.Current.Security.CurrentUser.AllowedSections.Any(x => x.Equals(Constants.Applications.Settings)))
                    {
                        var currentDocumentType = contentTypeService.GetContentType(display.ContentTypeAlias);
                        var currentDocumentTypeName = currentDocumentType == null ? string.Empty : currentDocumentType.Name;

                        var currentDocumentTypeId = currentDocumentType == null ? string.Empty : currentDocumentType.Id.ToString(CultureInfo.InvariantCulture);
                        //TODO: Hard coding this is not good
                        var docTypeLink = string.Format("#/settings/framed/settings%252FeditNodeTypeNew.aspx%253Fid%253D{0}", currentDocumentTypeId);

                        //Replace the doc type property
                        var docTypeProp = genericProperties.First(x => x.Alias == string.Format("{0}doctype", Constants.PropertyEditors.InternalGenericPropertiesPrefix));
                        docTypeProp.Value = new List<object>
                        {
                            new
                            {
                                linkText = currentDocumentTypeName,
                                url = docTypeLink,
                                target = "_self", icon = "icon-item-arrangement"
                            }
                        };
                        //TODO: Hard coding this because the templatepicker doesn't necessarily need to be a resolvable (real) property editor
                        docTypeProp.View = "urllist";
                    }
                });

        }

        /// <summary>
        /// Gets the published date value for the IContent object
        /// </summary>
        /// <param name="content"></param>
        /// <param name="applicationContext"></param>
        /// <returns></returns>
        private static DateTime? GetPublishedDate(IContent content, ApplicationContext applicationContext)
        {
            if (content.Published)
            {
                return content.UpdateDate;
            }
            if (content.HasPublishedVersion)
            {
                var published = applicationContext.Services.ContentService.GetPublishedVersion(content.Id);
                return published.UpdateDate;
            }
            return null;
        }

        /// <summary>
        /// Creates the list of action buttons allowed for this user - Publish, Send to publish, save, unpublish returned as the button's 'letter'
        /// </summary>
        private class ActionButtonsResolver : ValueResolver<IContent, IEnumerable<char>>
        {
            private readonly Lazy<IUserService> _userService;

            public ActionButtonsResolver(Lazy<IUserService> userService)
            {
                _userService = userService;
            }

            protected override IEnumerable<char> ResolveCore(IContent source)
            {
                if (UmbracoContext.Current == null)
                {
                    //cannot check permissions without a context
                    return Enumerable.Empty<char>();
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
                    .FirstOrDefault();

                if (permissions == null)
                {
                    return Enumerable.Empty<char>();
                }

                var result = new List<char>();

                //can they publish ?
                if (permissions.AssignedPermissions.Contains(ActionPublish.Instance.Letter.ToString(CultureInfo.InvariantCulture)))
                {
                    result.Add(ActionPublish.Instance.Letter);
                }
                //can they send to publish ?
                if (permissions.AssignedPermissions.Contains(ActionToPublish.Instance.Letter.ToString(CultureInfo.InvariantCulture)))
                {
                    result.Add(ActionToPublish.Instance.Letter);
                }
                //can they save ?
                if (permissions.AssignedPermissions.Contains(ActionUpdate.Instance.Letter.ToString(CultureInfo.InvariantCulture)))
                {
                    result.Add(ActionUpdate.Instance.Letter);
                }
                //can they create ?
                if (permissions.AssignedPermissions.Contains(ActionNew.Instance.Letter.ToString(CultureInfo.InvariantCulture)))
                {
                    result.Add(ActionNew.Instance.Letter);
                }

                return result;
            }
        }

    }
}