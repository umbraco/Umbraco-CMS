using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Tests.Testing;
using Umbraco.Web;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    [UmbracoTest(PluginManager = UmbracoTestOptions.PluginManager.PerFixture)]
    public class PublishedContentLanuageVariantTests : PublishedContentSnapshotTestBase
    {
        protected override void Compose()
        {
            base.Compose();

            Container.RegisterSingleton(_ => GetServiceContext());
        }

        protected ServiceContext GetServiceContext()
        {
            var serviceContext = TestObjects.GetServiceContextMock(Container);
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
                    new Language("en-US") { Id = 1, CultureName = "English", IsDefaultVariantLanguage = true },
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
            var props = new[]
                {
                    factory.CreatePropertyType("prop1", 1),
                };
            var contentType1 = factory.CreateContentType(1, "ContentType1", Enumerable.Empty<string>(), props);

            var prop1 = new SolidPublishedPropertyWithLanguageVariants
                {
                    Alias = "welcomeText",
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
                };
            prop2.SetSourceValue("en-US", "Welcome", true);
            prop2.SetValue("en-US", "Welcome", true);

            var prop3 = new SolidPublishedPropertyWithLanguageVariants
                {
                    Alias = "welcomeText",
                };
            prop3.SetSourceValue("en-US", "Welcome", true);
            prop3.SetValue("en-US", "Welcome", true);

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
                        prop1, prop2
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
                ChildIds = new int[] { },
                Properties = new Collection<IPublishedProperty>
                    {
                        prop3
                    }
            };

            item1.Children = new List<IPublishedContent> { item2 };
            item2.Parent = item1;

            cache.Add(item1);
            cache.Add(item2);
        }

        [Test]
        public void Can_Get_Content_For_Populated_Requested_Language()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot().First();
            var value = content.Value("welcomeText", "en-US");
            Assert.AreEqual("Welcome", value);
        }

        [Test]
        public void Can_Get_Content_For_Populated_Requested_Non_Default_Language()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot().First();
            var value = content.Value("welcomeText", "de");
            Assert.AreEqual("Willkommen", value);
        }

        [Test]
        public void Do_Not_Get_Content_For_Unpopulated_Requested_Language_Without_Fallback()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot().First();
            var value = content.Value("welcomeText", "fr");
            Assert.IsNull(value);
        }

        [Test]
        public void Do_Not_Get_Content_For_Unpopulated_Requested_Language_With_Fallback_Unless_Requested()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot().First();
            var value = content.Value("welcomeText", "es");
            Assert.IsNull(value);
        }

        [Test]
        public void Can_Get_Content_For_Unpopulated_Requested_Language_With_Fallback()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot().First();
            var value = content.Value("welcomeText", "es", fallbackMethods: new[] { Core.Constants.Content.FallbackMethods.FallbackLanguage });
            Assert.AreEqual("Welcome", value);
        }

        [Test]
        public void Can_Get_Content_For_Unpopulated_Requested_Language_With_Fallback_Over_Two_Levels()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot().First();
            var value = content.Value("welcomeText", "it", fallbackMethods: new[] { Core.Constants.Content.FallbackMethods.FallbackLanguage });
            Assert.AreEqual("Welcome", value);
        }

        [Test]
        public void Do_Not_GetContent_For_Unpopulated_Requested_Language_With_Fallback_Over_That_Loops()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot().First();
            var value = content.Value("welcomeText", "no", fallbackMethods: new[] { Core.Constants.Content.FallbackMethods.FallbackLanguage });
            Assert.IsNull(value);
        }

        [Test]
        public void Do_Not_Get_Content_Recursively_Unless_Requested()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot().First().Children.First();
            var value = content.Value("welcomeText2");
            Assert.IsNull(value);
        }

        [Test]
        public void Can_Get_Content_Recursively()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot().First().Children.First();
            var value = content.Value("welcomeText2", fallbackMethods: new[] { Core.Constants.Content.FallbackMethods.RecursiveTree });
            Assert.AreEqual("Welcome", value);
        }

        [Test]
        public void Can_Get_Content_With_Recursive_Priority()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot().First().Children.First();
            var value = content.Value("welcomeText", "nl", fallbackMethods: new[] { Core.Constants.Content.FallbackMethods.RecursiveTree, Core.Constants.Content.FallbackMethods.FallbackLanguage });

            // No Dutch value is directly assigned.  Check has fallen back to Dutch value from parent.
            Assert.AreEqual("Welkom", value);
        }

        [Test]
        public void Can_Get_Content_With_Fallback_Language_Priority()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot().First().Children.First();
            var value = content.Value("welcomeText", "nl", fallbackMethods: new[] { Core.Constants.Content.FallbackMethods.FallbackLanguage, Core.Constants.Content.FallbackMethods.RecursiveTree });

            // No Dutch value is directly assigned.  Check has fallen back to English value from language variant.
            Assert.AreEqual("Welcome", value);
        }
    }
}
