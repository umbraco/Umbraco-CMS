using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class PropertyValueLevelDetectionTests : UmbracoIntegrationTestWithContent
{
    private IPublishedContentFactory PublishedContentFactory => GetRequiredService<IPublishedContentFactory>();

    private IPublishedValueFallback PublishedValueFallback => GetRequiredService<IPublishedValueFallback>();

    private IPublishedContentTypeFactory PublishedContentTypeFactory => GetRequiredService<IPublishedContentTypeFactory>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        var requestCache = new DictionaryAppCache();
        var appCaches = new AppCaches(
                NoAppCache.Instance,
                requestCache,
                new IsolatedCaches(type => NoAppCache.Instance));
        builder.Services.AddUnique(appCaches);

        builder.PropertyValueConverters()
            .Remove<global::Umbraco.Cms.Core.PropertyEditors.ValueConverters.ContentPickerValueConverter>()
            .Append<PropertyValueLevelDetectionTestsConverter>();
    }

    [TestCase("validSourceLevel", true)]
    [TestCase("validInterLevel", true)]
    [TestCase("validObjectLevel", true)]
    [TestCase("invalidSourceLevel", false)]
    [TestCase("invalidInterLevel", false)]
    [TestCase("invalidObjectLevel", false)]
    [TestCase("nullSourceLevel", false)]
    [TestCase("nullInterLevel", false)]
    [TestCase("nullObjectLevel", false)]
    [TestCase("somethingElse", false)]
    public void Can_Detect_Property_Value_At_All_Levels_For_Document(string titleValue, bool expectHasValue)
    {
        var contentCacheNode = new ContentCacheNode
        {
            Id = Textpage.Id,
            Key = Textpage.Key,
            ContentTypeId = Textpage.ContentType.Id,
            CreateDate = Textpage.CreateDate,
            CreatorId = Textpage.CreatorId,
            SortOrder = Textpage.SortOrder,
            Data = new ContentData(
                Textpage.Name,
                "text-page",
                Textpage.VersionId,
                Textpage.UpdateDate,
                Textpage.WriterId,
                Textpage.TemplateId,
                true,
                new Dictionary<string, PropertyData[]>
                {
                    {
                        "title", new[]
                        {
                            new PropertyData
                            {
                                Value = titleValue,
                                Culture = string.Empty,
                                Segment = string.Empty,
                            },
                        }
                    },
                },
                null),
        };
        var result = PublishedContentFactory.ToIPublishedContent(contentCacheNode, false);
        Assert.IsNotNull(result);
        Assert.AreEqual(expectHasValue, result.HasValue("title"));

        // NOTE: the .Value() extensions always end up returning the source value, no matter if the property value
        //       converter returns false from .HasValue()... this is the case both at property and at content level
        var value = result.Value<string>(PublishedValueFallback, "title");
        Assert.AreEqual(titleValue, value);
    }

    [TestCase("validSourceLevel", true)]
    [TestCase("validInterLevel", true)]
    [TestCase("validObjectLevel", true)]
    [TestCase("invalidSourceLevel", false)]
    [TestCase("invalidInterLevel", false)]
    [TestCase("invalidObjectLevel", false)]
    [TestCase("nullSourceLevel", false)]
    [TestCase("nullInterLevel", false)]
    [TestCase("nullObjectLevel", false)]
    [TestCase("somethingElse", false)]
    public async Task Can_Detect_Property_Value_At_All_Levels_For_Element(string titleValue, bool expectHasValue)
    {
        var elementType = ContentTypeBuilder.CreateSimpleContentType("umbElement");
        elementType.IsElement = true;
        await ContentTypeService.UpdateAsync(elementType, Constants.Security.SuperUserKey);
        var publishedElementType = PublishedContentTypeFactory.CreateContentType(elementType);

        var element = new PublishedElement(
            publishedElementType,
            Guid.NewGuid(),
            new Dictionary<string, object> { { "title", titleValue } },
            false,
            new VariationContext());

        Assert.AreEqual(expectHasValue, element.HasValue("title"));

        // NOTE: the .Value() extensions always end up returning the source value, no matter if the property value
        //       converter returns false from .HasValue()... this is the case both at property and at content level
        var value = element.Value<string>(PublishedValueFallback, "title");
        Assert.AreEqual(titleValue, value);
    }

    [HideFromTypeFinder]
    public class PropertyValueLevelDetectionTestsConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias == Constants.PropertyEditors.Aliases.TextBox;

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(string);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.None;

        public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
            => source as string;

        public override bool? IsValue(object? value, PropertyValueLevel level)
            => level switch
            {
                PropertyValueLevel.Source when value?.ToString() is "validSourceLevel" => true,
                PropertyValueLevel.Source when value?.ToString() is "invalidSourceLevel" => false,
                PropertyValueLevel.Source when value?.ToString() is "nullSourceLevel" => null,
                PropertyValueLevel.Inter when value?.ToString() is "validInterLevel" => true,
                PropertyValueLevel.Inter when value?.ToString() is "invalidInterLevel" => false,
                PropertyValueLevel.Source when value?.ToString() is "nullInternalLevel" => null,
                PropertyValueLevel.Object when value?.ToString() is "validObjectLevel" => true,
                PropertyValueLevel.Object when value?.ToString() is "invalidObjectLevel" => false,
                PropertyValueLevel.Source when value?.ToString() is "nullObjectlLevel" => null,
                _ => null,
            };
    }
}
