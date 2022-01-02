using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.UI.App_Plugins.Test
{
    public class CustomPackageComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.AddNotificationHandler<SendingAllowedChildrenNotification, NotificationHandler>();
        }
    }

    public class NotificationHandler : INotificationHandler<SendingAllowedChildrenNotification>
    {
        public void Handle(SendingAllowedChildrenNotification notification)
        {
            var currentSettings = notification.UmbracoContext.Content.GetSingleByXPath("//newsContainer");
            if (currentSettings is null)
                return;

            notification.Children.RemoveAll((item) => item.Alias == "newsContainer");
        }
    }

    public class CustomPackageScript : JavaScriptFile
    {
        public CustomPackageScript() : base("/App_Plugins/Test/testJavascript.js") { }
    }

    public class TestUrlProvider : IUrlProvider
    {
        private readonly IShortStringHelper _shortStringHelper;

        public TestUrlProvider(IShortStringHelper shortStringHelper)
        {
            _shortStringHelper = shortStringHelper;
        }

        public UrlInfo GetUrl(IPublishedContent content, UrlMode mode, string culture, Uri current)
        {
            if (!content.TemplateId.HasValue || content.TemplateId == 0)
            {
                if (culture == "nl")
                {
                    return UrlInfo.Url($"/hallo/{content.UrlSegment}", culture);
                }
                return UrlInfo.Url($"/hello/{content.UrlSegment}", culture);
            }

            return null;
        }

        public IEnumerable<UrlInfo> GetOtherUrls(int id, Uri current)
        {
            return Enumerable.Empty<UrlInfo>();
        }
    }

    public class CacheLevelTestsValueConverter : PropertyValueConverterBase
    {
        private readonly ILogger<CacheLevelTestsValueConverter> _logger;

        public CacheLevelTestsValueConverter(ILogger<CacheLevelTestsValueConverter> logger)
        {
            _logger = logger;
        }


        private static readonly string[] PropertyTypeAliases =
        {
            Constants.PropertyEditors.Aliases.TextBox,
            Constants.PropertyEditors.Aliases.TextArea
        };

        public override bool IsConverter(IPublishedPropertyType propertyType)
            => PropertyTypeAliases.Contains(propertyType.EditorAlias);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(string);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Elements;

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            _logger.LogWarning("Converting!");
            if (source == null)
                return null;
            var sourceString = source.ToString();
            return sourceString;
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            // source should come from ConvertSource and be a string (or null) already
            return inter ?? string.Empty;
        }

        public override object ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            // source should come from ConvertSource and be a string (or null) already
            return inter;
        }
    }
}
