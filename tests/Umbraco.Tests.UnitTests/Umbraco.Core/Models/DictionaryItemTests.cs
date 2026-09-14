// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class DictionaryItemTests
{
    [SetUp]
    public void SetUp() => _builder = new DictionaryItemBuilder();

    private DictionaryItemBuilder _builder = new();

    [Test]
    public void Can_Deep_Clone()
    {
        var item = _builder
            .WithRandomTranslations(2)
            .Build();

        var clone = (DictionaryItem)item.DeepClone();

        Assert.That(item, Is.Not.SameAs(clone));
        Assert.That(item, Is.EqualTo(clone));
        Assert.That(item.CreateDate, Is.EqualTo(clone.CreateDate));
        Assert.That(item.Id, Is.EqualTo(clone.Id));
        Assert.That(item.ItemKey, Is.EqualTo(clone.ItemKey));
        Assert.That(item.Key, Is.EqualTo(clone.Key));
        Assert.That(item.ParentId, Is.EqualTo(clone.ParentId));
        Assert.That(item.UpdateDate, Is.EqualTo(clone.UpdateDate));
        Assert.That(item.Translations.Count(), Is.EqualTo(clone.Translations.Count()));
        for (var i = 0; i < item.Translations.Count(); i++)
        {
            Assert.That(item.Translations.ElementAt(i), Is.Not.SameAs(clone.Translations.ElementAt(i)));
            Assert.That(item.Translations.ElementAt(i), Is.EqualTo(clone.Translations.ElementAt(i)));
        }

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.That(propertyInfo.GetValue(item, null), Is.EqualTo(propertyInfo.GetValue(clone, null)));
        }
    }

    [Test]
    public void Can_Serialize_Without_Error()
    {
        var item = _builder
            .WithRandomTranslations(2)
            .Build();

        Assert.DoesNotThrow(() => JsonSerializer.Serialize(item));
    }

    [TestCase("en-AU", "en-AU value")]
    [TestCase("en-GB", "en-GB value")]
    [TestCase("en-US", "")]
    public void Can_Get_Translated_Value_By_IsoCode(string isoCode, string expectedValue)
    {
        var item = _builder
            .AddTranslation()
            .AddLanguage()
            .WithCultureInfo("en-AU")
            .Done()
            .WithValue("en-AU value")
            .Done()
            .AddTranslation()
            .AddLanguage()
            .WithCultureInfo("en-GB")
            .Done()
            .WithValue("en-GB value")
            .Done()
            .Build();

        var value = item.GetTranslatedValue(isoCode);
        Assert.That(value, Is.EqualTo(expectedValue));
    }
}
