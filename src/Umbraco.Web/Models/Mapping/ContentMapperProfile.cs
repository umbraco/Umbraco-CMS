using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Routing;
using Umbraco.Web.Trees;
using Umbraco.Web._Legacy.Actions;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Declares how model mappings for content
    /// </summary>
    internal class ContentMapperProfile : Profile
    {
        public ContentMapperProfile(IUserService userService, ILocalizedTextService textService, IContentService contentService, IContentTypeService contentTypeService, IDataTypeService dataTypeService)
        {
            // create, capture, cache
            var contentOwnerResolver = new OwnerResolver<IContent>(userService);
            var creatorResolver = new CreatorResolver(userService);
            var actionButtonsResolver = new ActionButtonsResolver(new Lazy<IUserService>(() => userService));
            var tabsAndPropertiesResolver = new TabsAndPropertiesResolver(textService);

            //FROM IContent TO ContentItemDisplay
            CreateMap<IContent, ContentItemDisplay>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src =>
                    Udi.Create(src.Blueprint ? Constants.UdiEntityType.DocumentBluePrint : Constants.UdiEntityType.Document, src.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => contentOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Updater, opt => opt.ResolveUsing(src => creatorResolver.Resolve(src)))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.ContentType.Icon))
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.MapFrom(src => src.ContentType.Alias))
                .ForMember(dest => dest.ContentTypeName, opt => opt.MapFrom(src => src.ContentType.Name))
                .ForMember(dest => dest.IsContainer, opt => opt.MapFrom(src => src.ContentType.IsContainer))
                .ForMember(dest => dest.IsBlueprint, opt => opt.MapFrom(src => src.Blueprint))
                .ForMember(dest => dest.IsChildOfListView, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.MapFrom(src => src.Trashed))
                .ForMember(dest => dest.PublishDate, opt => opt.MapFrom(src => src.PublishDate))
                .ForMember(dest => dest.TemplateAlias, opt => opt.MapFrom(src => src.Template.Alias))
                .ForMember(dest => dest.Urls, opt => opt.MapFrom(src =>
                        UmbracoContext.Current == null
                            ? new[] {"Cannot generate urls without a current Umbraco Context"}
                            : src.GetContentUrls(UmbracoContext.Current)))
                .ForMember(dest => dest.Properties, opt => opt.Ignore())
                .ForMember(dest => dest.AllowPreview, opt => opt.Ignore())
                .ForMember(dest => dest.TreeNodeUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Errors, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.Tabs, opt => opt.ResolveUsing(src => tabsAndPropertiesResolver.Resolve(src)))
                .ForMember(dest => dest.AllowedActions, opt => opt.ResolveUsing(src => actionButtonsResolver.Resolve(src)))
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .AfterMap((src, dest) => AfterMap(src, dest, dataTypeService, textService, contentTypeService, contentService));

            //FROM IContent TO ContentItemBasic<ContentPropertyBasic, IContent>
            CreateMap<IContent, ContentItemBasic<ContentPropertyBasic, IContent>>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src =>
                    Udi.Create(src.Blueprint ? Constants.UdiEntityType.DocumentBluePrint : Constants.UdiEntityType.Document, src.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => contentOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Updater, opt => opt.ResolveUsing(src => creatorResolver.Resolve(src)))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.ContentType.Icon))
                .ForMember(dest => dest.Trashed, opt => opt.MapFrom(src => src.Trashed))
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.MapFrom(src => src.ContentType.Alias))
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            //FROM IContent TO ContentItemDto<IContent>
            CreateMap<IContent, ContentItemDto<IContent>>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src =>
                    Udi.Create(src.Blueprint ? Constants.UdiEntityType.DocumentBluePrint : Constants.UdiEntityType.Document, src.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => contentOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Updater, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());
        }

        /// <summary>
        /// Maps the generic tab with custom properties for content
        /// </summary>
        /// <param name="content"></param>
        /// <param name="display"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="localizedText"></param>
        /// <param name="contentTypeService"></param>
        /// <param name="contentService"></param>
        private static void AfterMap(IContent content, ContentItemDisplay display, IDataTypeService dataTypeService,
            ILocalizedTextService localizedText, IContentTypeService contentTypeService, IContentService contentService)
        {
            // map the IsChildOfListView (this is actually if it is a descendant of a list view!)
                var parent = content.Parent(contentService);
            display.IsChildOfListView = parent != null && (parent.ContentType.IsContainer || contentTypeService.HasContainerInPath(parent.Path));

            //map the tree node url
            if (HttpContext.Current != null)
            {
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                var url = urlHelper.GetUmbracoApiService<ContentTreeController>(controller => controller.GetTreeNode(display.Id.ToString(), null));
                display.TreeNodeUrl = url;
            }

            //fill in the template config to be passed to the template drop down.
            var templateItemConfig = new Dictionary<string, string> {{"", localizedText.Localize("general/choose") } };
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
                    Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}doctype",
                    Label = localizedText.Localize("content/documentType"),
                    Value = localizedText.UmbracoDictionaryTranslate(display.ContentTypeName),
                    View = Current.PropertyEditors[Constants.PropertyEditors.Aliases.NoEdit].GetValueEditor().View
                },
                new ContentPropertyDisplay
                {
                    Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}releasedate",
                    Label = localizedText.Localize("content/releaseDate"),
                    Value = display.ReleaseDate?.ToIsoString(),
                    //Not editible for people without publish permission (U4-287)
                    View =  display.AllowedActions.Contains(ActionPublish.Instance.Letter.ToString(CultureInfo.InvariantCulture)) ? "datepicker"  : Current.PropertyEditors[Constants.PropertyEditors.Aliases.NoEdit].GetValueEditor().View,
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
                    View = display.AllowedActions.Contains(ActionPublish.Instance.Letter.ToString(CultureInfo.InvariantCulture)) ? "datepicker"  : Current.PropertyEditors[Constants.PropertyEditors.Aliases.NoEdit].GetValueEditor().View,
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
                    Value = display.TemplateAlias,
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
                        var currentDocumentType = contentTypeService.Get(display.ContentTypeAlias);
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
    }
}
