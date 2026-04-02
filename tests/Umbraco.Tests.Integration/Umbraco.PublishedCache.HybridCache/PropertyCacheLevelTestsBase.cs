using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public abstract class PropertyCacheLevelTestsBase : UmbracoIntegrationTest
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<MediaTreeChangeNotification, MediaTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();

        builder.PropertyValueConverters().Append<PropertyValueLevelDetectionTestsConverter>();
    }

    [HideFromTypeFinder]
    public class PropertyValueLevelDetectionTestsConverter : PropertyValueConverterBase
    {
        private static PropertyCacheLevel _cacheLevel;

        public static void Reset()
            => SourceConverts = InterConverts = 0;

        public static void SetCacheLevel(PropertyCacheLevel cacheLevel)
            => _cacheLevel = cacheLevel;

        public static int SourceConverts { get; private set; }

        public static int InterConverts { get; private set; }

        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias is Constants.PropertyEditors.Aliases.TextBox or Constants.PropertyEditors.Aliases.TextArea;

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(string);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => _cacheLevel;

        public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            SourceConverts++;
            return base.ConvertSourceToIntermediate(owner, propertyType, source, preview);
        }

        public override object? ConvertIntermediateToObject(
            IPublishedElement owner,
            IPublishedPropertyType propertyType,
            PropertyCacheLevel referenceCacheLevel,
            object inter,
            bool preview)
        {
            InterConverts++;
            return base.ConvertIntermediateToObject(owner, propertyType, referenceCacheLevel, inter, preview);
        }
    }
}
