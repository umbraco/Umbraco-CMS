using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PublishedCache;

[TestFixture]
public class PublishedContentLanguageVariantTests : PublishedSnapshotServiceTestBase
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();

        var dataTypes = GetDefaultDataTypes();
        var cache = CreateCache(dataTypes, out var contentTypes);

        InitializedCache(cache, contentTypes, dataTypes);
    }

    protected override PropertyValueConverterCollection PropertyValueConverterCollection
    {
        get
        {
            var collection = base.PropertyValueConverterCollection;
            return new PropertyValueConverterCollection(() => collection.Append(new TestNoValueValueConverter()));
        }
    }

    private class TestNoValueValueConverter : SimpleTinyMceValueConverter
    {
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.Alias == "noprop";

        // for this test, we return false for IsValue for this property
        public override bool? IsValue(object value, PropertyValueLevel level) => false;
    }

    /// <summary>
    ///     Override to mock localization service
    /// </summary>
    /// <returns></returns>
    protected override ServiceContext CreateServiceContext(IContentType[] contentTypes, IMediaType[] mediaTypes, IDataType[] dataTypes)
    {
        var serviceContext = base.CreateServiceContext(contentTypes, mediaTypes, dataTypes);

        var localizationService = Mock.Get(serviceContext.LocalizationService);

        var languages = new List<Language>
        {
            new("en-US", "English (United States)") { Id = 1, IsDefault = true },
            new("fr", "French") { Id = 2 },
            new("es", "Spanish") { Id = 3, FallbackLanguageId = 1 },
            new("it", "Italian") { Id = 4, FallbackLanguageId = 3 },
            new("de", "German") { Id = 5 },
            new Language("da", "Danish") { Id = 6, FallbackLanguageId = 8 },
            new Language("sv", "Swedish") { Id = 7, FallbackLanguageId = 6 },
            new Language("no", "Norweigan") { Id = 8, FallbackLanguageId = 7 },
            new Language("nl", "Dutch") { Id = 9, FallbackLanguageId = 1 },
        };

        localizationService.Setup(x => x.GetAllLanguages()).Returns(languages);
        localizationService.Setup(x => x.GetLanguageById(It.IsAny<int>()))
            .Returns((int id) => languages.SingleOrDefault(y => y.Id == id));
        localizationService.Setup(x => x.GetLanguageByIsoCode(It.IsAny<string>()))
            .Returns((string c) => languages.SingleOrDefault(y => y.IsoCode == c));

        return serviceContext;
    }

    /// <summary>
    ///     Creates a content cache
    /// </summary>
    /// <param name="dataTypes"></param>
    /// <param name="contentTypes"></param>
    /// <returns></returns>
    /// <remarks>
    ///     Builds a content hierarchy of 3 nodes, each has a different set of cultural properties.
    ///     The first 2 share the same content type, the last one is a different content type.
    ///     NOTE: The content items themselves are 'Invariant' but their properties are 'Variant' by culture.
    ///     Normally in Umbraco this is prohibited but our APIs and database do actually support that behavior.
    ///     It is simpler to have these tests run this way, else we would need to use WithCultureInfos
    ///     for each item and pass in name values for all cultures we are supporting and then specify the
    ///     default VariationContextAccessor.VariationContext value to be a default culture instead of "".
    /// </remarks>
    private IEnumerable<ContentNodeKit> CreateCache(IDataType[] dataTypes, out ContentType[] contentTypes)
    {
        var result = new List<ContentNodeKit>();

        var propertyDataTypes = new Dictionary<string, IDataType>
        {
            // we only have one data type for this test which will be resolved with string empty.
            [string.Empty] = dataTypes[0],
        };

        var contentType1 = new ContentType(ShortStringHelper, -1);

        var item1Data = new ContentDataBuilder()
            .WithName("Content 1")
            .WithProperties(new PropertyDataBuilder()
                .WithPropertyData("welcomeText", "Welcome")
                .WithPropertyData("welcomeText", "Welcome", "en-US")
                .WithPropertyData("welcomeText", "Willkommen", "de")
                .WithPropertyData("welcomeText", "Welkom", "nl")
                .WithPropertyData("welcomeText2", "Welcome")
                .WithPropertyData("welcomeText2", "Welcome", "en-US")
                .WithPropertyData("noprop", "xxx")
                .Build())

            // build with a dynamically created content type
            .Build(ShortStringHelper, propertyDataTypes, contentType1, "ContentType1");

        var item1 = ContentNodeKitBuilder.CreateWithContent(
            contentType1.Id,
            1,
            "-1,1",
            draftData: item1Data,
            publishedData: item1Data);

        result.Add(item1);

        var item2Data = new ContentDataBuilder()
            .WithName("Content 2")
            .WithProperties(new PropertyDataBuilder()
                .WithPropertyData("welcomeText", "Welcome")
                .WithPropertyData("welcomeText", "Welcome", "en-US")
                .WithPropertyData("noprop", "xxx")
                .Build())

            // build while dynamically updating the same content type
            .Build(ShortStringHelper, propertyDataTypes, contentType1);

        var item2 = ContentNodeKitBuilder.CreateWithContent(
            contentType1.Id,
            2,
            "-1,1,2",
            parentContentId: 1,
            draftData: item2Data,
            publishedData: item2Data);

        result.Add(item2);

        var contentType2 = new ContentType(ShortStringHelper, -1);

        var item3Data = new ContentDataBuilder()
            .WithName("Content 3")
            .WithProperties(new PropertyDataBuilder()
                .WithPropertyData("prop3", "Oxxo")
                .WithPropertyData("prop3", "Oxxo", "en-US")
                .Build())

            // build with a dynamically created content type
            .Build(ShortStringHelper, propertyDataTypes, contentType2, "ContentType2");

        var item3 = ContentNodeKitBuilder.CreateWithContent(
            contentType2.Id,
            3,
            "-1,1,2,3",
            parentContentId: 2,
            draftData: item3Data,
            publishedData: item3Data);

        result.Add(item3);

        contentTypes = new[] { contentType1, contentType2 };

        return result;
    }

    [Test]
    public void Can_Get_Content_For_Populated_Requested_Language()
    {
        var snapshot = GetPublishedSnapshot();
        var content = snapshot.Content.GetAtRoot().First();
        var value = content.Value(Mock.Of<IPublishedValueFallback>(), "welcomeText", "en-US");
        Assert.AreEqual("Welcome", value);
    }

    [Test]
    public void Can_Get_Content_For_Populated_Requested_Non_Default_Language()
    {
        var snapshot = GetPublishedSnapshot();
        var content = snapshot.Content.GetAtRoot().First();
        var value = content.Value(Mock.Of<IPublishedValueFallback>(), "welcomeText", "de");
        Assert.AreEqual("Willkommen", value);
    }

    [Test]
    public void Do_Not_Get_Content_For_Unpopulated_Requested_Language_Without_Fallback()
    {
        var snapshot = GetPublishedSnapshot();
        var content = snapshot.Content.GetAtRoot().First();
        var value = content.Value(Mock.Of<IPublishedValueFallback>(), "welcomeText", "fr");
        Assert.IsNull(value);
    }

    [Test]
    public void Do_Not_Get_Content_For_Unpopulated_Requested_Language_With_Fallback_Unless_Requested()
    {
        var snapshot = GetPublishedSnapshot();
        var content = snapshot.Content.GetAtRoot().First();
        var value = content.Value(Mock.Of<IPublishedValueFallback>(), "welcomeText", "es");
        Assert.IsNull(value);
    }

    [Test]
    public void Can_Get_Content_For_Unpopulated_Requested_Language_With_Fallback()
    {
        var snapshot = GetPublishedSnapshot();
        var content = snapshot.Content.GetAtRoot().First();
        var value = content.Value(PublishedValueFallback, "welcomeText", "es", fallback: Fallback.ToLanguage);
        Assert.AreEqual("Welcome", value);
    }

    [Test]
    public void Can_Get_Content_For_Unpopulated_Requested_Language_With_Fallback_Over_Two_Levels()
    {
        var snapshot = GetPublishedSnapshot();
        var content = snapshot.Content.GetAtRoot().First();
        var value = content.Value(PublishedValueFallback, "welcomeText", "it", fallback: Fallback.To(Fallback.Language, Fallback.Ancestors));
        Assert.AreEqual("Welcome", value);
    }

    [Test]
    public void Do_Not_GetContent_For_Unpopulated_Requested_Language_With_Fallback_Over_That_Loops()
    {
        var snapshot = GetPublishedSnapshot();
        var content = snapshot.Content.GetAtRoot().First();
        var value = content.Value(Mock.Of<IPublishedValueFallback>(), "welcomeText", "no", fallback: Fallback.ToLanguage);
        Assert.IsNull(value);
    }

    [Test]
    public void Do_Not_Get_Content_Recursively_Unless_Requested()
    {
        var snapshot = GetPublishedSnapshot();
        var content = snapshot.Content.GetAtRoot().First().Children.First();
        var value = content.Value(Mock.Of<IPublishedValueFallback>(), "welcomeText2");
        Assert.IsNull(value);
    }

    [Test]
    public void Can_Get_Content_Recursively()
    {
        var snapshot = GetPublishedSnapshot();
        var content = snapshot.Content.GetAtRoot().First().Children.First();
        var value = content.Value(PublishedValueFallback, "welcomeText2", fallback: Fallback.ToAncestors);
        Assert.AreEqual("Welcome", value);
    }

    [Test]
    public void Do_Not_Get_Content_Recursively_Unless_Requested2()
    {
        var snapshot = GetPublishedSnapshot();
        var content = snapshot.Content.GetAtRoot().First().Children.First().Children.First();
        Assert.IsNull(content.GetProperty("welcomeText2"));
        var value = content.Value(Mock.Of<IPublishedValueFallback>(), "welcomeText2");
        Assert.IsNull(value);
    }

    [Test]
    public void Can_Get_Content_Recursively2()
    {
        var snapshot = GetPublishedSnapshot();
        var content = snapshot.Content.GetAtRoot().First().Children.First().Children.First();
        Assert.IsNull(content.GetProperty("welcomeText2"));
        var value = content.Value(PublishedValueFallback, "welcomeText2", fallback: Fallback.ToAncestors);
        Assert.AreEqual("Welcome", value);
    }

    [Test]
    public void Can_Get_Content_Recursively3()
    {
        var snapshot = GetPublishedSnapshot();
        var content = snapshot.Content.GetAtRoot().First().Children.First().Children.First();
        Assert.IsNull(content.GetProperty("noprop"));
        var value = content.Value(PublishedValueFallback, "noprop", fallback: Fallback.ToAncestors);

        // property has no value - based on the converter
        // but we still get the value (ie, the converter would do something)
        Assert.AreEqual("xxx", value.ToString());
    }

    [Test]
    public void Can_Get_Content_With_Recursive_Priority()
    {
        VariationContextAccessor.VariationContext = new VariationContext("nl");

        var snapshot = GetPublishedSnapshot();
        var content = snapshot.Content.GetAtRoot().First().Children.First();

        var value = content.Value(PublishedValueFallback, "welcomeText", "nl", fallback: Fallback.To(Fallback.Ancestors, Fallback.Language));

        // No Dutch value is directly assigned. Check has fallen back to Dutch value from parent.
        Assert.AreEqual("Welkom", value);
    }

    [Test]
    public void Can_Get_Content_With_Fallback_Language_Priority()
    {
        var snapshot = GetPublishedSnapshot();
        var content = snapshot.Content.GetAtRoot().First().Children.First();

        var value = content.Value(PublishedValueFallback, "welcomeText", "nl", fallback: Fallback.ToLanguage);

        // No Dutch value is directly assigned.  Check has fallen back to English value from language variant.
        Assert.AreEqual("Welcome", value);
    }

    [Test]
    public void Throws_For_Non_Supported_Fallback()
    {
        var snapshot = GetPublishedSnapshot();
        var content = snapshot.Content.GetAtRoot().First().Children.First();

        Assert.Throws<NotSupportedException>(() =>
            content.Value(PublishedValueFallback, "welcomeText", "nl", fallback: Fallback.To(999)));
    }

    [Test]
    public void Can_Fallback_To_Default_Value()
    {
        var snapshot = GetPublishedSnapshot();
        var content = snapshot.Content.GetAtRoot().First().Children.First();

        // no Dutch value is assigned, so getting null
        var value = content.Value(PublishedValueFallback, "welcomeText", "nl");
        Assert.IsNull(value);

        // even if we 'just' provide a default value
        value = content.Value(PublishedValueFallback, "welcomeText", "nl", defaultValue: "woop");
        Assert.IsNull(value);

        // but it works with proper fallback settings
        value = content.Value(PublishedValueFallback, "welcomeText", "nl", fallback: Fallback.ToDefaultValue, defaultValue: "woop");
        Assert.AreEqual("woop", value);
    }
}
