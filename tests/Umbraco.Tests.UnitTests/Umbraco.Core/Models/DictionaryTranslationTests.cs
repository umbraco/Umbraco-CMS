// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class DictionaryTranslationTests
{
    [SetUp]
    public void SetUp() => _builder = new DictionaryTranslationBuilder();

    private DictionaryTranslationBuilder _builder = new();

    [Test]
    public void Can_Deep_Clone()
    {
        var item = BuildDictionaryTranslation();

        var clone = (DictionaryTranslation)item.DeepClone();

        Assert.That(item, Is.Not.SameAs(clone));
        Assert.That(item, Is.EqualTo(clone));
        Assert.That(item.CreateDate, Is.EqualTo(clone.CreateDate));
        Assert.That(item.Id, Is.EqualTo(clone.Id));
        Assert.That(item.Key, Is.EqualTo(clone.Key));
        Assert.That(item.UpdateDate, Is.EqualTo(clone.UpdateDate));
        Assert.That(item.LanguageIsoCode, Is.EqualTo(clone.LanguageIsoCode));
        Assert.That(item.Value, Is.EqualTo(clone.Value));

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps.Where(x => x.Name != "Language"))
        {
            Assert.That(propertyInfo.GetValue(item, null), Is.EqualTo(propertyInfo.GetValue(clone, null)));
        }
    }

    [Test]
    public void Can_Serialize_Without_Error()
    {
        var item = BuildDictionaryTranslation();

        var json = JsonSerializer.Serialize(item);
        Debug.Print(json);
    }

    private IDictionaryTranslation BuildDictionaryTranslation() =>
        _builder
            .AddLanguage()
            .WithCultureInfo("en-AU")
            .Done()
            .WithValue("colour")
            .Build();
}
