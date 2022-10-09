using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PublishedCache;

[TestFixture]
public class PublishedSnapshotServiceContentTests : PublishedSnapshotServiceTestBase
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();

        _propertyType =
            new PropertyType(TestHelper.ShortStringHelper, "Umbraco.Void.Editor", ValueStorageType.Nvarchar)
            {
                Alias = "prop",
                DataTypeId = 3,
                Variations = ContentVariation.Culture,
            };
        _contentType =
            new ContentType(TestHelper.ShortStringHelper, -1)
            {
                Id = 2,
                Alias = "alias-ct",
                Variations = ContentVariation.Culture,
            };
        _contentType.AddPropertyType(_propertyType);

        var contentTypes = new[] { _contentType };

        InitializedCache(new[] { CreateKit() }, contentTypes);
    }

    private ContentType _contentType;
    private PropertyType _propertyType;

    private ContentNodeKit CreateKit()
    {
        var draftData = new ContentDataBuilder()
            .WithName("It Works2!")
            .WithPublished(false)
            .WithProperties(new Dictionary<string, PropertyData[]>
            {
                ["prop"] = new[]
                {
                    new PropertyData { Culture = string.Empty, Segment = string.Empty, Value = "val2" },
                    new PropertyData { Culture = "fr-FR", Segment = string.Empty, Value = "val-fr2" },
                    new PropertyData { Culture = "en-UK", Segment = string.Empty, Value = "val-uk2" },
                    new PropertyData { Culture = "dk-DA", Segment = string.Empty, Value = "val-da2" },
                    new PropertyData { Culture = "de-DE", Segment = string.Empty, Value = "val-de2" },
                },
            })
            .WithCultureInfos(new Dictionary<string, CultureVariation>
            {
                // draft data = everything, and IsDraft indicates what's edited
                ["fr-FR"] = new() { Name = "name-fr2", IsDraft = true, Date = new DateTime(2018, 01, 03, 01, 00, 00) },
                ["en-UK"] = new() { Name = "name-uk2", IsDraft = true, Date = new DateTime(2018, 01, 04, 01, 00, 00) },
                ["dk-DA"] = new() { Name = "name-da2", IsDraft = true, Date = new DateTime(2018, 01, 05, 01, 00, 00) },
                ["de-DE"] = new() { Name = "name-de1", IsDraft = false, Date = new DateTime(2018, 01, 02, 01, 00, 00) },
            })
            .Build();

        var publishedData = new ContentDataBuilder()
            .WithName("It Works1!")
            .WithPublished(true)
            .WithProperties(new Dictionary<string, PropertyData[]>
            {
                ["prop"] = new[]
                {
                    new PropertyData { Culture = string.Empty, Segment = string.Empty, Value = "val1" },
                    new PropertyData { Culture = "fr-FR", Segment = string.Empty, Value = "val-fr1" },
                    new PropertyData { Culture = "en-UK", Segment = string.Empty, Value = "val-uk1" },
                },
            })
            .WithCultureInfos(new Dictionary<string, CultureVariation>
            {
                // published data = only what's actually published, and IsDraft has to be false
                ["fr-FR"] = new() { Name = "name-fr1", IsDraft = false, Date = new DateTime(2018, 01, 01, 01, 00, 00) },
                ["en-UK"] = new() { Name = "name-uk1", IsDraft = false, Date = new DateTime(2018, 01, 02, 01, 00, 00) },
                ["de-DE"] = new() { Name = "name-de1", IsDraft = false, Date = new DateTime(2018, 01, 02, 01, 00, 00) },
            })
            .Build();

        var kit = ContentNodeKitBuilder.CreateWithContent(
            2,
            1,
            "-1,1",
            0,
            draftData: draftData,
            publishedData: publishedData);

        return kit;
    }

    [Test]
    public void Verifies_Variant_Data()
    {
        // this test implements a full standalone NuCache (based upon a test IDataSource, does not
        // use any local db files, does not rely on any database) - and tests variations

        // get a snapshot, get a published content
        var snapshot = GetPublishedSnapshot();
        var publishedContent = snapshot.Content.GetById(1);

        Assert.IsNotNull(publishedContent);
        Assert.AreEqual("val1", publishedContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop"));
        Assert.AreEqual("val-fr1", publishedContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop", "fr-FR"));
        Assert.AreEqual("val-uk1", publishedContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop", "en-UK"));

        Assert.IsNull(publishedContent.Name(VariationContextAccessor)); // no invariant name for varying content
        Assert.AreEqual("name-fr1", publishedContent.Name(VariationContextAccessor, "fr-FR"));
        Assert.AreEqual("name-uk1", publishedContent.Name(VariationContextAccessor, "en-UK"));

        var draftContent = snapshot.Content.GetById(true, 1);
        Assert.AreEqual("val2", draftContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop"));
        Assert.AreEqual("val-fr2", draftContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop", "fr-FR"));
        Assert.AreEqual("val-uk2", draftContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop", "en-UK"));

        Assert.IsNull(draftContent.Name(VariationContextAccessor)); // no invariant name for varying content
        Assert.AreEqual("name-fr2", draftContent.Name(VariationContextAccessor, "fr-FR"));
        Assert.AreEqual("name-uk2", draftContent.Name(VariationContextAccessor, "en-UK"));

        // now french is default
        VariationContextAccessor.VariationContext = new VariationContext("fr-FR");
        Assert.AreEqual("val-fr1", publishedContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop"));
        Assert.AreEqual("name-fr1", publishedContent.Name(VariationContextAccessor));
        Assert.AreEqual(new DateTime(2018, 01, 01, 01, 00, 00), publishedContent.CultureDate(VariationContextAccessor));

        // now uk is default
        VariationContextAccessor.VariationContext = new VariationContext("en-UK");
        Assert.AreEqual("val-uk1", publishedContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop"));
        Assert.AreEqual("name-uk1", publishedContent.Name(VariationContextAccessor));
        Assert.AreEqual(new DateTime(2018, 01, 02, 01, 00, 00), publishedContent.CultureDate(VariationContextAccessor));

        // invariant needs to be retrieved explicitly, when it's not default
        Assert.AreEqual("val1", publishedContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop", string.Empty));

        // but,
        // if the content type / property type does not vary, then it's all invariant again
        // modify the content type and property type, notify the snapshot service
        _contentType.Variations = ContentVariation.Nothing;
        _propertyType.Variations = ContentVariation.Nothing;
        SnapshotService.Notify(new[]
        {
            new ContentTypeCacheRefresher.JsonPayload("IContentType", publishedContent.ContentType.Id, ContentTypeChangeTypes.RefreshMain),
        });

        // get a new snapshot (nothing changed in the old one), get the published content again
        var anotherSnapshot = SnapshotService.CreatePublishedSnapshot(null);
        var againContent = anotherSnapshot.Content.GetById(1);

        Assert.AreEqual(ContentVariation.Nothing, againContent.ContentType.Variations);
        Assert.AreEqual(ContentVariation.Nothing, againContent.ContentType.GetPropertyType("prop").Variations);

        // now, "no culture" means "invariant"
        Assert.AreEqual("It Works1!", againContent.Name(VariationContextAccessor));
        Assert.AreEqual("val1", againContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop"));
    }

    [Test]
    public void Verifies_Published_And_Draft_Content()
    {
        // get the published published content
        var snapshot = GetPublishedSnapshot();
        var c1 = snapshot.Content.GetById(1);

        // published content = nothing is draft here
        Assert.IsFalse(c1.IsDraft("fr-FR"));
        Assert.IsFalse(c1.IsDraft("en-UK"));
        Assert.IsFalse(c1.IsDraft("dk-DA"));
        Assert.IsFalse(c1.IsDraft("de-DE"));

        // and only those with published name, are published
        Assert.IsTrue(c1.IsPublished("fr-FR"));
        Assert.IsTrue(c1.IsPublished("en-UK"));
        Assert.IsFalse(c1.IsDraft("dk-DA"));
        Assert.IsTrue(c1.IsPublished("de-DE"));

        // get the draft published content
        var c2 = snapshot.Content.GetById(true, 1);

        // draft content = we have drafts
        Assert.IsTrue(c2.IsDraft("fr-FR"));
        Assert.IsTrue(c2.IsDraft("en-UK"));
        Assert.IsTrue(c2.IsDraft("dk-DA"));
        Assert.IsFalse(c2.IsDraft("de-DE")); // except for the one that does not

        // and only those with published name, are published
        Assert.IsTrue(c2.IsPublished("fr-FR"));
        Assert.IsTrue(c2.IsPublished("en-UK"));
        Assert.IsFalse(c2.IsPublished("dk-DA"));
        Assert.IsTrue(c2.IsPublished("de-DE"));
    }
}
