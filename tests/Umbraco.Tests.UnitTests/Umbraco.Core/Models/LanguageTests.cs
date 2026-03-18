// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

/// <summary>
/// Contains unit tests for the <see cref="Language"/> model in <c>Umbraco.Core.Models</c>.
/// </summary>
[TestFixture]
public class LanguageTests
{
    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public void SetUp() => _builder = new LanguageBuilder();

    private LanguageBuilder _builder = new();

    /// <summary>
    /// Tests that a Language object can be deep cloned correctly.
    /// </summary>
    [Test]
    public void Can_Deep_Clone()
    {
        var item = _builder
            .WithId(1)
            .Build();

        var clone = (Language)item.DeepClone();
        Assert.AreNotSame(clone, item);
        Assert.AreEqual(clone, item);
        Assert.AreEqual(clone.CreateDate, item.CreateDate);
        Assert.AreEqual(clone.CultureName, item.CultureName);
        Assert.AreEqual(clone.Id, item.Id);
        Assert.AreEqual(clone.IsoCode, item.IsoCode);
        Assert.AreEqual(clone.Key, item.Key);
        Assert.AreEqual(clone.UpdateDate, item.UpdateDate);

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(item, null));
        }
    }

    /// <summary>
    /// Tests that the language object can be serialized without throwing an error.
    /// </summary>
    [Test]
    public void Can_Serialize_Without_Error()
    {
        var item = _builder.Build();

        Assert.DoesNotThrow(() => JsonSerializer.Serialize(item, new JsonSerializerOptions()
        {
            // Ignore CultureInfo reference
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        }));
    }
}
