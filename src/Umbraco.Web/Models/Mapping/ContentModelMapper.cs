using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Trees;
using umbraco;
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
                .ForMember(
                    dto => dto.IsChildOfListView,
                    //TODO: Fix this shorthand .Parent() lookup, at least have an overload to use the current
                    // application context so it's testable!
                    expression => expression.MapFrom(content => content.Parent().ContentType.IsContainer))
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
                .AfterMap((media, display) => AfterMap(media, display, applicationContext.Services.DataTypeService));

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
        private static void AfterMap(IContent content, ContentItemDisplay display, IDataTypeService dataTypeService)
        {
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

            TabsAndPropertiesResolver.MapGenericProperties(
                content, display,
                new ContentPropertyDisplay
                    {
                        Alias = string.Format("{0}releasedate", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                        Label = ui.Text("content", "releaseDate"),
                        Value = display.ReleaseDate.HasValue ? display.ReleaseDate.Value.ToIsoString() : null,
                        View = "datepicker" //TODO: Hard coding this because the templatepicker doesn't necessarily need to be a resolvable (real) property editor
                    },
                new ContentPropertyDisplay
                    {
                        Alias = string.Format("{0}expiredate", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                        Label = ui.Text("content", "unpublishDate"),
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
                        Label = ui.Text("content", "urls"),
                        Value = string.Join(",", display.Urls),
                        View = "urllist" //TODO: Hard coding this because the templatepicker doesn't necessarily need to be a resolvable (real) property editor
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

                var permissions = svc.GetPermissions(UmbracoContext.Current.Security.CurrentUser, source.Id)
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