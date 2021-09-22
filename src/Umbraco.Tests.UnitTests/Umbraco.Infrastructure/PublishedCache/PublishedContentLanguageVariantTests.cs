using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PublishedCache
{
    [TestFixture]
    public class PublishedContentLanguageVariantTests : PublishedSnapshotServiceTestBase
    {
        // TODO: Now we need to figure out how best to populate our caches
        // for variants without any XML translations. Ideally we also don't use
        // any of this 'Solid...' classes and just use the native published cache.
        // To do that, we just need to built up a list of ContentNodeKit.
        // And then we'll want to be able to try to automatically build the associated
        // content types based on that data just like we do with XML so it's all less manual.

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            var dataTypes = GetDefaultDataTypes();
            var cache = CreateCache(dataTypes, out ContentType[] contentTypes);

            Init(cache, contentTypes, dataTypes);
        }

        private IEnumerable<ContentNodeKit> CreateCache(IDataType[] dataTypes, out ContentType[] contentTypes)
        {
            var result = new List<ContentNodeKit>();

            var propertyDataTypes = new Dictionary<string, IDataType>
            {
                // we only have one data type for this test which will be resolved with string empty.
                [string.Empty] = dataTypes[0]
            };

            ContentData item1Data = new ContentDataBuilder()
                .WithName("Content 1")
                .WithCultureInfos(new Dictionary<string, CultureVariation>())
                .WithProperties(new PropertyDataBuilder()
                    .WithPropertyData("welcomeText", "Welcome")
                    .WithPropertyData("welcomeText", "Welcome", "en-US")
                    .WithPropertyData("welcomeText", "Willkommen", "de")
                    .WithPropertyData("welcomeText", "Welkom", "nl")
                    .WithPropertyData("welcomeText2", "Welcome")
                    .WithPropertyData("welcomeText2", "Welcome", "en-US")
                    .Build())
                // build with a dynamically created content type
                .Build(TestHelper.ShortStringHelper, "ContentType1", propertyDataTypes, out ContentType contentType1);

            ContentNodeKit item1 = ContentNodeKitBuilder.CreateWithContent(
                contentType1.Id,
                1, "-1,1",
                draftData: item1Data,
                publishedData: item1Data);

            result.Add(item1);

            ContentData item2Data = new ContentDataBuilder()
                .WithName("Content 2")
                .WithCultureInfos(new Dictionary<string, CultureVariation>())
                .WithProperties(new PropertyDataBuilder()
                    .WithPropertyData("welcomeText", "Welcome")
                    .WithPropertyData("welcomeText", "Welcome", "en-US")
                    .Build())
                // build while dynamically updating the same content type
                .Build(TestHelper.ShortStringHelper, propertyDataTypes, contentType1, out contentType1);

            ContentNodeKit item2 = ContentNodeKitBuilder.CreateWithContent(
                contentType1.Id,
                2, "-1,1,2",
                parentContentId: 1,
                draftData: item2Data,
                publishedData: item2Data);

            result.Add(item2);

            ContentData item3Data = new ContentDataBuilder()
                .WithName("Content 3")
                .WithCultureInfos(new Dictionary<string, CultureVariation>())
                .WithProperties(new PropertyDataBuilder()
                    .WithPropertyData("prop3", "Oxxo")
                    .WithPropertyData("prop3", "Oxxo", "en-US")
                    .Build())
                // build with a dynamically created content type
                .Build(TestHelper.ShortStringHelper, "ContentType2", propertyDataTypes, out ContentType contentType2);

            ContentNodeKit item3 = ContentNodeKitBuilder.CreateWithContent(
                contentType2.Id,
                3, "-1,1,2,3",
                parentContentId: 2,
                draftData: item3Data,
                publishedData: item3Data);

            result.Add(item3);

            contentTypes = new[] { contentType1, contentType2 };

            return result;
        }

        //protected ServiceContext GetServiceContext()
        //{
        //    var serviceContext = TestObjects.GetServiceContextMock(Factory);
        //    MockLocalizationService(serviceContext);
        //    return serviceContext;
        //}

        //private static void MockLocalizationService(ServiceContext serviceContext)
        //{
        //    // Set up languages.
        //    // Spanish falls back to English and Italian to Spanish (and then to English).
        //    // French has no fall back.
        //    // Danish, Swedish and Norweigan create an invalid loop.
        //    var globalSettings = new GlobalSettings();
        //    var languages = new List<Language>
        //        {
        //            new Language(globalSettings, "en-US") { Id = 1, CultureName = "English", IsDefault = true },
        //            new Language(globalSettings, "fr") { Id = 2, CultureName = "French" },
        //            new Language(globalSettings, "es") { Id = 3, CultureName = "Spanish", FallbackLanguageId = 1 },
        //            new Language(globalSettings, "it") { Id = 4, CultureName = "Italian", FallbackLanguageId = 3 },
        //            new Language(globalSettings, "de") { Id = 5, CultureName = "German" },
        //            new Language(globalSettings, "da") { Id = 6, CultureName = "Danish", FallbackLanguageId = 8 },
        //            new Language(globalSettings, "sv") { Id = 7, CultureName = "Swedish", FallbackLanguageId = 6 },
        //            new Language(globalSettings, "no") { Id = 8, CultureName = "Norweigan", FallbackLanguageId = 7 },
        //            new Language(globalSettings, "nl") { Id = 9, CultureName = "Dutch", FallbackLanguageId = 1 }
        //        };

        //    var localizationService = Mock.Get(serviceContext.LocalizationService);
        //    localizationService.Setup(x => x.GetAllLanguages()).Returns(languages);
        //    localizationService.Setup(x => x.GetLanguageById(It.IsAny<int>()))
        //        .Returns((int id) => languages.SingleOrDefault(y => y.Id == id));
        //    localizationService.Setup(x => x.GetLanguageByIsoCode(It.IsAny<string>()))
        //        .Returns((string c) => languages.SingleOrDefault(y => y.IsoCode == c));
        //}

        //internal override void PopulateCache(PublishedContentTypeFactory factory, SolidPublishedContentCache cache)
        //{
        //    var prop1Type = factory.CreatePropertyType("prop1", 1, variations: ContentVariation.Culture);
        //    var welcomeType = factory.CreatePropertyType("welcomeText", 1, variations: ContentVariation.Culture);
        //    var welcome2Type = factory.CreatePropertyType("welcomeText2", 1, variations: ContentVariation.Culture);
        //    var nopropType = factory.CreatePropertyType("noprop", 1, variations: ContentVariation.Culture);

        //    IEnumerable<IPublishedPropertyType> CreatePropertyTypes1(IPublishedContentType contentType)
        //    {
        //        yield return factory.CreatePropertyType(contentType, "prop1", 1, variations: ContentVariation.Culture);
        //        yield return factory.CreatePropertyType(contentType, "welcomeText", 1, variations: ContentVariation.Culture);
        //        yield return factory.CreatePropertyType(contentType, "welcomeText2", 1, variations: ContentVariation.Culture);
        //        yield return factory.CreatePropertyType(contentType, "noprop", 1, variations: ContentVariation.Culture);
        //    }

        //    var contentType1 = factory.CreateContentType(Guid.NewGuid(), 1, "ContentType1", Enumerable.Empty<string>(), CreatePropertyTypes1);

        //    IEnumerable<IPublishedPropertyType> CreatePropertyTypes2(IPublishedContentType contentType)
        //    {
        //        yield return factory.CreatePropertyType(contentType, "prop3", 1, variations: ContentVariation.Culture);
        //    }

        //    var contentType2 = factory.CreateContentType(Guid.NewGuid(), 2, "contentType2", Enumerable.Empty<string>(), CreatePropertyTypes2);




        //    var prop3 = new SolidPublishedPropertyWithLanguageVariants
        //    {
        //        Alias = "welcomeText",
        //        PropertyType = welcomeType
        //    };
        //    prop3.SetSourceValue("en-US", "Welcome", true);
        //    prop3.SetValue("en-US", "Welcome", true);

        //    var noprop = new SolidPublishedProperty
        //    {
        //        Alias = "noprop",
        //        PropertyType = nopropType
        //    };
        //    noprop.SolidHasValue = false; // has no value
        //    noprop.SolidValue = "xxx"; // but returns something



        //    var prop4 = new SolidPublishedPropertyWithLanguageVariants
        //    {
        //        Alias = "prop3",
        //        PropertyType = contentType2.GetPropertyType("prop3")
        //    };
        //    prop4.SetSourceValue("en-US", "Oxxo", true);
        //    prop4.SetValue("en-US", "Oxxo", true);

        //    var item3 = new SolidPublishedContent(contentType2)
        //    {
        //        Id = 3,
        //        SortOrder = 0,
        //        Name = "Content 3",
        //        UrlSegment = "content-3",
        //        Path = "/1/2/3",
        //        Level = 3,
        //        ParentId = 2,
        //        ChildIds = new int[] { },
        //        Properties = new Collection<IPublishedProperty>
        //        {
        //            prop4
        //        }
        //    };

        //    item1.Children = new List<IPublishedContent> { item2 };
        //    item2.Parent = item1;

        //    item2.Children = new List<IPublishedContent> { item3 };
        //    item3.Parent = item2;

        //    cache.Add(item1);
        //    cache.Add(item2);
        //    cache.Add(item3);
        //}

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

        //[Test]
        //public void Do_Not_GetContent_For_Unpopulated_Requested_Language_With_Fallback_Over_That_Loops()
        //{
        //    var content = Current.UmbracoContext.Content.GetAtRoot().First();
        //    var value = content.Value(Mock.Of<IPublishedValueFallback>(), "welcomeText", "no", fallback: Fallback.ToLanguage);
        //    Assert.IsNull(value);
        //}

        //[Test]
        //public void Do_Not_Get_Content_Recursively_Unless_Requested()
        //{
        //    var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First();
        //    var value = content.Value(Mock.Of<IPublishedValueFallback>(), "welcomeText2");
        //    Assert.IsNull(value);
        //}

        //[Test]
        //public void Can_Get_Content_Recursively()
        //{
        //    var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First();
        //    var value = content.Value(Factory.GetRequiredService<IPublishedValueFallback>(), "welcomeText2", fallback: Fallback.ToAncestors);
        //    Assert.AreEqual("Welcome", value);
        //}

        //[Test]
        //public void Do_Not_Get_Content_Recursively_Unless_Requested2()
        //{
        //    var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First().Children.First();
        //    Assert.IsNull(content.GetProperty("welcomeText2"));
        //    var value = content.Value(Mock.Of<IPublishedValueFallback>(), "welcomeText2");
        //    Assert.IsNull(value);
        //}

        //[Test]
        //public void Can_Get_Content_Recursively2()
        //{
        //    var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First().Children.First();
        //    Assert.IsNull(content.GetProperty("welcomeText2"));
        //    var value = content.Value(Factory.GetRequiredService<IPublishedValueFallback>(), "welcomeText2", fallback: Fallback.ToAncestors);
        //    Assert.AreEqual("Welcome", value);
        //}

        //[Test]
        //public void Can_Get_Content_Recursively3()
        //{
        //    var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First().Children.First();
        //    Assert.IsNull(content.GetProperty("noprop"));
        //    var value = content.Value(Factory.GetRequiredService<IPublishedValueFallback>(), "noprop", fallback: Fallback.ToAncestors);
        //    // property has no value but we still get the value (ie, the converter would do something)
        //    Assert.AreEqual("xxx", value);
        //}

        //[Test]
        //public void Can_Get_Content_With_Recursive_Priority()
        //{
        //    Current.VariationContextAccessor.VariationContext = new VariationContext("nl");
        //    var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First();

        //    var value = content.Value(Factory.GetRequiredService<IPublishedValueFallback>(), "welcomeText", "nl", fallback: Fallback.To(Fallback.Ancestors, Fallback.Language));

        //    // No Dutch value is directly assigned. Check has fallen back to Dutch value from parent.
        //    Assert.AreEqual("Welkom", value);
        //}

        //[Test]
        //public void Can_Get_Content_With_Fallback_Language_Priority()
        //{
        //    var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First();
        //    var value = content.Value(Factory.GetRequiredService<IPublishedValueFallback>(), "welcomeText", "nl", fallback: Fallback.ToLanguage);

        //    // No Dutch value is directly assigned.  Check has fallen back to English value from language variant.
        //    Assert.AreEqual("Welcome", value);
        //}

        //[Test]
        //public void Throws_For_Non_Supported_Fallback()
        //{
        //    var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First();
        //    Assert.Throws<NotSupportedException>(() => content.Value(Factory.GetRequiredService<IPublishedValueFallback>(), "welcomeText", "nl", fallback: Fallback.To(999)));
        //}

        //[Test]
        //public void Can_Fallback_To_Default_Value()
        //{
        //    var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First();

        //    // no Dutch value is assigned, so getting null
        //    var value = content.Value(Factory.GetRequiredService<IPublishedValueFallback>(), "welcomeText", "nl");
        //    Assert.IsNull(value);

        //    // even if we 'just' provide a default value
        //    value = content.Value(Factory.GetRequiredService<IPublishedValueFallback>(), "welcomeText", "nl", defaultValue: "woop");
        //    Assert.IsNull(value);

        //    // but it works with proper fallback settings
        //    value = content.Value(Factory.GetRequiredService<IPublishedValueFallback>(), "welcomeText", "nl", fallback: Fallback.ToDefaultValue, defaultValue: "woop");
        //    Assert.AreEqual("woop", value);
        //}

        //[Test]
        //public void Can_Have_Custom_Default_Value()
        //{
        //    var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First();

        //    // HACK: the value, pretend the converter would return something
        //    var prop = content.GetProperty("welcomeText") as SolidPublishedPropertyWithLanguageVariants;
        //    Assert.IsNotNull(prop);
        //    prop.SetValue("nl", "nope"); // HasValue false but getting value returns this

        //    // there is an EN value
        //    var value = content.Value(Factory.GetRequiredService<IPublishedValueFallback>(), "welcomeText", "en-US");
        //    Assert.AreEqual("Welcome", value);

        //    // there is no NL value and we get the 'converted' value
        //    value = content.Value(Factory.GetRequiredService<IPublishedValueFallback>(), "welcomeText", "nl");
        //    Assert.AreEqual("nope", value);

        //    // but it works with proper fallback settings
        //    value = content.Value(Factory.GetRequiredService<IPublishedValueFallback>(), "welcomeText", "nl", fallback: Fallback.ToDefaultValue, defaultValue: "woop");
        //    Assert.AreEqual("woop", value);
        //}
    }
}
