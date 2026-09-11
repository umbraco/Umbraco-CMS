// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class LanguageTests
{
    [SetUp]
    public void SetUp() => _builder = new LanguageBuilder();

    private LanguageBuilder _builder = new();

    [Test]
    public void Can_Deep_Clone()
    {
        var item = _builder
            .WithId(1)
            .Build();

        var clone = (Language)item.DeepClone();
        Assert.That(item, Is.Not.SameAs(clone));
        Assert.That(item, Is.EqualTo(clone));
        Assert.That(item.CreateDate, Is.EqualTo(clone.CreateDate));
        Assert.That(item.CultureName, Is.EqualTo(clone.CultureName));
        Assert.That(item.Id, Is.EqualTo(clone.Id));
        Assert.That(item.IsoCode, Is.EqualTo(clone.IsoCode));
        Assert.That(item.Key, Is.EqualTo(clone.Key));
        Assert.That(item.UpdateDate, Is.EqualTo(clone.UpdateDate));

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
        var item = _builder.Build();

        Assert.DoesNotThrow(() => JsonSerializer.Serialize(item, new JsonSerializerOptions()
        {
            // Ignore CultureInfo reference
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        }));
    }
}
