// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class TemplateTests
{
    [SetUp]
    public void SetUp() => _builder = new TemplateBuilder();

    private TemplateBuilder _builder;

    [Test]
    public void Can_Deep_Clone()
    {
        var template = BuildTemplate();

        var clone = (Template)template.DeepClone();

        Assert.That(template, Is.Not.SameAs(clone));
        Assert.That(template, Is.EqualTo(clone));
        Assert.That(template.Path, Is.EqualTo(clone.Path));
        Assert.That(template.IsLayoutTemplate, Is.EqualTo(clone.IsLayoutTemplate));
        Assert.That(template.CreateDate, Is.EqualTo(clone.CreateDate));
        Assert.That(template.Alias, Is.EqualTo(clone.Alias));
        Assert.That(template.Id, Is.EqualTo(clone.Id));
        Assert.That(template.Key, Is.EqualTo(clone.Key));
        Assert.That(template.LayoutTemplateAlias, Is.EqualTo(clone.LayoutTemplateAlias));
        Assert.That(((Template)template).LayoutTemplateId.Value, Is.EqualTo(clone.LayoutTemplateId.Value));
        Assert.That(template.Name, Is.EqualTo(clone.Name));
        Assert.That(template.UpdateDate, Is.EqualTo(clone.UpdateDate));

        // clone.Content should be null but getting it would lazy-load
        var type = clone.GetType();
        var contentField = type.BaseType.GetField("_content", BindingFlags.Instance | BindingFlags.NonPublic);
        var value = contentField.GetValue(clone);
        Assert.That(value, Is.Null);

        // this double verifies by reflection
        // need to exclude content else it would lazy-load
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps.Where(x => x.Name != "Content"))
        {
            Assert.That(propertyInfo.GetValue(template, null), Is.EqualTo(propertyInfo.GetValue(clone, null)));
        }
    }

    [Test]
    public void Can_Serialize_Without_Error()
    {
        var template = BuildTemplate();

        var json = JsonSerializer.Serialize(template);
        Debug.Print(json);
    }

    private ITemplate BuildTemplate() =>
        _builder
            .WithId(3)
            .WithAlias("test")
            .WithName("Test")
            .WithCreateDate(DateTime.UtcNow)
            .WithUpdateDate(DateTime.UtcNow)
            .WithKey(Guid.NewGuid())
            .WithContent("blah")
            .AsLayoutTemplate("master", 88)
            .Build();
}
