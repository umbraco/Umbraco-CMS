﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.NuCache;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

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
            Composition.RegisterUnique<IServerMessenger, LocalServerMessenger>();
            Composition.RegisterUnique(f => Mock.Of<IServerRegistrar>());
            Composition.WithCollectionBuilder<CacheRefresherCollectionBuilder>()
                .Add(() => Composition.TypeLoader.GetCacheRefreshers());
        }

        protected override IPublishedSnapshotService CreatePublishedSnapshotService()
        {
            var options = new PublishedSnapshotServiceOptions { IgnoreLocalDb = true };
            var publishedSnapshotAccessor = new UmbracoContextPublishedSnapshotAccessor(Umbraco.Web.Composing.Current.UmbracoContextAccessor);
            var runtimeStateMock = new Mock<IRuntimeState>();
            runtimeStateMock.Setup(x => x.Level).Returns(() => RuntimeLevel.Run);

            var contentTypeFactory = Factory.GetInstance<IPublishedContentTypeFactory>();            
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
                ProfilingLogger,
                ScopeProvider,
                documentRepository, mediaRepository, memberRepository,
                DefaultCultureAccessor,
                new DatabaseDataSource(),
                Factory.GetInstance<IGlobalSettings>(),
                Factory.GetInstance<IEntityXmlSerializer>(),
                Mock.Of<IPublishedModelFactory>(),
                new UrlSegmentProviderCollection(new[] { new DefaultUrlSegmentProvider() }));
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

        [TestCase(ContentVariation.Nothing, ContentVariation.Nothing, false)]
        [TestCase(ContentVariation.Nothing, ContentVariation.Culture, true)]
        [TestCase(ContentVariation.Nothing, ContentVariation.CultureAndSegment, true)]
        [TestCase(ContentVariation.Nothing, ContentVariation.Segment, true)]
        [TestCase(ContentVariation.Culture, ContentVariation.Nothing, true)]
        [TestCase(ContentVariation.Culture, ContentVariation.Culture, false)]
        [TestCase(ContentVariation.Culture, ContentVariation.Segment, true)]
        [TestCase(ContentVariation.Culture, ContentVariation.CultureAndSegment, true)]
        [TestCase(ContentVariation.Segment, ContentVariation.Nothing, true)]
        [TestCase(ContentVariation.Segment, ContentVariation.Culture, true)]
        [TestCase(ContentVariation.Segment, ContentVariation.Segment, false)]
        [TestCase(ContentVariation.Segment, ContentVariation.CultureAndSegment, true)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Nothing, true)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Culture, true)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Segment, true)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.CultureAndSegment, false)]
        public void Change_Content_Type_Variation_Clears_Redirects(ContentVariation startingContentTypeVariation, ContentVariation changedContentTypeVariation, bool shouldUrlRedirectsBeCleared)
        {
            var contentType = MockedContentTypes.CreateBasicContentType();            
            contentType.Variations = startingContentTypeVariation;            
            ServiceContext.ContentTypeService.Save(contentType);
            var contentType2 = MockedContentTypes.CreateBasicContentType("test");
            ServiceContext.ContentTypeService.Save(contentType2);

            //create some content of this content type
            IContent doc = MockedContent.CreateBasicContent(contentType);
            doc.Name = "Hello1";
            if(startingContentTypeVariation.HasFlag(ContentVariation.Culture))
            {
                doc.SetCultureName(doc.Name, "en-US");
            }

            ServiceContext.ContentService.Save(doc);

            IContent doc2 = MockedContent.CreateBasicContent(contentType2);
            ServiceContext.ContentService.Save(doc2);

            ServiceContext.RedirectUrlService.Register("hello/world", doc.Key);
            ServiceContext.RedirectUrlService.Register("hello2/world2", doc2.Key);

            // These 2 assertions should probably be moved to a test for the Register() method?
            Assert.AreEqual(1, ServiceContext.RedirectUrlService.GetContentRedirectUrls(doc.Key).Count());
            Assert.AreEqual(1, ServiceContext.RedirectUrlService.GetContentRedirectUrls(doc2.Key).Count());

            //change variation
            contentType.Variations = changedContentTypeVariation;
            ServiceContext.ContentTypeService.Save(contentType);
            var expectedRedirectUrlCount = shouldUrlRedirectsBeCleared ? 0 : 1;
            Assert.AreEqual(expectedRedirectUrlCount, ServiceContext.RedirectUrlService.GetContentRedirectUrls(doc.Key).Count());
            Assert.AreEqual(1, ServiceContext.RedirectUrlService.GetContentRedirectUrls(doc2.Key).Count());
        }

        [TestCase(ContentVariation.Nothing, ContentVariation.Culture)]
        [TestCase(ContentVariation.Nothing, ContentVariation.CultureAndSegment)]
        [TestCase(ContentVariation.Segment, ContentVariation.Culture)]
        [TestCase(ContentVariation.Segment, ContentVariation.CultureAndSegment)]
        public void Change_Content_Type_From_No_Culture_To_Culture(ContentVariation from, ContentVariation to)
        {
            var contentType = MockedContentTypes.CreateBasicContentType();
            contentType.Variations = from;
            var properties = CreatePropertyCollection(("title", from));
            contentType.PropertyGroups.Add(new PropertyGroup(properties) { Name = "Content" });
            ServiceContext.ContentTypeService.Save(contentType);

            //create some content of this content type
            IContent doc = MockedContent.CreateBasicContent(contentType);
            doc.Name = "Hello1";
            doc.SetValue("title", "hello world");
            ServiceContext.ContentService.Save(doc);

            doc = ServiceContext.ContentService.GetById(doc.Id); //re-get

            Assert.AreEqual("Hello1", doc.Name);
            Assert.AreEqual("hello world", doc.GetValue("title"));
            Assert.IsTrue(doc.Edited);
            Assert.IsFalse(doc.IsCultureEdited("en-US"));

            //change the content type to be variant, we will also update the name here to detect the copy changes
            doc.Name = "Hello2";
            ServiceContext.ContentService.Save(doc);
            contentType.Variations = to;
            ServiceContext.ContentTypeService.Save(contentType);
            doc = ServiceContext.ContentService.GetById(doc.Id); //re-get

            Assert.AreEqual("Hello2", doc.GetCultureName("en-US"));
            Assert.AreEqual("hello world", doc.GetValue("title")); //We are not checking against en-US here because properties will remain invariant
            Assert.IsTrue(doc.Edited);
            Assert.IsTrue(doc.IsCultureEdited("en-US"));

            //change back property type to be invariant, we will also update the name here to detect the copy changes
            doc.SetCultureName("Hello3", "en-US");
            ServiceContext.ContentService.Save(doc);
            contentType.Variations = from;
            ServiceContext.ContentTypeService.Save(contentType);
            doc = ServiceContext.ContentService.GetById(doc.Id); //re-get

            Assert.AreEqual("Hello3", doc.Name);
            Assert.AreEqual("hello world", doc.GetValue("title"));
            Assert.IsTrue(doc.Edited);
            Assert.IsFalse(doc.IsCultureEdited("en-US"));
        }

        [TestCase(ContentVariation.Culture, ContentVariation.Nothing)]
        [TestCase(ContentVariation.Culture, ContentVariation.Segment)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Nothing)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Segment)]
        public void Change_Content_Type_From_Culture_To_No_Culture(ContentVariation startingContentTypeVariation, ContentVariation changeContentTypeVariationTo)
        {
            var contentType = MockedContentTypes.CreateBasicContentType();
            contentType.Variations = startingContentTypeVariation;
            var properties = CreatePropertyCollection(("title", startingContentTypeVariation));
            contentType.PropertyGroups.Add(new PropertyGroup(properties) { Name = "Content" });
            ServiceContext.ContentTypeService.Save(contentType);

            //create some content of this content type
            IContent doc = MockedContent.CreateBasicContent(contentType);
            doc.SetCultureName("Hello1", "en-US");
            doc.SetValue("title", "hello world", "en-US");
            ServiceContext.ContentService.Save(doc);

            doc = ServiceContext.ContentService.GetById(doc.Id); //re-get
            Assert.AreEqual("Hello1", doc.GetCultureName("en-US"));
            Assert.AreEqual("hello world", doc.GetValue("title", "en-US"));
            Assert.IsTrue(doc.Edited);
            Assert.IsTrue(doc.IsCultureEdited("en-US"));

            //change the content type to be invariant, we will also update the name here to detect the copy changes
            doc.SetCultureName("Hello2", "en-US");
            ServiceContext.ContentService.Save(doc);
            contentType.Variations = changeContentTypeVariationTo;
            ServiceContext.ContentTypeService.Save(contentType);
            doc = ServiceContext.ContentService.GetById(doc.Id); //re-get

            Assert.AreEqual("Hello2", doc.Name);
            Assert.AreEqual("hello world", doc.GetValue("title"));
            Assert.IsTrue(doc.Edited);
            Assert.IsFalse(doc.IsCultureEdited("en-US"));

            //change back property type to be variant, we will also update the name here to detect the copy changes
            doc.Name = "Hello3";
            ServiceContext.ContentService.Save(doc);
            contentType.Variations = startingContentTypeVariation;
            ServiceContext.ContentTypeService.Save(contentType);
            doc = ServiceContext.ContentService.GetById(doc.Id); //re-get

            //at this stage all property types were switched to invariant so even though the variant value
            //exists it will not be returned because the property type is invariant,
            //so this check proves that null will be returned
            Assert.AreEqual("Hello3", doc.Name);
            Assert.IsNull(doc.GetValue("title", "en-US"));
            Assert.IsTrue(doc.Edited);
            Assert.IsTrue(doc.IsCultureEdited("en-US")); // this is true because the name change is copied to the default language

            //we can now switch the property type to be variant and the value can be returned again
            contentType.PropertyTypes.First().Variations = startingContentTypeVariation;
            ServiceContext.ContentTypeService.Save(contentType);
            doc = ServiceContext.ContentService.GetById(doc.Id); //re-get

            Assert.AreEqual("Hello3", doc.GetCultureName("en-US"));
            Assert.AreEqual("hello world", doc.GetValue("title", "en-US"));
            Assert.IsTrue(doc.Edited);
            Assert.IsTrue(doc.IsCultureEdited("en-US"));
        }

        [TestCase(ContentVariation.Nothing, ContentVariation.Nothing)]
        [TestCase(ContentVariation.Nothing, ContentVariation.Culture)]
        [TestCase(ContentVariation.Nothing, ContentVariation.Segment)]
        [TestCase(ContentVariation.Nothing, ContentVariation.CultureAndSegment)]
        [TestCase(ContentVariation.Culture, ContentVariation.Nothing)]
        [TestCase(ContentVariation.Culture, ContentVariation.Culture)]
        [TestCase(ContentVariation.Culture, ContentVariation.Segment)]
        [TestCase(ContentVariation.Culture, ContentVariation.CultureAndSegment)]
        [TestCase(ContentVariation.Segment, ContentVariation.Nothing)]
        [TestCase(ContentVariation.Segment, ContentVariation.Culture)]
        [TestCase(ContentVariation.Segment, ContentVariation.Segment)]
        [TestCase(ContentVariation.Segment, ContentVariation.CultureAndSegment)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Nothing)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Culture)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Segment)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.CultureAndSegment)]
        public void Preserve_Content_Name_After_Content_Type_Variation_Change(ContentVariation contentTypeVariationFrom, ContentVariation contentTypeVariationTo)
        {
            var contentType = MockedContentTypes.CreateBasicContentType();
            contentType.Variations = contentTypeVariationFrom;
            ServiceContext.ContentTypeService.Save(contentType);

            var invariantContentName = "Content Invariant";

            var defaultCultureContentName = "Content en-US";
            var defaultCulture = "en-US";

            var nlContentName = "Content nl-NL";
            var nlCulture = "nl-NL";

            ServiceContext.LocalizationService.Save(new Language(nlCulture));

            var includeCultureNames = contentType.Variations.HasFlag(ContentVariation.Culture);

            // Create some content of this content type
            IContent doc = MockedContent.CreateBasicContent(contentType);

            doc.Name = invariantContentName;
            if (includeCultureNames)
            {
                Assert.DoesNotThrow(() => doc.SetCultureName(defaultCultureContentName, defaultCulture));
                Assert.DoesNotThrow(() => doc.SetCultureName(nlContentName, nlCulture));
            } else
            {
                Assert.Throws<NotSupportedException>(() => doc.SetCultureName(defaultCultureContentName, defaultCulture));
                Assert.Throws<NotSupportedException>(() => doc.SetCultureName(nlContentName, nlCulture));
            }

            ServiceContext.ContentService.Save(doc);
            doc = ServiceContext.ContentService.GetById(doc.Id);

            AssertAll();

            // Change variation
            contentType.Variations = contentTypeVariationTo;
            ServiceContext.ContentService.Save(doc);
            doc = ServiceContext.ContentService.GetById(doc.Id);

            AssertAll();

            void AssertAll()
            {
                if (includeCultureNames)
                {
                    // Invariant content name is not preserved when content type is set to culture
                    Assert.AreEqual(defaultCultureContentName, doc.Name);
                    Assert.AreEqual(doc.Name, doc.GetCultureName(defaultCulture));
                    Assert.AreEqual(nlContentName, doc.GetCultureName(nlCulture));
                }
                else
                {
                    Assert.AreEqual(invariantContentName, doc.Name);
                    Assert.AreEqual(null, doc.GetCultureName(defaultCulture));
                    Assert.AreEqual(null, doc.GetCultureName(nlCulture));
                }
            }
        }

        [TestCase(ContentVariation.Nothing, ContentVariation.Nothing)]
        [TestCase(ContentVariation.Nothing, ContentVariation.Culture)]
        [TestCase(ContentVariation.Nothing, ContentVariation.Segment)]
        [TestCase(ContentVariation.Nothing, ContentVariation.CultureAndSegment)]
        [TestCase(ContentVariation.Culture, ContentVariation.Nothing)]
        [TestCase(ContentVariation.Culture, ContentVariation.Culture)]
        [TestCase(ContentVariation.Culture, ContentVariation.Segment)]
        [TestCase(ContentVariation.Culture, ContentVariation.CultureAndSegment)]
        [TestCase(ContentVariation.Segment, ContentVariation.Nothing)]
        [TestCase(ContentVariation.Segment, ContentVariation.Culture)]
        [TestCase(ContentVariation.Segment, ContentVariation.Segment)]
        [TestCase(ContentVariation.Segment, ContentVariation.CultureAndSegment)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Nothing)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Culture)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Segment)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.CultureAndSegment)]
        public void Verify_If_Property_Type_Variation_Is_Correctly_Corrected_When_Content_Type_Is_Updated(ContentVariation contentTypeVariation, ContentVariation propertyTypeVariation)
        {
            var contentType = MockedContentTypes.CreateBasicContentType();

            // We test an updated content type so it has to be saved first.
            ServiceContext.ContentTypeService.Save(contentType);

            // Update it
            contentType.Variations = contentTypeVariation;
            var properties = CreatePropertyCollection(("title", propertyTypeVariation));
            contentType.PropertyGroups.Add(new PropertyGroup(properties) { Name = "Content" });
            ServiceContext.ContentTypeService.Save(contentType);

            // Check if property type variations have been updated correctly
            Assert.AreEqual(properties.First().Variations, contentTypeVariation & propertyTypeVariation);
        }

        [TestCase(ContentVariation.Nothing, ContentVariation.Culture)]
        [TestCase(ContentVariation.Nothing, ContentVariation.CultureAndSegment)]
        [TestCase(ContentVariation.Segment, ContentVariation.Culture)]
        [TestCase(ContentVariation.Segment, ContentVariation.CultureAndSegment)]
        public void Change_Property_Type_From_Invariant_Variant(ContentVariation invariant, ContentVariation variant)
        {
            var contentType = MockedContentTypes.CreateBasicContentType();
            // content type supports all variations
            contentType.Variations = ContentVariation.Culture | ContentVariation.Segment;
            var properties = CreatePropertyCollection(("title", invariant));
            contentType.PropertyGroups.Add(new PropertyGroup(properties) { Name = "Content" });
            ServiceContext.ContentTypeService.Save(contentType);

            //create some content of this content type
            IContent doc = MockedContent.CreateBasicContent(contentType);
            doc.SetCultureName("Home", "en-US");
            doc.SetValue("title", "hello world");
            ServiceContext.ContentService.Save(doc);

            doc = ServiceContext.ContentService.GetById(doc.Id); //re-get
            Assert.AreEqual("hello world", doc.GetValue("title"));
            Assert.IsTrue(doc.IsCultureEdited("en-US")); //invariant prop changes show up on default lang
            Assert.IsTrue(doc.Edited);
            
            //change the property type to be variant
            contentType.PropertyTypes.First().Variations = variant;
            ServiceContext.ContentTypeService.Save(contentType);
            doc = ServiceContext.ContentService.GetById(doc.Id); //re-get

            Assert.AreEqual("hello world", doc.GetValue("title", "en-US"));
            Assert.IsTrue(doc.IsCultureEdited("en-US"));
            Assert.IsTrue(doc.Edited);

            //change back property type to be invariant
            contentType.PropertyTypes.First().Variations = invariant;
            ServiceContext.ContentTypeService.Save(contentType);
            doc = ServiceContext.ContentService.GetById(doc.Id); //re-get

            Assert.AreEqual("hello world", doc.GetValue("title"));
            Assert.IsTrue(doc.IsCultureEdited("en-US"));  //invariant prop changes show up on default lang
            Assert.IsTrue(doc.Edited);
        }

        [TestCase(ContentVariation.Culture, ContentVariation.Nothing)]
        [TestCase(ContentVariation.Culture, ContentVariation.Segment)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Nothing)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Segment)]
        public void Change_Property_Type_From_Variant_Invariant(ContentVariation variant, ContentVariation invariant)
        {
            //create content type with a property type that varies by culture
            var contentType = MockedContentTypes.CreateBasicContentType();
            // content type supports all variations            
            contentType.Variations = ContentVariation.Culture | ContentVariation.Segment;
            var properties = CreatePropertyCollection(("title", variant));
            contentType.PropertyGroups.Add(new PropertyGroup(properties) { Name = "Content" });
            ServiceContext.ContentTypeService.Save(contentType);

            //create some content of this content type
            IContent doc = MockedContent.CreateBasicContent(contentType);
            doc.SetCultureName("Home", "en-US");
            doc.SetValue("title", "hello world", "en-US");
            ServiceContext.ContentService.Save(doc);

            Assert.AreEqual("hello world", doc.GetValue("title", "en-US"));

            //change the property type to be invariant
            contentType.PropertyTypes.First().Variations = invariant;
            ServiceContext.ContentTypeService.Save(contentType);
            doc = ServiceContext.ContentService.GetById(doc.Id); //re-get

            Assert.AreEqual("hello world", doc.GetValue("title"));

            //change back property type to be variant
            contentType.PropertyTypes.First().Variations = variant;
            ServiceContext.ContentTypeService.Save(contentType);
            doc = ServiceContext.ContentService.GetById(doc.Id); //re-get

            Assert.AreEqual("hello world", doc.GetValue("title", "en-US"));
        }

        [TestCase(ContentVariation.Culture, ContentVariation.Nothing)]
        [TestCase(ContentVariation.Culture, ContentVariation.Segment)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Nothing)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Segment)]
        public void Change_Property_Type_From_Variant_Invariant_On_A_Composition(ContentVariation variant, ContentVariation invariant)
        {
            //create content type with a property type that varies by culture
            var contentType = MockedContentTypes.CreateBasicContentType();
            // content type supports all variations            
            contentType.Variations = ContentVariation.Culture | ContentVariation.Segment;
            var properties = CreatePropertyCollection(("title", variant));
            contentType.PropertyGroups.Add(new PropertyGroup(properties) { Name = "Content" });
            ServiceContext.ContentTypeService.Save(contentType);

            //compose this from the other one
            var contentType2 = MockedContentTypes.CreateBasicContentType("test");
            contentType2.Variations = contentType.Variations;
            contentType2.AddContentType(contentType);
            ServiceContext.ContentTypeService.Save(contentType2);

            //create some content of this content type
            IContent doc = MockedContent.CreateBasicContent(contentType);
            doc.SetCultureName("Home", "en-US");
            doc.SetValue("title", "hello world", "en-US");
            ServiceContext.ContentService.Save(doc);

            IContent doc2 = MockedContent.CreateBasicContent(contentType2);
            doc2.SetCultureName("Home", "en-US");
            doc2.SetValue("title", "hello world", "en-US");
            ServiceContext.ContentService.Save(doc2);

            //change the property type to be invariant
            contentType.PropertyTypes.First().Variations = invariant;
            ServiceContext.ContentTypeService.Save(contentType);
            doc = ServiceContext.ContentService.GetById(doc.Id); //re-get
            doc2 = ServiceContext.ContentService.GetById(doc2.Id); //re-get

            Assert.AreEqual("hello world", doc.GetValue("title"));
            Assert.AreEqual("hello world", doc2.GetValue("title"));

            //change back property type to be variant
            contentType.PropertyTypes.First().Variations = variant;
            ServiceContext.ContentTypeService.Save(contentType);
            doc = ServiceContext.ContentService.GetById(doc.Id); //re-get
            doc2 = ServiceContext.ContentService.GetById(doc2.Id); //re-get

            Assert.AreEqual("hello world", doc.GetValue("title", "en-US"));
            Assert.AreEqual("hello world", doc2.GetValue("title", "en-US"));
        }

        [TestCase(ContentVariation.Culture, ContentVariation.Nothing)]
        [TestCase(ContentVariation.Culture, ContentVariation.Segment)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Nothing)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Segment)]
        public void Change_Content_Type_From_Variant_Invariant_On_A_Composition(ContentVariation variant, ContentVariation invariant)
        {
            //create content type with a property type that varies by culture
            var contentType = MockedContentTypes.CreateBasicContentType();
            contentType.Variations = variant;
            var properties = CreatePropertyCollection(("title", ContentVariation.Culture));
            contentType.PropertyGroups.Add(new PropertyGroup(properties) { Name = "Content" });
            ServiceContext.ContentTypeService.Save(contentType);

            //compose this from the other one
            var contentType2 = MockedContentTypes.CreateBasicContentType("test");
            contentType2.Variations = contentType.Variations;
            contentType2.AddContentType(contentType);
            ServiceContext.ContentTypeService.Save(contentType2);

            //create some content of this content type
            IContent doc = MockedContent.CreateBasicContent(contentType);
            doc.SetCultureName("Home", "en-US");
            doc.SetValue("title", "hello world", "en-US");
            ServiceContext.ContentService.Save(doc);

            IContent doc2 = MockedContent.CreateBasicContent(contentType2);
            doc2.SetCultureName("Home", "en-US");
            doc2.SetValue("title", "hello world", "en-US");
            ServiceContext.ContentService.Save(doc2);

            //change the content type to be invariant
            contentType.Variations = invariant;
            ServiceContext.ContentTypeService.Save(contentType);
            doc = ServiceContext.ContentService.GetById(doc.Id); //re-get
            doc2 = ServiceContext.ContentService.GetById(doc2.Id); //re-get

            Assert.AreEqual("hello world", doc.GetValue("title"));
            Assert.AreEqual("hello world", doc2.GetValue("title"));

            //change back content type to be variant
            contentType.Variations = variant;
            ServiceContext.ContentTypeService.Save(contentType);
            doc = ServiceContext.ContentService.GetById(doc.Id); //re-get
            doc2 = ServiceContext.ContentService.GetById(doc2.Id); //re-get

            //this will be null because the doc type was changed back to variant but it's property types don't get changed back
            Assert.IsNull(doc.GetValue("title", "en-US"));
            Assert.IsNull(doc2.GetValue("title", "en-US"));
        }

        [Test]
        public void Change_Variations_SimpleContentType_VariantToInvariantAndBack()
        {
            // one simple content type, variant, with both variant and invariant properties
            // can change it to invariant and back

            CreateFrenchAndEnglishLangs();

            var contentType = CreateContentType(ContentVariation.Culture);

            var properties = CreatePropertyCollection(
                ("value1", ContentVariation.Culture),
                ("value2", ContentVariation.Nothing));

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

            var contentType = CreateContentType(ContentVariation.Nothing);

            var properties = CreatePropertyCollection(
                ("value1", ContentVariation.Nothing),
                ("value2", ContentVariation.Nothing));

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

            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value1':[{'culture':'','seg':'','val':'v1'}],'value2':[{'culture':'','seg':'','val':'v2'}]},'cultureData':");
        }

        [Test]
        public void Change_Variations_SimpleContentType_VariantPropertyToInvariantAndBack()
        {
            // one simple content type, variant, with both variant and invariant properties
            // can change an invariant property to variant and back

            CreateFrenchAndEnglishLangs();

            var contentType = CreateContentType(ContentVariation.Culture);

            var properties = CreatePropertyCollection(
                ("value1", ContentVariation.Culture),
                ("value2", ContentVariation.Nothing));

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

            Console.WriteLine(GetJson(document.Id));
            AssertJsonStartsWith(document.Id,
                "{'properties':{'value1':[{'culture':'en','seg':'','val':'v1en'},{'culture':'fr','seg':'','val':'v1fr'}],'value2':[{'culture':'en','seg':'','val':'v2'}]},'cultureData':");
        }

        [TestCase(ContentVariation.Culture, ContentVariation.Nothing)]
        [TestCase(ContentVariation.Culture, ContentVariation.Segment)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Nothing)]
        [TestCase(ContentVariation.CultureAndSegment, ContentVariation.Segment)]
        public void Change_Property_Variations_From_Variant_To_Invariant_And_Ensure_Edited_Values_Are_Renormalized(ContentVariation variant, ContentVariation invariant)
        {
            // one simple content type, variant, with both variant and invariant properties
            // can change an invariant property to variant and back

            CreateFrenchAndEnglishLangs();

            var contentType = CreateContentType(ContentVariation.Culture | ContentVariation.Segment);

            var properties = CreatePropertyCollection(("value1", variant));

            contentType.PropertyGroups.Add(new PropertyGroup(properties) { Name = "Content" });
            ServiceContext.ContentTypeService.Save(contentType);

            IContent document = new Content("document", -1, contentType);
            document.SetCultureName("doc1en", "en");
            document.SetCultureName("doc1fr", "fr");
            document.SetValue("value1", "v1en-init", "en");
            document.SetValue("value1", "v1fr-init", "fr");
            ServiceContext.ContentService.SaveAndPublish(document); //all values are published which means the document is not 'edited'

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.IsFalse(document.IsCultureEdited("en"));
            Assert.IsFalse(document.IsCultureEdited("fr"));
            Assert.IsFalse(document.Edited);

            document.SetValue("value1", "v1en", "en"); //change the property culture value, so now this culture will be edited
            document.SetValue("value1", "v1fr", "fr"); //change the property culture value, so now this culture will be edited
            ServiceContext.ContentService.Save(document);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("doc1en", document.Name);
            Assert.AreEqual("doc1en", document.GetCultureName("en"));
            Assert.AreEqual("doc1fr", document.GetCultureName("fr"));
            Assert.AreEqual("v1en", document.GetValue("value1", "en"));
            Assert.AreEqual("v1en-init", document.GetValue("value1", "en", published: true));
            Assert.AreEqual("v1fr", document.GetValue("value1", "fr"));
            Assert.AreEqual("v1fr-init", document.GetValue("value1", "fr", published: true));
            Assert.IsTrue(document.IsCultureEdited("en")); //This will be true because the edited value isn't the same as the published value
            Assert.IsTrue(document.IsCultureEdited("fr")); //This will be true because the edited value isn't the same as the published value
            Assert.IsTrue(document.Edited);

            // switch property type to Invariant
            contentType.PropertyTypes.First(x => x.Alias == "value1").Variations = invariant;
            ServiceContext.ContentTypeService.Save(contentType); //This is going to have to re-normalize the "Edited" flag
            
            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.IsTrue(document.IsCultureEdited("en")); //This will remain true because there is now a pending change for the invariant property data which is flagged under the default lang
            Assert.IsFalse(document.IsCultureEdited("fr")); //This will be false because nothing has changed for this culture and the property no longer reflects variant changes
            Assert.IsTrue(document.Edited);

            //update the invariant value and publish
            document.SetValue("value1", "v1inv"); 
            ServiceContext.ContentService.SaveAndPublish(document);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("doc1en", document.Name);
            Assert.AreEqual("doc1en", document.GetCultureName("en"));
            Assert.AreEqual("doc1fr", document.GetCultureName("fr"));
            Assert.IsNull(document.GetValue("value1", "en")); //The values are there but the business logic returns null
            Assert.IsNull(document.GetValue("value1", "fr")); //The values are there but the business logic returns null
            Assert.IsNull(document.GetValue("value1", "en", published: true)); //The values are there but the business logic returns null
            Assert.IsNull(document.GetValue("value1", "fr", published: true)); //The values are there but the business logic returns null
            Assert.AreEqual("v1inv", document.GetValue("value1"));
            Assert.AreEqual("v1inv", document.GetValue("value1", published: true));
            Assert.IsFalse(document.IsCultureEdited("en")); //This returns false, everything is published
            Assert.IsFalse(document.IsCultureEdited("fr")); //This will be false because nothing has changed for this culture and the property no longer reflects variant changes
            Assert.IsFalse(document.Edited);

            // switch property back to Culture
            contentType.PropertyTypes.First(x => x.Alias == "value1").Variations = variant;
            ServiceContext.ContentTypeService.Save(contentType);
                        
            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("v1inv", document.GetValue("value1", "en")); //The invariant property value gets copied over to the default language
            Assert.AreEqual("v1inv", document.GetValue("value1", "en", published: true));
            Assert.AreEqual("v1fr", document.GetValue("value1", "fr")); //values are still retained
            Assert.AreEqual("v1fr-init", document.GetValue("value1", "fr", published: true)); //values are still retained
            Assert.IsFalse(document.IsCultureEdited("en")); //The invariant published AND edited values are copied over to the default language
            Assert.IsTrue(document.IsCultureEdited("fr"));  //The previously existing french values are there and there is no published value
            Assert.IsTrue(document.Edited); //Will be flagged edited again because the french culture had pending changes

            // publish again
            document.SetValue("value1", "v1en2", "en"); //update the value now that it's variant again
            document.SetValue("value1", "v1fr2", "fr"); //update the value now that it's variant again
            ServiceContext.ContentService.SaveAndPublish(document);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("doc1en", document.Name);
            Assert.AreEqual("doc1en", document.GetCultureName("en"));
            Assert.AreEqual("doc1fr", document.GetCultureName("fr"));
            Assert.AreEqual("v1en2", document.GetValue("value1", "en"));
            Assert.AreEqual("v1fr2", document.GetValue("value1", "fr"));
            Assert.IsNull(document.GetValue("value1")); //The value is there but the business logic returns null
            Assert.IsFalse(document.IsCultureEdited("en")); //This returns false, the variant property value has been published
            Assert.IsFalse(document.IsCultureEdited("fr")); //This returns false, the variant property value has been published
            Assert.IsFalse(document.Edited);
        }

        [TestCase(ContentVariation.Nothing, ContentVariation.Culture)]
        [TestCase(ContentVariation.Nothing, ContentVariation.CultureAndSegment)]
        [TestCase(ContentVariation.Segment, ContentVariation.Culture)]
        [TestCase(ContentVariation.Segment, ContentVariation.CultureAndSegment)]
        public void Change_Property_Variations_From_Invariant_To_Variant_And_Ensure_Edited_Values_Are_Renormalized(ContentVariation invariant, ContentVariation variant)
        {
            // one simple content type, variant, with both variant and invariant properties
            // can change an invariant property to variant and back

            CreateFrenchAndEnglishLangs();

            var contentType = CreateContentType(ContentVariation.Culture | ContentVariation.Segment);

            var properties = CreatePropertyCollection(("value1", invariant));

            contentType.PropertyGroups.Add(new PropertyGroup(properties) { Name = "Content" });
            ServiceContext.ContentTypeService.Save(contentType);

            var document = (IContent)new Content("document", -1, contentType);
            document.SetCultureName("doc1en", "en");
            document.SetCultureName("doc1fr", "fr");
            document.SetValue("value1", "v1en-init");
            ServiceContext.ContentService.SaveAndPublish(document); //all values are published which means the document is not 'edited'

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.IsFalse(document.IsCultureEdited("en"));
            Assert.IsFalse(document.IsCultureEdited("fr"));
            Assert.IsFalse(document.Edited);

            document.SetValue("value1", "v1en"); //change the property value, so now the invariant (default) culture will be edited
            ServiceContext.ContentService.Save(document);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("doc1en", document.Name);
            Assert.AreEqual("doc1en", document.GetCultureName("en"));
            Assert.AreEqual("doc1fr", document.GetCultureName("fr"));
            Assert.AreEqual("v1en", document.GetValue("value1"));
            Assert.AreEqual("v1en-init", document.GetValue("value1", published: true));            
            Assert.IsTrue(document.IsCultureEdited("en")); //This is true because the invariant property reflects changes on the default lang
            Assert.IsFalse(document.IsCultureEdited("fr")); 
            Assert.IsTrue(document.Edited);

            // switch property type to Culture
            contentType.PropertyTypes.First(x => x.Alias == "value1").Variations = variant;
            ServiceContext.ContentTypeService.Save(contentType); //This is going to have to re-normalize the "Edited" flag

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.IsTrue(document.IsCultureEdited("en")); //Remains true 
            Assert.IsFalse(document.IsCultureEdited("fr")); //False because no french property has ever been edited
            Assert.IsTrue(document.Edited);

            //update the culture value and publish
            document.SetValue("value1", "v1en2", "en");
            ServiceContext.ContentService.SaveAndPublish(document);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("doc1en", document.Name);
            Assert.AreEqual("doc1en", document.GetCultureName("en"));
            Assert.AreEqual("doc1fr", document.GetCultureName("fr"));
            Assert.IsNull(document.GetValue("value1")); //The values are there but the business logic returns null
            Assert.IsNull(document.GetValue("value1", published: true)); //The values are there but the business logic returns null
            Assert.AreEqual("v1en2", document.GetValue("value1", "en"));
            Assert.AreEqual("v1en2", document.GetValue("value1", "en", published: true));
            Assert.IsFalse(document.IsCultureEdited("en")); //This returns false, everything is published
            Assert.IsFalse(document.IsCultureEdited("fr")); //False because no french property has ever been edited
            Assert.IsFalse(document.Edited);

            // switch property back to Invariant
            contentType.PropertyTypes.First(x => x.Alias == "value1").Variations = invariant;
            ServiceContext.ContentTypeService.Save(contentType);

            document = ServiceContext.ContentService.GetById(document.Id);
            Assert.AreEqual("v1en2", document.GetValue("value1")); //The variant property value gets copied over to the invariant
            Assert.AreEqual("v1en2", document.GetValue("value1", published: true));
            Assert.IsNull(document.GetValue("value1", "fr"));  //The values are there but the business logic returns null
            Assert.IsNull(document.GetValue("value1", "fr", published: true));  //The values are there but the business logic returns null
            Assert.IsFalse(document.IsCultureEdited("en")); //The variant published AND edited values are copied over to the invariant
            Assert.IsFalse(document.IsCultureEdited("fr"));  
            Assert.IsFalse(document.Edited);

        }

        [Test]
        public void Change_Variations_ComposedContentType_1()
        {
            // one composing content type, variant, with both variant and invariant properties
            // one composed content type, variant, with both variant and invariant properties
            // can change the composing content type to invariant and back
            // can change the composed content type to invariant and back

            CreateFrenchAndEnglishLangs();

            var composing = CreateContentType(ContentVariation.Culture, "composing");

            var properties1 = CreatePropertyCollection(
                ("value11", ContentVariation.Culture),
                ("value12", ContentVariation.Nothing));

            composing.PropertyGroups.Add(new PropertyGroup(properties1) { Name = "Content" });
            ServiceContext.ContentTypeService.Save(composing);

            var composed = CreateContentType(ContentVariation.Culture, "composed");

            var properties2 = CreatePropertyCollection(
                ("value21", ContentVariation.Culture),
                ("value22", ContentVariation.Nothing));

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

            CreateFrenchAndEnglishLangs();

            var composing = CreateContentType(ContentVariation.Culture, "composing");

            var properties1 = CreatePropertyCollection(
                ("value11", ContentVariation.Culture),
                ("value12", ContentVariation.Nothing));

            composing.PropertyGroups.Add(new PropertyGroup(properties1) { Name = "Content" });
            ServiceContext.ContentTypeService.Save(composing);

            var composed1 = CreateContentType(ContentVariation.Culture, "composed1");

            var properties2 = CreatePropertyCollection(
                ("value21", ContentVariation.Culture),
                ("value22", ContentVariation.Nothing));

            composed1.PropertyGroups.Add(new PropertyGroup(properties2) { Name = "Content" });
            composed1.AddContentType(composing);
            ServiceContext.ContentTypeService.Save(composed1);

            var composed2 = CreateContentType(ContentVariation.Nothing, "composed2");

            var properties3 = CreatePropertyCollection(
                ("value31", ContentVariation.Nothing),
                ("value32", ContentVariation.Nothing));

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

        private void CreateFrenchAndEnglishLangs()
        {
            var languageEn = new Language("en") { IsDefault = true };
            ServiceContext.LocalizationService.Save(languageEn);
            var languageFr = new Language("fr");
            ServiceContext.LocalizationService.Save(languageFr);
        }

        private IContentType CreateContentType(ContentVariation variance, string alias = "contentType") => new ContentType(-1)
        {
            Alias = alias,
            Name = alias,
            Variations = variance
        };

        private PropertyTypeCollection CreatePropertyCollection(params (string alias, ContentVariation variance)[] props)
        {
            var propertyCollection = new PropertyTypeCollection(true);

            foreach (var (alias, variance) in props)
                propertyCollection.Add(new PropertyType(alias, ValueStorageType.Ntext)
                {
                    Alias = alias,
                    DataTypeId = -88,
                    Variations = variance
                });

            return propertyCollection;
        }
    }
}
