using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Tests.Testing;
using Umbraco.Web;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    [UmbracoTest(TypeLoader = UmbracoTestOptions.TypeLoader.PerFixture)]
    public class PublishedContentLanguageVariantTests : PublishedContentSnapshotTestBase
    {
        protected override void Compose()
        {
            base.Compose();

            Composition.RegisterUnique(_ => GetServiceContext());
        }

        protected ServiceContext GetServiceContext()
        {
            var serviceContext = TestObjects.GetServiceContextMock(Factory);
            MockLocalizationService(serviceContext);
            return serviceContext;
        }

        private static void MockLocalizationService(ServiceContext serviceContext)
        {
            // Set up languages.
            // Spanish falls back to English and Italian to Spanish (and then to English).
            // French has no fall back.
            // Danish, Swedish and Norweigan create an invalid loop.
            var languages = new List<Language>
                {
                    new Language("en-US") { Id = 1, CultureName = "English", IsDefault = true },
                    new Language("fr") { Id = 2, CultureName = "French" },
                    new Language("es") { Id = 3, CultureName = "Spanish", FallbackLanguageId = 1 },
                    new Language("it") { Id = 4, CultureName = "Italian", FallbackLanguageId = 3 },
                    new Language("de") { Id = 5, CultureName = "German" },
                    new Language("da") { Id = 6, CultureName = "Danish", FallbackLanguageId = 8 },
                    new Language("sv") { Id = 7, CultureName = "Swedish", FallbackLanguageId = 6 },
                    new Language("no") { Id = 8, CultureName = "Norweigan", FallbackLanguageId = 7 },
                    new Language("nl") { Id = 9, CultureName = "Dutch", FallbackLanguageId = 1 }
                };

            var localizationService = Mock.Get(serviceContext.LocalizationService);
            localizationService.Setup(x => x.GetAllLanguages()).Returns(languages);
            localizationService.Setup(x => x.GetLanguageById(It.IsAny<int>()))
                .Returns((int id) => languages.SingleOrDefault(y => y.Id == id));
            localizationService.Setup(x => x.GetLanguageByIsoCode(It.IsAny<string>()))
                .Returns((string c) => languages.SingleOrDefault(y => y.IsoCode == c));
        }

        internal override void PopulateCache(PublishedContentTypeFactory factory, SolidPublishedContentCache cache)
        {
            var prop1Type = factory.CreatePropertyType("prop1", 1, variations: ContentVariation.Culture);
            var welcomeType = factory.CreatePropertyType("welcomeText", 1, variations: ContentVariation.Culture);
            var welcome2Type = factory.CreatePropertyType("welcomeText2", 1, variations: ContentVariation.Culture);
            var nopropType = factory.CreatePropertyType("noprop", 1, variations: ContentVariation.Culture);

            IEnumerable<IPublishedPropertyType> CreatePropertyTypes1(IPublishedContentType contentType)
            {
                yield return factory.CreatePropertyType(contentType, "prop1", 1, variations: ContentVariation.Culture);
                yield return factory.CreatePropertyType(contentType, "welcomeText", 1, variations: ContentVariation.Culture);
                yield return factory.CreatePropertyType(contentType, "welcomeText2", 1, variations: ContentVariation.Culture);
                yield return factory.CreatePropertyType(contentType, "noprop", 1, variations: ContentVariation.Culture);
            }

            var contentType1 = factory.CreateContentType(Guid.NewGuid(), 1, "ContentType1", Enumerable.Empty<string>(), CreatePropertyTypes1);

            IEnumerable<IPublishedPropertyType> CreatePropertyTypes2(IPublishedContentType contentType)
            {
                yield return factory.CreatePropertyType(contentType, "prop3", 1, variations: ContentVariation.Culture);
            }

            var contentType2 = factory.CreateContentType(Guid.NewGuid(), 2, "contentType2", Enumerable.Empty<string>(), CreatePropertyTypes2);

            var prop1 = new SolidPublishedPropertyWithLanguageVariants
            {
                Alias = "welcomeText",
                PropertyType = welcomeType
            };
            prop1.SetSourceValue("en-US", "Welcome", true);
            prop1.SetValue("en-US", "Welcome", true);
            prop1.SetSourceValue("de", "Willkommen");
            prop1.SetValue("de", "Willkommen");
            prop1.SetSourceValue("nl", "Welkom");
            prop1.SetValue("nl", "Welkom");

            var prop2 = new SolidPublishedPropertyWithLanguageVariants
            {
                Alias = "welcomeText2",
                PropertyType = welcome2Type
            };
            prop2.SetSourceValue("en-US", "Welcome", true);
            prop2.SetValue("en-US", "Welcome", true);

            var prop3 = new SolidPublishedPropertyWithLanguageVariants
            {
                Alias = "welcomeText",
                PropertyType = welcomeType
            };
            prop3.SetSourceValue("en-US", "Welcome", true);
            prop3.SetValue("en-US", "Welcome", true);

            var noprop = new SolidPublishedProperty
            {
                Alias = "noprop",
                PropertyType = nopropType
            };
            noprop.SolidHasValue = false; // has no value
            noprop.SolidValue = "xxx"; // but returns something

            var item1 = new SolidPublishedContent(contentType1)
            {
                Id = 1,
                SortOrder = 0,
                Name = "Content 1",
                UrlSegment = "content-1",
                Path = "/1",
                Level = 1,
                Url = "/content-1",
                ParentId = -1,
                ChildIds = new[] { 2 },
                Properties = new Collection<IPublishedProperty>
                    {
                        prop1, prop2, noprop
                    }
            };

            var item2 = new SolidPublishedContent(contentType1)
            {
                Id = 2,
                SortOrder = 0,
                Name = "Content 2",
                UrlSegment = "content-2",
                Path = "/1/2",
                Level = 2,
                Url = "/content-1/content-2",
                ParentId = 1,
                ChildIds = new int[] { 3 },
                Properties = new Collection<IPublishedProperty>
                    {
                        prop3
                    }
            };

            var prop4 = new SolidPublishedPropertyWithLanguageVariants
            {
                Alias = "prop3",
                PropertyType = contentType2.GetPropertyType("prop3")
            };
            prop4.SetSourceValue("en-US", "Oxxo", true);
            prop4.SetValue("en-US", "Oxxo", true);

            var item3 = new SolidPublishedContent(contentType2)
            {
                Id = 3,
                SortOrder = 0,
                Name = "Content 3",
                UrlSegment = "content-3",
                Path = "/1/2/3",
                Level = 3,
                Url = "/content-1/content-2/content-3",
                ParentId = 2,
                ChildIds = new int[] { },
                Properties = new Collection<IPublishedProperty>
                {
                    prop4
                }
            };

            item1.Children = new List<IPublishedContent> { item2 };
            item2.Parent = item1;

            item2.Children = new List<IPublishedContent> { item3 };
            item3.Parent = item2;

            cache.Add(item1);
            cache.Add(item2);
            cache.Add(item3);
        }

        [Test]
        public void Can_Get_Content_For_Populated_Requested_Language()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot().First();
            var value = content.Value("welcomeText", "en-US");
            Assert.AreEqual("Welcome", value);
        }

        [Test]
        public void Can_Get_Content_For_Populated_Requested_Non_Default_Language()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot().First();
            var value = content.Value("welcomeText", "de");
            Assert.AreEqual("Willkommen", value);
        }

        [Test]
        public void Do_Not_Get_Content_For_Unpopulated_Requested_Language_Without_Fallback()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot().First();
            var value = content.Value("welcomeText", "fr");
            Assert.IsNull(value);
        }

        [Test]
        public void Do_Not_Get_Content_For_Unpopulated_Requested_Language_With_Fallback_Unless_Requested()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot().First();
            var value = content.Value("welcomeText", "es");
            Assert.IsNull(value);
        }

        [Test]
        public void Can_Get_Content_For_Unpopulated_Requested_Language_With_Fallback()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot().First();
            var value = content.Value("welcomeText", "es", fallback: Fallback.ToLanguage);
            Assert.AreEqual("Welcome", value);
        }

        [Test]
        public void Can_Get_Content_For_Unpopulated_Requested_Language_With_Fallback_Over_Two_Levels()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot().First();
            var value = content.Value("welcomeText", "it", fallback: Fallback.To(Fallback.Language, Fallback.Ancestors));
            Assert.AreEqual("Welcome", value);
        }

        [Test]
        public void Do_Not_GetContent_For_Unpopulated_Requested_Language_With_Fallback_Over_That_Loops()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot().First();
            var value = content.Value("welcomeText", "no", fallback: Fallback.ToLanguage);
            Assert.IsNull(value);
        }

        [Test]
        public void Do_Not_Get_Content_Recursively_Unless_Requested()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First();
            var value = content.Value("welcomeText2");
            Assert.IsNull(value);
        }

        [Test]
        public void Can_Get_Content_Recursively()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First();
            var value = content.Value("welcomeText2", fallback: Fallback.ToAncestors);
            Assert.AreEqual("Welcome", value);
        }

        [Test]
        public void Do_Not_Get_Content_Recursively_Unless_Requested2()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First().Children.First();
            Assert.IsNull(content.GetProperty("welcomeText2"));
            var value = content.Value("welcomeText2");
            Assert.IsNull(value);
        }

        [Test]
        public void Can_Get_Content_Recursively2()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First().Children.First();
            Assert.IsNull(content.GetProperty("welcomeText2"));
            var value = content.Value("welcomeText2", fallback: Fallback.ToAncestors);
            Assert.AreEqual("Welcome", value);
        }

        [Test]
        public void Can_Get_Content_Recursively3()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First().Children.First();
            Assert.IsNull(content.GetProperty("noprop"));
            var value = content.Value("noprop", fallback: Fallback.ToAncestors);
            // property has no value but we still get the value (ie, the converter would do something)
            Assert.AreEqual("xxx", value);
        }

        [Test]
        public void Can_Get_Content_With_Recursive_Priority()
        {
            Current.VariationContextAccessor.VariationContext = new VariationContext("nl");
            var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First();

            var value = content.Value("welcomeText", "nl", fallback: Fallback.To(Fallback.Ancestors, Fallback.Language));

            // No Dutch value is directly assigned. Check has fallen back to Dutch value from parent.
            Assert.AreEqual("Welkom", value);
        }

        [Test]
        public void Can_Get_Content_With_Fallback_Language_Priority()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First();
            var value = content.Value("welcomeText", "nl", fallback: Fallback.ToLanguage);

            // No Dutch value is directly assigned.  Check has fallen back to English value from language variant.
            Assert.AreEqual("Welcome", value);
        }

        [Test]
        public void Throws_For_Non_Supported_Fallback()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First();
            Assert.Throws<NotSupportedException>(() => content.Value("welcomeText", "nl", fallback: Fallback.To(999)));
        }

        [Test]
        public void Can_Fallback_To_Default_Value()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First();

            // no Dutch value is assigned, so getting null
            var value = content.Value("welcomeText", "nl");
            Assert.IsNull(value);

            // even if we 'just' provide a default value
            value = content.Value("welcomeText", "nl", defaultValue: "woop");
            Assert.IsNull(value);

            // but it works with proper fallback settings
            value = content.Value("welcomeText", "nl", fallback: Fallback.ToDefaultValue, defaultValue: "woop");
            Assert.AreEqual("woop", value);
        }

        [Test]
        public void Can_Have_Custom_Default_Value()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot().First().Children.First();

            // HACK: the value, pretend the converter would return something
            var prop = content.GetProperty("welcomeText") as SolidPublishedPropertyWithLanguageVariants;
            Assert.IsNotNull(prop);
            prop.SetValue("nl", "nope"); // HasValue false but getting value returns this

            // there is an EN value
            var value = content.Value("welcomeText", "en-US");
            Assert.AreEqual("Welcome", value);

            // there is no NL value and we get the 'converted' value
            value = content.Value("welcomeText", "nl");
            Assert.AreEqual("nope", value);

            // but it works with proper fallback settings
            value = content.Value("welcomeText", "nl", fallback: Fallback.ToDefaultValue, defaultValue: "woop");
            Assert.AreEqual("woop", value);
        }
    }
}
