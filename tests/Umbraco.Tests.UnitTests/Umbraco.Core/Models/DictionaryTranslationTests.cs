// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

/// <summary>
/// Contains unit tests for the <see cref="Umbraco.Core.Models.DictionaryTranslation"/> class.
/// </summary>
[TestFixture]
public class DictionaryTranslationTests
{
    /// <summary>
    /// Sets up the test environment before each test is run.
    /// </summary>
    [SetUp]
    public void SetUp() => _builder = new DictionaryTranslationBuilder();

    private DictionaryTranslationBuilder _builder = new();

    /// <summary>
    /// Tests that the DictionaryTranslation object can be deeply cloned correctly.
    /// </summary>
    [Test]
    public void Can_Deep_Clone()
    {
        var item = BuildDictionaryTranslation();

        var clone = (DictionaryTranslation)item.DeepClone();

        Assert.AreNotSame(clone, item);
        Assert.AreEqual(clone, item);
        Assert.AreEqual(clone.CreateDate, item.CreateDate);
        Assert.AreEqual(clone.Id, item.Id);
        Assert.AreEqual(clone.Key, item.Key);
        Assert.AreEqual(clone.UpdateDate, item.UpdateDate);
        Assert.AreEqual(clone.LanguageIsoCode, item.LanguageIsoCode);
        Assert.AreEqual(clone.Value, item.Value);

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps.Where(x => x.Name != "Language"))
        {
            Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(item, null));
        }
    }

    /// <summary>
    /// Tests that a DictionaryTranslation object can be serialized to JSON without throwing an error.
    /// </summary>
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
