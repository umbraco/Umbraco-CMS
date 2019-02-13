using System;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class VariationTests
    {
        [SetUp]
        public void SetUp()
        {
            // annoying, but content type wants short string helper ;(
            SettingsForTests.Reset();

            // well, this is also annoying, but...
            // validating a value is performed by its data editor,
            // based upon the configuration in the data type, so we
            // need to be able to retrieve them all...

            Current.Reset();

            var configs = new Configs();
            configs.Add(SettingsForTests.GetDefaultGlobalSettings);
            configs.Add(SettingsForTests.GetDefaultUmbracoSettings);

            var factory = Mock.Of<IFactory>();
            Current.Factory = factory;

            var dataEditors = new DataEditorCollection(new IDataEditor[]
            {
                new DataEditor(Mock.Of<ILogger>()) { Alias = "editor", ExplicitValueEditor = new DataValueEditor("view") }
            });
            var propertyEditors = new PropertyEditorCollection(dataEditors);

            var dataType = Mock.Of<IDataType>();
            Mock.Get(dataType)
                .Setup(x => x.Configuration)
                .Returns(null);

            var dataTypeService = Mock.Of<IDataTypeService>();
            Mock.Get(dataTypeService)
                .Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns<int>(x => dataType);

            var serviceContext = ServiceContext.CreatePartial(
                dataTypeService: dataTypeService,
                localizedTextService: Mock.Of<ILocalizedTextService>());

            Mock.Get(factory)
                .Setup(x => x.GetInstance(It.IsAny<Type>()))
                .Returns<Type>(x =>
                {
                    if (x == typeof(Configs)) return configs;
                    if (x == typeof(PropertyEditorCollection)) return propertyEditors;
                    if (x == typeof(ServiceContext)) return serviceContext;
                    if (x == typeof(ILocalizedTextService)) return serviceContext.LocalizationService;
                    throw new NotSupportedException(x.FullName);
                });
        }

        [Test]
        public void ValidateVariationTests()
        {
            void Assert4A(ContentVariation v, string c, string s, bool xx)
            {
                Assert4B(v, c, s, xx, xx, xx, xx);
            }

            void Assert4B(ContentVariation v, string c, string s, bool ew, bool nn, bool en, bool nw)
            {
                Assert.AreEqual(ew, v.ValidateVariation(c, s, true, true, false));
                Assert.AreEqual(nn, v.ValidateVariation(c, s, false, false, false));
                Assert.AreEqual(en, v.ValidateVariation(c, s, true, false, false));
                Assert.AreEqual(nw, v.ValidateVariation(c, s, false, true, false));
            }

            // always support invariant,neutral
            Assert4A(ContentVariation.Nothing, null, null, true);

            // never support culture and/or segment
            Assert4A(ContentVariation.Nothing, "culture", null, false);
            Assert4A(ContentVariation.Nothing, null, "segment", false);
            Assert4A(ContentVariation.Nothing, "culture", "segment", false);

            // support '*' only when wildcards are supported
            Assert4B(ContentVariation.Nothing, "*", null, true, false, false, true);
            Assert4B(ContentVariation.Nothing, null, "*", true, false, false, true);
            Assert4B(ContentVariation.Nothing, "*", "*", true, false, false, true);


            // support invariant if not exact
            Assert4B(ContentVariation.Culture, null, null, false, true, false, true);

            // support invariant if not exact, '*' when wildcards are supported
            Assert4B(ContentVariation.Culture, "*", null, true, false, false, true);
            Assert4B(ContentVariation.Culture, null, "*", false, false, false, true);
            Assert4B(ContentVariation.Culture, "*", "*", true, false, false, true);

            // never support segment
            Assert4A(ContentVariation.Culture, null, "segment", false);
            Assert4A(ContentVariation.Culture, "culture", "segment", false);
            Assert4A(ContentVariation.Culture, "*", "segment", false);

            Assert4B(ContentVariation.Culture, null, "*", false, false, false, true);
            Assert4B(ContentVariation.Culture, "culture", "*", true, false, false, true);

            // could do the same with .Segment, and .CultureAndSegment
        }

        [Test]
        public void PropertyTests()
        {
            var propertyType = new PropertyType("editor", ValueStorageType.Nvarchar) { Alias = "prop" };
            var prop = new Property(propertyType);

            const string langFr = "fr-FR";

            // can set value
            // and get edited and published value
            // because non-publishing
            prop.SetValue("a");
            Assert.AreEqual("a", prop.GetValue());
            Assert.AreEqual("a", prop.GetValue(published: true));

            // illegal, 'cos non-publishing
            Assert.Throws<NotSupportedException>(() => prop.PublishValues());

            // change
            propertyType.IsPublishing = true;

            // can get value
            // and now published value is null
            Assert.AreEqual("a", prop.GetValue());
            Assert.IsNull(prop.GetValue(published: true));

            // cannot set non-supported variation value
            Assert.Throws<NotSupportedException>(() => prop.SetValue("x", langFr));
            Assert.IsNull(prop.GetValue(langFr));

            // can publish value
            // and get edited and published values
            prop.PublishValues();
            Assert.AreEqual("a", prop.GetValue());
            Assert.AreEqual("a", prop.GetValue(published: true));

            // can set value
            // and get edited and published values
            prop.SetValue("b");
            Assert.AreEqual("b", prop.GetValue());
            Assert.AreEqual("a", prop.GetValue(published: true));

            // can clear value
            prop.UnpublishValues();
            Assert.AreEqual("b", prop.GetValue());
            Assert.IsNull(prop.GetValue(published: true));

            // change - now we vary by culture
            propertyType.Variations |= ContentVariation.Culture;

            // can set value
            // and get values
            prop.SetValue("c", langFr);
            Assert.IsNull(prop.GetValue()); // there is no invariant value anymore
            Assert.IsNull(prop.GetValue(published: true));
            Assert.AreEqual("c", prop.GetValue(langFr));
            Assert.IsNull(prop.GetValue(langFr, published: true));

            // can publish value
            // and get edited and published values
            prop.PublishValues(langFr);
            Assert.IsNull(prop.GetValue());
            Assert.IsNull(prop.GetValue(published: true));
            Assert.AreEqual("c", prop.GetValue(langFr));
            Assert.AreEqual("c", prop.GetValue(langFr, published: true));

            // can clear all
            prop.UnpublishValues("*");
            Assert.IsNull(prop.GetValue());
            Assert.IsNull(prop.GetValue(published: true));
            Assert.AreEqual("c", prop.GetValue(langFr));
            Assert.IsNull(prop.GetValue(langFr, published: true));

            // can publish all
            prop.PublishValues("*");
            Assert.IsNull(prop.GetValue());
            Assert.IsNull(prop.GetValue(published: true));
            Assert.AreEqual("c", prop.GetValue(langFr));
            Assert.AreEqual("c", prop.GetValue(langFr, published: true));

            // same for culture
            prop.UnpublishValues(langFr);
            Assert.AreEqual("c", prop.GetValue(langFr));
            Assert.IsNull(prop.GetValue(langFr, published: true));
            prop.PublishValues(langFr);
            Assert.AreEqual("c", prop.GetValue(langFr));
            Assert.AreEqual("c", prop.GetValue(langFr, published: true));

            prop.UnpublishValues(); // does not throw, internal, content item throws
            Assert.IsNull(prop.GetValue());
            Assert.IsNull(prop.GetValue(published: true));
            prop.PublishValues(); // does not throw, internal, content item throws
            Assert.IsNull(prop.GetValue());
            Assert.IsNull(prop.GetValue(published: true));
        }

        [Test]
        public void ContentNames()
        {
            var contentType = new ContentType(-1) { Alias = "contentType" };
            var content = new Content("content", -1, contentType) { Id = 1, VersionId = 1 };

            const string langFr = "fr-FR";
            const string langUk = "en-UK";

            // throws if the content type does not support the variation
            Assert.Throws<NotSupportedException>(() => content.SetCultureName("name-fr", langFr));

            // now it will work
            contentType.Variations = ContentVariation.Culture;

            // recreate content to re-capture content type variations
            content = new Content("content", -1, contentType) { Id = 1, VersionId = 1 };

            // invariant name works
            content.Name = "name";
            Assert.AreEqual("name", content.GetCultureName(null));
            content.SetCultureName("name2", null);
            Assert.AreEqual("name2", content.Name);
            Assert.AreEqual("name2", content.GetCultureName(null));

            // variant names work
            content.SetCultureName("name-fr", langFr);
            content.SetCultureName("name-uk", langUk);
            Assert.AreEqual("name-fr", content.GetCultureName(langFr));
            Assert.AreEqual("name-uk", content.GetCultureName(langUk));

            // variant dictionary of names work
            Assert.AreEqual(2, content.CultureInfos.Count);
            Assert.IsTrue(content.CultureInfos.ContainsKey(langFr));
            Assert.AreEqual("name-fr", content.CultureInfos[langFr].Name);
            Assert.IsTrue(content.CultureInfos.ContainsKey(langUk));
            Assert.AreEqual("name-uk", content.CultureInfos[langUk].Name);
        }

        [Test]
        public void ContentPublishValues()
        {
            const string langFr = "fr-FR";

            var propertyType = new PropertyType("editor", ValueStorageType.Nvarchar) { Alias = "prop" };
            var contentType = new ContentType(-1) { Alias = "contentType" };
            contentType.AddPropertyType(propertyType);

            var content = new Content("content", -1, contentType) { Id = 1, VersionId = 1 };

            // can set value
            // and get edited value, published is null
            // because publishing
            content.SetValue("prop", "a");
            Assert.AreEqual("a", content.GetValue("prop"));
            Assert.IsNull(content.GetValue("prop", published: true));

            // cannot set non-supported variation value
            Assert.Throws<NotSupportedException>(() => content.SetValue("prop", "x", langFr));
            Assert.IsNull(content.GetValue("prop", langFr));

            // can publish value
            // and get edited and published values
            Assert.IsTrue(content.PublishCulture());
            Assert.AreEqual("a", content.GetValue("prop"));
            Assert.AreEqual("a", content.GetValue("prop", published: true));

            // can set value
            // and get edited and published values
            content.SetValue("prop", "b");
            Assert.AreEqual("b", content.GetValue("prop"));
            Assert.AreEqual("a", content.GetValue("prop", published: true));

            // can clear value
            content.UnpublishCulture();
            Assert.AreEqual("b", content.GetValue("prop"));
            Assert.IsNull(content.GetValue("prop", published: true));

            // change - now we vary by culture
            contentType.Variations |= ContentVariation.Culture;
            propertyType.Variations |= ContentVariation.Culture;
            content.ChangeContentType(contentType);

            // can set value
            // and get values
            content.SetValue("prop", "c", langFr);
            Assert.IsNull(content.GetValue("prop")); // there is no invariant value anymore
            Assert.IsNull(content.GetValue("prop", published: true));
            Assert.AreEqual("c", content.GetValue("prop", langFr));
            Assert.IsNull(content.GetValue("prop", langFr, published: true));

            // can publish value
            // and get edited and published values
            Assert.IsFalse(content.PublishCulture(langFr)); // no name
            content.SetCultureName("name-fr", langFr);
            Assert.IsTrue(content.PublishCulture(langFr));
            Assert.IsNull(content.GetValue("prop"));
            Assert.IsNull(content.GetValue("prop", published: true));
            Assert.AreEqual("c", content.GetValue("prop", langFr));
            Assert.AreEqual("c", content.GetValue("prop", langFr, published: true));

            // can clear all
            content.UnpublishCulture("*");
            Assert.IsNull(content.GetValue("prop"));
            Assert.IsNull(content.GetValue("prop", published: true));
            Assert.AreEqual("c", content.GetValue("prop", langFr));
            Assert.IsNull(content.GetValue("prop", langFr, published: true));

            // can publish all
            Assert.IsTrue(content.PublishCulture("*"));
            Assert.IsNull(content.GetValue("prop"));
            Assert.IsNull(content.GetValue("prop", published: true));
            Assert.AreEqual("c", content.GetValue("prop", langFr));
            Assert.AreEqual("c", content.GetValue("prop", langFr, published: true));

            // same for culture
            content.UnpublishCulture(langFr);
            Assert.AreEqual("c", content.GetValue("prop", langFr));
            Assert.IsNull(content.GetValue("prop", langFr, published: true));
            content.PublishCulture(langFr);
            Assert.AreEqual("c", content.GetValue("prop", langFr));
            Assert.AreEqual("c", content.GetValue("prop", langFr, published: true));

            content.UnpublishCulture(); // clears invariant props if any
            Assert.IsNull(content.GetValue("prop"));
            Assert.IsNull(content.GetValue("prop", published: true));
            content.PublishCulture(); // publishes invariant props if any
            Assert.IsNull(content.GetValue("prop"));
            Assert.IsNull(content.GetValue("prop", published: true));

            var other = new Content("other", -1, contentType) { Id = 2, VersionId = 1 };
            Assert.Throws<NotSupportedException>(() => other.SetValue("prop", "o")); // don't even try
            other.SetValue("prop", "o1", langFr);

            // can copy other's edited value
            content.CopyFrom(other);
            Assert.IsNull(content.GetValue("prop"));
            Assert.IsNull(content.GetValue("prop", published: true));
            Assert.AreEqual("o1", content.GetValue("prop", langFr));
            Assert.AreEqual("c", content.GetValue("prop", langFr, published: true));

            // can copy self's published value
            content.CopyFrom(content);
            Assert.IsNull(content.GetValue("prop"));
            Assert.IsNull(content.GetValue("prop", published: true));
            Assert.AreEqual("c", content.GetValue("prop", langFr));
            Assert.AreEqual("c", content.GetValue("prop", langFr, published: true));
        }

        [Test]
        public void ContentPublishValuesWithMixedPropertyTypeVariations()
        {
            const string langFr = "fr-FR";

            // content type varies by Culture
            // prop1 varies by Culture
            // prop2 is invariant

            var contentType = new ContentType(-1) { Alias = "contentType" };
            contentType.Variations |= ContentVariation.Culture;

            var variantPropType = new PropertyType("editor", ValueStorageType.Nvarchar) { Alias = "prop1", Variations = ContentVariation.Culture, Mandatory = true };
            var invariantPropType = new PropertyType("editor", ValueStorageType.Nvarchar) { Alias = "prop2", Variations = ContentVariation.Nothing, Mandatory = true};

            contentType.AddPropertyType(variantPropType);
            contentType.AddPropertyType(invariantPropType);

            var content = new Content("content", -1, contentType) { Id = 1, VersionId = 1 };

            content.SetCultureName("hello", langFr);

            Assert.IsFalse(content.PublishCulture(langFr)); // fails because prop1 is mandatory
            content.SetValue("prop1", "a", langFr);
            Assert.IsFalse(content.PublishCulture(langFr)); // fails because prop2 is mandatory and invariant
            content.SetValue("prop2", "x");
            Assert.IsTrue(content.PublishCulture(langFr)); // now it's ok

            Assert.AreEqual("a", content.GetValue("prop1", langFr, published: true));
            Assert.AreEqual("x", content.GetValue("prop2", published: true));
        }

        [Test]
        public void ContentPublishVariations()
        {
            const string langFr = "fr-FR";
            const string langUk = "en-UK";
            const string langEs = "es-ES";

            var propertyType = new PropertyType("editor", ValueStorageType.Nvarchar) { Alias = "prop" };
            var contentType = new ContentType(-1) { Alias = "contentType" };
            contentType.AddPropertyType(propertyType);

            var content = new Content("content", -1, contentType) { Id = 1, VersionId = 1 };

            // change - now we vary by culture
            contentType.Variations |= ContentVariation.Culture;
            propertyType.Variations |= ContentVariation.Culture;

            content.ChangeContentType(contentType);

            Assert.Throws<NotSupportedException>(() => content.SetValue("prop", "a")); // invariant = no
            content.SetValue("prop", "a-fr", langFr);
            content.SetValue("prop", "a-uk", langUk);
            content.SetValue("prop", "a-es", langEs);

            // cannot publish without a name
            Assert.IsFalse(content.PublishCulture(langFr));

            // works with a name
            // and then FR is available, and published
            content.SetCultureName("name-fr", langFr);
            Assert.IsTrue(content.PublishCulture(langFr));

            // now UK is available too
            content.SetCultureName("name-uk", langUk);

            // test available, published
            Assert.IsTrue(content.IsCultureAvailable(langFr));
            Assert.IsTrue(content.IsCulturePublished(langFr));
            Assert.AreEqual("name-fr", content.GetPublishName(langFr));
            Assert.AreNotEqual(DateTime.MinValue, content.GetPublishDate(langFr));
            Assert.IsFalse(content.IsCultureEdited(langFr)); // once published, edited is *wrong* until saved

            Assert.IsTrue(content.IsCultureAvailable(langUk));
            Assert.IsFalse(content.IsCulturePublished(langUk));
            Assert.IsNull(content.GetPublishName(langUk));
            Assert.IsNull(content.GetPublishDate(langUk)); // not published

            Assert.IsFalse(content.IsCultureAvailable(langEs));
            Assert.IsFalse(content.IsCultureEdited(langEs)); // not avail, so... not edited
            Assert.IsFalse(content.IsCulturePublished(langEs));

            // not published!
            Assert.IsNull(content.GetPublishName(langEs));
            Assert.IsNull(content.GetPublishDate(langEs));

            // cannot test IsCultureEdited here - as that requires the content service and repository
            // see: ContentServiceTests.Can_SaveRead_Variations
        }

        [Test]
        public void IsDirtyTests()
        {
            var propertyType = new PropertyType("editor", ValueStorageType.Nvarchar) { Alias = "prop" };
            var prop = new Property(propertyType);
            var contentType = new ContentType(-1) { Alias = "contentType" };
            contentType.AddPropertyType(propertyType);

            var content = new Content("content", -1, contentType) { Id = 1, VersionId = 1 };

            prop.SetValue("a");
            Assert.AreEqual("a", prop.GetValue());
            Assert.IsNull(prop.GetValue(published: true));

            Assert.IsTrue(prop.IsDirty());

            content.SetValue("prop", "a");
            Assert.AreEqual("a", content.GetValue("prop"));
            Assert.IsNull(content.GetValue("prop", published: true));

            Assert.IsTrue(content.IsDirty());
            Assert.IsTrue(content.IsAnyUserPropertyDirty());
            // how can we tell which variation was dirty?
        }

        [Test]
        public void ValidationTests()
        {
            var propertyType = new PropertyType("editor", ValueStorageType.Nvarchar) { Alias = "prop", IsPublishing = true };
            var prop = new Property(propertyType);

            prop.SetValue("a");
            Assert.AreEqual("a", prop.GetValue());
            Assert.IsNull(prop.GetValue(published: true));

            Assert.IsTrue(prop.IsValid());

            propertyType.Mandatory = true;
            Assert.IsTrue(prop.IsValid());

            prop.SetValue(null);
            Assert.IsFalse(prop.IsValid());

            // can publish, even though invalid
            prop.PublishValues();
        }
    }
}
