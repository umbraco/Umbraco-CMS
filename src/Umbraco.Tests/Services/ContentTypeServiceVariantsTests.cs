using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Tests.Testing;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.NuCache;
using Umbraco.Web.PublishedCache.NuCache.DataSource;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true, WithApplication = true)]
    public class ContentTypeServiceVariantsTests : TestWithSomeContentBase
    {
        protected override void Compose()
        {
            base.Compose();

            // pfew - see note in ScopedNuCacheTests?
            Composition.RegisterSingleton<IServerMessenger, LocalServerMessenger>();
            Composition.RegisterSingleton(f => Mock.Of<IServerRegistrar>());
            Composition.GetCollectionBuilder<CacheRefresherCollectionBuilder>()
                .Add(() => Composition.TypeLoader.GetCacheRefreshers());
        }

        protected override IPublishedSnapshotService CreatePublishedSnapshotService()
        {
            var options = new PublishedSnapshotService.Options { IgnoreLocalDb = true };
            var publishedSnapshotAccessor = new UmbracoContextPublishedSnapshotAccessor(Umbraco.Web.Composing.Current.UmbracoContextAccessor);
            var runtimeStateMock = new Mock<IRuntimeState>();
            runtimeStateMock.Setup(x => x.Level).Returns(() => RuntimeLevel.Run);

            var contentTypeFactory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), new PropertyValueConverterCollection(Array.Empty<IPropertyValueConverter>()), Mock.Of<IDataTypeService>());
            //var documentRepository = Mock.Of<IDocumentRepository>();
            var documentRepository = Factory.GetInstance<IDocumentRepository>();
            var mediaRepository = Mock.Of<IMediaRepository>();
            var memberRepository = Mock.Of<IMemberRepository>();

            return new PublishedSnapshotService(
                options,
                null,
                runtimeStateMock.Object,
                ServiceContext,
                contentTypeFactory,
                null,
                publishedSnapshotAccessor,
                Mock.Of<IVariationContextAccessor>(),
                Logger,
                ScopeProvider,
                documentRepository, mediaRepository, memberRepository,
                DefaultCultureAccessor,
                new DatabaseDataSource(),
                Factory.GetInstance<IGlobalSettings>(), new SiteDomainHelper());
        }

        public class LocalServerMessenger : ServerMessengerBase
        {
            public LocalServerMessenger()
                : base(false)
            { }

            protected override void DeliverRemote(ICacheRefresher refresher, MessageType messageType, IEnumerable<object> ids = null, string json = null)
            {
                throw new NotImplementedException();
            }
        }

        private void AssertJsonStartsWith(int id, string expected)
        {
            var json = GetJson(id).Replace('"', '\'');
            var pos = json.IndexOf("'cultureData':", StringComparison.InvariantCultureIgnoreCase);
            json = json.Substring(0, pos + "'cultureData':".Length);
            Assert.AreEqual(expected, json);
        }

        private string GetJson(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var selectJson = SqlContext.Sql().Select<ContentNuDto>().From<ContentNuDto>().Where<ContentNuDto>(x => x.NodeId == id && !x.Published);
                var dto = scope.Database.Fetch<ContentNuDto>(selectJson).FirstOrDefault();
                Assert.IsNotNull(dto);
                var json = dto.Data;
                return json;
            }
        }

        [Test]
        public void Change_Variations_SimpleContentType_VariantToInvariantAndBack()
        {
            // one simple content type, variant, with both variant and invariant properties
            // can change it to invariant and back

            var languageEn = new Language("en") { IsDefault = true };
            ServiceContext.LocalizationService.Save(languageEn);
            var languageFr = new Language("fr");
            ServiceContext.LocalizationService.Save(languageFr);

            var contentType = new ContentType(-1)
            {
                Alias = "contentType",
                Name = "contentType",
                Variations = ContentVariation.Culture
            };

            var properties = new PropertyTypeCollection(true)
            {
                new PropertyType("value1", ValueStorageType.Ntext)
                {
                    Alias = "value1",
                    DataTypeId = -88,
                    Variations = ContentVariation.Culture
                },
                new PropertyType("value2", ValueStorageType.Ntext)
                {
                    Alias = "value2",
                    DataTypeId = -88,
                    Variations = ContentVariation.Nothing
                }
            };

            contentType.PropertyGroups.Add(new PropertyGroup(properties) { Name = "Content" });
            ServiceContext.ContentTypeService.Save(contentType);

            var document = (IContent) new Content("document", -1, contentType);
            document.SetCultureName("doc1en", "en");
            document.SetCultureName("doc1fr", "fr");
            document.SetValue("value1", "v1en", "en");
            document.SetValue("value1", "v1fr", "fr");
            document.SetValue("value2", "v2");
            ServiceContext.ContentService.Save(document);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("doc1en", document.Name);
            Assert.AreEqual("doc1en", document.GetCultureName("en"));
            Assert.AreEqual("doc1fr", document.GetCultureName("fr"));
            Assert.AreEqual("v1en", document.GetValue("value1", "en"));
            Assert.AreEqual("v1fr", document.GetValue("value1", "fr"));
            Assert.AreEqual("v2", document.GetValue("value2"));

            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value1':[{'culture':'en','seg':'','val':'v1en'},{'culture':'fr','seg':'','val':'v1fr'}],'value2':[{'culture':'','seg':'','val':'v2'}]},'cultureData':");

            // switch content type to Nothing
            contentType.Variations = ContentVariation.Nothing;
            ServiceContext.ContentTypeService.Save(contentType);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("doc1en", document.Name);
            Assert.IsNull(document.GetCultureName("en"));
            Assert.IsNull(document.GetCultureName("fr"));
            Assert.IsNull(document.GetValue("value1", "en"));
            Assert.IsNull(document.GetValue("value1", "fr"));
            Assert.AreEqual("v1en", document.GetValue("value1"));
            Assert.AreEqual("v2", document.GetValue("value2"));

            Assert.IsFalse(document.ContentType.PropertyTypes.First(x => x.Alias == "value1").VariesByCulture());

            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value1':[{'culture':'','seg':'','val':'v1en'}],'value2':[{'culture':'','seg':'','val':'v2'}]},'cultureData':");

            // switch content back to Culture
            contentType.Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(contentType);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("doc1en", document.Name);
            Assert.AreEqual("doc1en", document.GetCultureName("en"));
            Assert.AreEqual("doc1fr", document.GetCultureName("fr"));
            Assert.IsNull(document.GetValue("value1", "en"));
            Assert.IsNull(document.GetValue("value1", "fr"));
            Assert.AreEqual("v1en", document.GetValue("value1"));
            Assert.AreEqual("v2", document.GetValue("value2"));

            Assert.IsFalse(document.ContentType.PropertyTypes.First(x => x.Alias == "value1").VariesByCulture());

            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value1':[{'culture':'','seg':'','val':'v1en'}],'value2':[{'culture':'','seg':'','val':'v2'}]},'cultureData':");

            // switch property back to Culture
            contentType.PropertyTypes.First(x => x.Alias == "value1").Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(contentType);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("doc1en", document.Name);
            Assert.AreEqual("doc1en", document.GetCultureName("en"));
            Assert.AreEqual("doc1fr", document.GetCultureName("fr"));
            Assert.AreEqual("v1en", document.GetValue("value1", "en"));
            Assert.AreEqual("v1fr", document.GetValue("value1", "fr"));
            Assert.AreEqual("v2", document.GetValue("value2"));

            Assert.IsTrue(document.ContentType.PropertyTypes.First(x => x.Alias == "value1").VariesByCulture());

            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value1':[{'culture':'en','seg':'','val':'v1en'},{'culture':'fr','seg':'','val':'v1fr'}],'value2':[{'culture':'','seg':'','val':'v2'}]},'cultureData':");
        }

        [Test]
        public void Change_Variations_SimpleContentType_InvariantToVariantAndBack()
        {
            // one simple content type, invariant
            // can change it to variant and back
            // can then switch one property to variant

            var languageEn = new Language("en") { IsDefault = true };
            ServiceContext.LocalizationService.Save(languageEn);
            var languageFr = new Language("fr");
            ServiceContext.LocalizationService.Save(languageFr);

            var contentType = new ContentType(-1)
            {
                Alias = "contentType",
                Name = "contentType",
                Variations = ContentVariation.Nothing
            };

            var properties = new PropertyTypeCollection(true)
            {
                new PropertyType("value1", ValueStorageType.Ntext)
                {
                    Alias = "value1",
                    DataTypeId = -88,
                    Variations = ContentVariation.Nothing
                },
                new PropertyType("value2", ValueStorageType.Ntext)
                {
                    Alias = "value2",
                    DataTypeId = -88,
                    Variations = ContentVariation.Nothing
                }
            };

            contentType.PropertyGroups.Add(new PropertyGroup(properties) { Name = "Content" });
            ServiceContext.ContentTypeService.Save(contentType);

            var document = (IContent) new Content("document", -1, contentType);
            document.Name = "doc1";
            document.SetValue("value1", "v1");
            document.SetValue("value2", "v2");
            ServiceContext.ContentService.Save(document);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("doc1", document.Name);
            Assert.IsNull(document.GetCultureName("en"));
            Assert.IsNull(document.GetCultureName("fr"));
            Assert.IsNull(document.GetValue("value1", "en"));
            Assert.IsNull(document.GetValue("value1", "fr"));
            Assert.AreEqual("v1", document.GetValue("value1"));
            Assert.AreEqual("v2", document.GetValue("value2"));

            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value1':[{'culture':'','seg':'','val':'v1'}],'value2':[{'culture':'','seg':'','val':'v2'}]},'cultureData':");

            // switch content type to Culture
            contentType.Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(contentType);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("doc1", document.GetCultureName("en"));
            Assert.IsNull(document.GetCultureName("fr"));
            Assert.IsNull(document.GetValue("value1", "en"));
            Assert.IsNull(document.GetValue("value1", "fr"));
            Assert.AreEqual("v1", document.GetValue("value1"));
            Assert.AreEqual("v2", document.GetValue("value2"));

            Assert.IsFalse(document.ContentType.PropertyTypes.First(x => x.Alias == "value1").VariesByCulture());

            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value1':[{'culture':'','seg':'','val':'v1'}],'value2':[{'culture':'','seg':'','val':'v2'}]},'cultureData':");

            // switch property to Culture
            contentType.PropertyTypes.First(x => x.Alias == "value1").Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(contentType);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("doc1", document.GetCultureName("en"));
            Assert.IsNull(document.GetCultureName("fr"));
            Assert.AreEqual("v1", document.GetValue("value1", "en"));
            Assert.IsNull(document.GetValue("value1", "fr"));
            Assert.AreEqual("v2", document.GetValue("value2"));

            Assert.IsTrue(document.ContentType.PropertyTypes.First(x => x.Alias == "value1").VariesByCulture());

            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value1':[{'culture':'en','seg':'','val':'v1'}],'value2':[{'culture':'','seg':'','val':'v2'}]},'cultureData':");

            // switch content back to Nothing
            contentType.Variations = ContentVariation.Nothing;
            ServiceContext.ContentTypeService.Save(contentType);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("doc1", document.Name);
            Assert.IsNull(document.GetCultureName("en"));
            Assert.IsNull(document.GetCultureName("fr"));
            Assert.IsNull(document.GetValue("value1", "en"));
            Assert.IsNull(document.GetValue("value1", "fr"));
            Assert.AreEqual("v1", document.GetValue("value1"));
            Assert.AreEqual("v2", document.GetValue("value2"));

            Assert.IsFalse(document.ContentType.PropertyTypes.First(x => x.Alias == "value1").VariesByCulture());

            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value1':[{'culture':'','seg':'','val':'v1'}],'value2':[{'culture':'','seg':'','val':'v2'}]},'cultureData':");
        }

        [Test]
        public void Change_Variations_SimpleContentType_VariantPropertyToInvariantAndBack()
        {
            // one simple content type, variant, with both variant and invariant properties
            // can change an invariant property to variant and back

            var languageEn = new Language("en") { IsDefault = true };
            ServiceContext.LocalizationService.Save(languageEn);
            var languageFr = new Language("fr");
            ServiceContext.LocalizationService.Save(languageFr);

            var contentType = new ContentType(-1)
            {
                Alias = "contentType",
                Name = "contentType",
                Variations = ContentVariation.Culture
            };

            var properties = new PropertyTypeCollection(true)
            {
                new PropertyType("value1", ValueStorageType.Ntext)
                {
                    Alias = "value1",
                    DataTypeId = -88,
                    Variations = ContentVariation.Culture
                },
                new PropertyType("value2", ValueStorageType.Ntext)
                {
                    Alias = "value2",
                    DataTypeId = -88,
                    Variations = ContentVariation.Nothing
                }
            };

            contentType.PropertyGroups.Add(new PropertyGroup(properties) { Name = "Content" });
            ServiceContext.ContentTypeService.Save(contentType);

            var document = (IContent)new Content("document", -1, contentType);
            document.SetCultureName("doc1en", "en");
            document.SetCultureName("doc1fr", "fr");
            document.SetValue("value1", "v1en", "en");
            document.SetValue("value1", "v1fr", "fr");
            document.SetValue("value2", "v2");
            ServiceContext.ContentService.Save(document);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("doc1en", document.Name);
            Assert.AreEqual("doc1en", document.GetCultureName("en"));
            Assert.AreEqual("doc1fr", document.GetCultureName("fr"));
            Assert.AreEqual("v1en", document.GetValue("value1", "en"));
            Assert.AreEqual("v1fr", document.GetValue("value1", "fr"));
            Assert.AreEqual("v2", document.GetValue("value2"));

            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value1':[{'culture':'en','seg':'','val':'v1en'},{'culture':'fr','seg':'','val':'v1fr'}],'value2':[{'culture':'','seg':'','val':'v2'}]},'cultureData':");

            // switch property type to Nothing
            contentType.PropertyTypes.First(x => x.Alias == "value1").Variations = ContentVariation.Nothing;
            ServiceContext.ContentTypeService.Save(contentType);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("doc1en", document.Name);
            Assert.AreEqual("doc1en", document.GetCultureName("en"));
            Assert.AreEqual("doc1fr", document.GetCultureName("fr"));
            Assert.IsNull(document.GetValue("value1", "en"));
            Assert.IsNull(document.GetValue("value1", "fr"));
            Assert.AreEqual("v1en", document.GetValue("value1"));
            Assert.AreEqual("v2", document.GetValue("value2"));

            Assert.IsFalse(document.ContentType.PropertyTypes.First(x => x.Alias == "value1").VariesByCulture());

            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value1':[{'culture':'','seg':'','val':'v1en'}],'value2':[{'culture':'','seg':'','val':'v2'}]},'cultureData':");

            // switch property back to Culture
            contentType.PropertyTypes.First(x => x.Alias == "value1").Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(contentType);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("doc1en", document.Name);
            Assert.AreEqual("doc1en", document.GetCultureName("en"));
            Assert.AreEqual("doc1fr", document.GetCultureName("fr"));
            Assert.AreEqual("v1en", document.GetValue("value1", "en"));
            Assert.AreEqual("v1fr", document.GetValue("value1", "fr"));
            Assert.AreEqual("v2", document.GetValue("value2"));

            Assert.IsTrue(document.ContentType.PropertyTypes.First(x => x.Alias == "value1").VariesByCulture());

            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value1':[{'culture':'en','seg':'','val':'v1en'},{'culture':'fr','seg':'','val':'v1fr'}],'value2':[{'culture':'','seg':'','val':'v2'}]},'cultureData':");

            // switch other property to Culture
            contentType.PropertyTypes.First(x => x.Alias == "value2").Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(contentType);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("doc1en", document.Name);
            Assert.AreEqual("doc1en", document.GetCultureName("en"));
            Assert.AreEqual("doc1fr", document.GetCultureName("fr"));
            Assert.AreEqual("v1en", document.GetValue("value1", "en"));
            Assert.AreEqual("v1fr", document.GetValue("value1", "fr"));
            Assert.AreEqual("v2", document.GetValue("value2", "en"));
            Assert.IsNull(document.GetValue("value2", "fr"));
            Assert.IsNull(document.GetValue("value2"));

            Assert.IsTrue(document.ContentType.PropertyTypes.First(x => x.Alias == "value2").VariesByCulture());

            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value1':[{'culture':'en','seg':'','val':'v1en'},{'culture':'fr','seg':'','val':'v1fr'}],'value2':[{'culture':'en','seg':'','val':'v2'}]},'cultureData':");
        }

        [Test]
        public void Change_Variations_ComposedContentType_1()
        {
            // one composing content type, variant, with both variant and invariant properties
            // one composed content type, variant, with both variant and invariant properties
            // can change the composing content type to invariant and back
            // can change the composed content type to invariant and back

            var languageEn = new Language("en") { IsDefault = true };
            ServiceContext.LocalizationService.Save(languageEn);
            var languageFr = new Language("fr");
            ServiceContext.LocalizationService.Save(languageFr);

            var composing = new ContentType(-1)
            {
                Alias = "composing",
                Name = "composing",
                Variations = ContentVariation.Culture
            };

            var properties1 = new PropertyTypeCollection(true)
            {
                new PropertyType("value11", ValueStorageType.Ntext)
                {
                    Alias = "value11",
                    DataTypeId = -88,
                    Variations = ContentVariation.Culture
                },
                new PropertyType("value12", ValueStorageType.Ntext)
                {
                    Alias = "value12",
                    DataTypeId = -88,
                    Variations = ContentVariation.Nothing
                }
            };

            composing.PropertyGroups.Add(new PropertyGroup(properties1) { Name = "Content" });
            ServiceContext.ContentTypeService.Save(composing);

            var composed = new ContentType(-1)
            {
                Alias = "composed",
                Name = "composed",
                Variations = ContentVariation.Culture
            };

            var properties2 = new PropertyTypeCollection(true)
            {
                new PropertyType("value21", ValueStorageType.Ntext)
                {
                    Alias = "value21",
                    DataTypeId = -88,
                    Variations = ContentVariation.Culture
                },
                new PropertyType("value22", ValueStorageType.Ntext)
                {
                    Alias = "value22",
                    DataTypeId = -88,
                    Variations = ContentVariation.Nothing
                }
            };

            composed.PropertyGroups.Add(new PropertyGroup(properties2) { Name = "Content" });
            composed.AddContentType(composing);
            ServiceContext.ContentTypeService.Save(composed);

            var document = (IContent) new Content("document", -1, composed);
            document.SetCultureName("doc1en", "en");
            document.SetCultureName("doc1fr", "fr");
            document.SetValue("value11", "v11en", "en");
            document.SetValue("value11", "v11fr", "fr");
            document.SetValue("value12", "v12");
            document.SetValue("value21", "v21en", "en");
            document.SetValue("value21", "v21fr", "fr");
            document.SetValue("value22", "v22");
            ServiceContext.ContentService.Save(document);

            // both value11 and value21 are variant
            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value11':[{'culture':'en','seg':'','val':'v11en'},{'culture':'fr','seg':'','val':'v11fr'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value21':[{'culture':'en','seg':'','val':'v21en'},{'culture':'fr','seg':'','val':'v21fr'}],'value22':[{'culture':'','seg':'','val':'v22'}]},'cultureData':");

            composed.Variations = ContentVariation.Nothing;
            ServiceContext.ContentTypeService.Save(composed);

            // both value11 and value21 are invariant
            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value11':[{'culture':'','seg':'','val':'v11en'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value21':[{'culture':'','seg':'','val':'v21en'}],'value22':[{'culture':'','seg':'','val':'v22'}]},'cultureData':");

            composed.Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(composed);

            // value11 is variant again, but value21 is still invariant
            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value11':[{'culture':'en','seg':'','val':'v11en'},{'culture':'fr','seg':'','val':'v11fr'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value21':[{'culture':'','seg':'','val':'v21en'}],'value22':[{'culture':'','seg':'','val':'v22'}]},'cultureData':");

            composed.PropertyTypes.First(x => x.Alias == "value21").Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(composed);

            // we can make it variant again
            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value11':[{'culture':'en','seg':'','val':'v11en'},{'culture':'fr','seg':'','val':'v11fr'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value21':[{'culture':'en','seg':'','val':'v21en'},{'culture':'fr','seg':'','val':'v21fr'}],'value22':[{'culture':'','seg':'','val':'v22'}]},'cultureData':");

            composing.Variations = ContentVariation.Nothing;
            ServiceContext.ContentTypeService.Save(composing);

            // value11 is invariant
            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value11':[{'culture':'','seg':'','val':'v11en'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value21':[{'culture':'en','seg':'','val':'v21en'},{'culture':'fr','seg':'','val':'v21fr'}],'value22':[{'culture':'','seg':'','val':'v22'}]},'cultureData':");

            composing.Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(composing);

            // value11 is still invariant
            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value11':[{'culture':'','seg':'','val':'v11en'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value21':[{'culture':'en','seg':'','val':'v21en'},{'culture':'fr','seg':'','val':'v21fr'}],'value22':[{'culture':'','seg':'','val':'v22'}]},'cultureData':");

            composing.PropertyTypes.First(x => x.Alias == "value11").Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(composing);

            // we can make it variant again
            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value11':[{'culture':'en','seg':'','val':'v11en'},{'culture':'fr','seg':'','val':'v11fr'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value21':[{'culture':'en','seg':'','val':'v21en'},{'culture':'fr','seg':'','val':'v21fr'}],'value22':[{'culture':'','seg':'','val':'v22'}]},'cultureData':");
        }

        [Test]
        public void Change_Variations_ComposedContentType_2()
        {
            // one composing content type, variant, with both variant and invariant properties
            // one composed content type, variant, with both variant and invariant properties
            // one composed content type, invariant
            // can change the composing content type to invariant and back
            // can change the variant composed content type to invariant and back

            var languageEn = new Language("en") { IsDefault = true };
            ServiceContext.LocalizationService.Save(languageEn);
            var languageFr = new Language("fr");
            ServiceContext.LocalizationService.Save(languageFr);

            var composing = new ContentType(-1)
            {
                Alias = "composing",
                Name = "composing",
                Variations = ContentVariation.Culture
            };

            var properties1 = new PropertyTypeCollection(true)
            {
                new PropertyType("value11", ValueStorageType.Ntext)
                {
                    Alias = "value11",
                    DataTypeId = -88,
                    Variations = ContentVariation.Culture
                },
                new PropertyType("value12", ValueStorageType.Ntext)
                {
                    Alias = "value12",
                    DataTypeId = -88,
                    Variations = ContentVariation.Nothing
                }
            };

            composing.PropertyGroups.Add(new PropertyGroup(properties1) { Name = "Content" });
            ServiceContext.ContentTypeService.Save(composing);

            var composed1 = new ContentType(-1)
            {
                Alias = "composed1",
                Name = "composed1",
                Variations = ContentVariation.Culture
            };

            var properties2 = new PropertyTypeCollection(true)
            {
                new PropertyType("value21", ValueStorageType.Ntext)
                {
                    Alias = "value21",
                    DataTypeId = -88,
                    Variations = ContentVariation.Culture
                },
                new PropertyType("value22", ValueStorageType.Ntext)
                {
                    Alias = "value22",
                    DataTypeId = -88,
                    Variations = ContentVariation.Nothing
                }
            };

            composed1.PropertyGroups.Add(new PropertyGroup(properties2) { Name = "Content" });
            composed1.AddContentType(composing);
            ServiceContext.ContentTypeService.Save(composed1);

            var composed2 = new ContentType(-1)
            {
                Alias = "composed2",
                Name = "composed2",
                Variations = ContentVariation.Nothing
            };

            var properties3 = new PropertyTypeCollection(true)
            {
                new PropertyType("value31", ValueStorageType.Ntext)
                {
                    Alias = "value31",
                    DataTypeId = -88,
                    Variations = ContentVariation.Nothing
                },
                new PropertyType("value32", ValueStorageType.Ntext)
                {
                    Alias = "value32",
                    DataTypeId = -88,
                    Variations = ContentVariation.Nothing
                }
            };

            composed2.PropertyGroups.Add(new PropertyGroup(properties3) { Name = "Content" });
            composed2.AddContentType(composing);
            ServiceContext.ContentTypeService.Save(composed2);

            var document1 = (IContent) new Content ("document1", -1, composed1);
            document1.SetCultureName("doc1en", "en");
            document1.SetCultureName("doc1fr", "fr");
            document1.SetValue("value11", "v11en", "en");
            document1.SetValue("value11", "v11fr", "fr");
            document1.SetValue("value12", "v12");
            document1.SetValue("value21", "v21en", "en");
            document1.SetValue("value21", "v21fr", "fr");
            document1.SetValue("value22", "v22");
            ServiceContext.ContentService.Save(document1);

            var document2 = (IContent)new Content("document2", -1, composed2);
            document2.Name = "doc2";
            document2.SetValue("value11", "v11");
            document2.SetValue("value12", "v12");
            document2.SetValue("value31", "v31");
            document2.SetValue("value32", "v32");
            ServiceContext.ContentService.Save(document2);

            // both value11 and value21 are variant
            Console.WriteLine(GetJson(document1.Id));
            AssertJsonStartsWith(document1.Id,
                "{'properties':{'value11':[{'culture':'en','seg':'','val':'v11en'},{'culture':'fr','seg':'','val':'v11fr'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value21':[{'culture':'en','seg':'','val':'v21en'},{'culture':'fr','seg':'','val':'v21fr'}],'value22':[{'culture':'','seg':'','val':'v22'}]},'cultureData':");

            Console.WriteLine(GetJson(document2.Id));
            AssertJsonStartsWith(document2.Id,
                "{'properties':{'value11':[{'culture':'','seg':'','val':'v11'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value31':[{'culture':'','seg':'','val':'v31'}],'value32':[{'culture':'','seg':'','val':'v32'}]},'cultureData':");

            composed1.Variations = ContentVariation.Nothing;
            ServiceContext.ContentTypeService.Save(composed1);

            // both value11 and value21 are invariant
            Console.WriteLine(GetJson(document1.Id));
            AssertJsonStartsWith(document1.Id,
                "{'properties':{'value11':[{'culture':'','seg':'','val':'v11en'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value21':[{'culture':'','seg':'','val':'v21en'}],'value22':[{'culture':'','seg':'','val':'v22'}]},'cultureData':");

            Console.WriteLine(GetJson(document2.Id));
            AssertJsonStartsWith(document2.Id,
                "{'properties':{'value11':[{'culture':'','seg':'','val':'v11'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value31':[{'culture':'','seg':'','val':'v31'}],'value32':[{'culture':'','seg':'','val':'v32'}]},'cultureData':");

            composed1.Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(composed1);

            // value11 is variant again, but value21 is still invariant
            Console.WriteLine(GetJson(document1.Id));
            AssertJsonStartsWith(document1.Id,
                "{'properties':{'value11':[{'culture':'en','seg':'','val':'v11en'},{'culture':'fr','seg':'','val':'v11fr'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value21':[{'culture':'','seg':'','val':'v21en'}],'value22':[{'culture':'','seg':'','val':'v22'}]},'cultureData':");

            Console.WriteLine(GetJson(document2.Id));
            AssertJsonStartsWith(document2.Id,
                "{'properties':{'value11':[{'culture':'','seg':'','val':'v11'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value31':[{'culture':'','seg':'','val':'v31'}],'value32':[{'culture':'','seg':'','val':'v32'}]},'cultureData':");

            composed1.PropertyTypes.First(x => x.Alias == "value21").Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(composed1);

            // we can make it variant again
            Console.WriteLine(GetJson(document1.Id));
            AssertJsonStartsWith(document1.Id,
                "{'properties':{'value11':[{'culture':'en','seg':'','val':'v11en'},{'culture':'fr','seg':'','val':'v11fr'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value21':[{'culture':'en','seg':'','val':'v21en'},{'culture':'fr','seg':'','val':'v21fr'}],'value22':[{'culture':'','seg':'','val':'v22'}]},'cultureData':");

            Console.WriteLine(GetJson(document2.Id));
            AssertJsonStartsWith(document2.Id,
                "{'properties':{'value11':[{'culture':'','seg':'','val':'v11'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value31':[{'culture':'','seg':'','val':'v31'}],'value32':[{'culture':'','seg':'','val':'v32'}]},'cultureData':");

            composing.Variations = ContentVariation.Nothing;
            ServiceContext.ContentTypeService.Save(composing);

            // value11 is invariant
            Console.WriteLine(GetJson(document1.Id));
            AssertJsonStartsWith(document1.Id,
                "{'properties':{'value11':[{'culture':'','seg':'','val':'v11en'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value21':[{'culture':'en','seg':'','val':'v21en'},{'culture':'fr','seg':'','val':'v21fr'}],'value22':[{'culture':'','seg':'','val':'v22'}]},'cultureData':");

            Console.WriteLine(GetJson(document2.Id));
            AssertJsonStartsWith(document2.Id,
                "{'properties':{'value11':[{'culture':'','seg':'','val':'v11'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value31':[{'culture':'','seg':'','val':'v31'}],'value32':[{'culture':'','seg':'','val':'v32'}]},'cultureData':");

            composing.Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(composing);

            // value11 is still invariant
            Console.WriteLine(GetJson(document1.Id));
            AssertJsonStartsWith(document1.Id,
                "{'properties':{'value11':[{'culture':'','seg':'','val':'v11en'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value21':[{'culture':'en','seg':'','val':'v21en'},{'culture':'fr','seg':'','val':'v21fr'}],'value22':[{'culture':'','seg':'','val':'v22'}]},'cultureData':");

            Console.WriteLine(GetJson(document2.Id));
            AssertJsonStartsWith(document2.Id,
                "{'properties':{'value11':[{'culture':'','seg':'','val':'v11'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value31':[{'culture':'','seg':'','val':'v31'}],'value32':[{'culture':'','seg':'','val':'v32'}]},'cultureData':");

            composing.PropertyTypes.First(x => x.Alias == "value11").Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(composing);

            // we can make it variant again
            Console.WriteLine(GetJson(document1.Id));
            AssertJsonStartsWith(document1.Id,
                "{'properties':{'value11':[{'culture':'en','seg':'','val':'v11en'},{'culture':'fr','seg':'','val':'v11fr'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value21':[{'culture':'en','seg':'','val':'v21en'},{'culture':'fr','seg':'','val':'v21fr'}],'value22':[{'culture':'','seg':'','val':'v22'}]},'cultureData':");

            Console.WriteLine(GetJson(document2.Id));
            AssertJsonStartsWith(document2.Id,
                "{'properties':{'value11':[{'culture':'','seg':'','val':'v11'}],'value12':[{'culture':'','seg':'','val':'v12'}],'value31':[{'culture':'','seg':'','val':'v31'}],'value32':[{'culture':'','seg':'','val':'v32'}]},'cultureData':");
        }
    }
}
