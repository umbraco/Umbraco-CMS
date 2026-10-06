// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

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

        Assert.AreNotSame(clone, template);
        Assert.AreEqual(clone, template);
        Assert.AreEqual(clone.Path, template.Path);
        Assert.AreEqual(clone.IsLayoutTemplate, template.IsLayoutTemplate);
        Assert.AreEqual(clone.CreateDate, template.CreateDate);
        Assert.AreEqual(clone.Alias, template.Alias);
        Assert.AreEqual(clone.Id, template.Id);
        Assert.AreEqual(clone.Key, template.Key);
        Assert.AreEqual(clone.LayoutTemplateAlias, template.LayoutTemplateAlias);
        Assert.AreEqual(clone.LayoutTemplateId.Value, ((Template)template).LayoutTemplateId.Value);
        Assert.AreEqual(clone.Name, template.Name);
        Assert.AreEqual(clone.UpdateDate, template.UpdateDate);

        // clone.Content should be null but getting it would lazy-load
        var type = clone.GetType();
        var contentField = type.BaseType.GetField("_content", BindingFlags.Instance | BindingFlags.NonPublic);
        var value = contentField.GetValue(clone);
        Assert.IsNull(value);

        // this double verifies by reflection
        // need to exclude content else it would lazy-load
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps.Where(x => x.Name != "Content"))
        {
            Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(template, null));
        }
    }

    [Test]
    public void Content_Is_Empty_For_A_New_Template()
    {
        var template = new Template(TestHelper.ShortStringHelper, "Test", "test");

        Assert.AreEqual(string.Empty, template.Content);
    }

    [Test]
    public void Content_Is_Null_When_Not_Loaded()
    {
        var template = new Template(TestHelper.ShortStringHelper, "Test", "test", contentLoaded: false);

        Assert.IsNull(template.Content);
    }

    [Test]
    public void Content_Is_Read_By_The_Loader_When_Not_Loaded()
    {
        var template = new Template(TestHelper.ShortStringHelper, "Test", "test", contentLoaded: false);

        template.GetFileContent = _ => "loaded content";

        Assert.AreEqual("loaded content", template.Content);
    }

    [Test]
    public void Content_Is_Returned_When_Set_After_Not_Loaded()
    {
        var template = new Template(TestHelper.ShortStringHelper, "Test", "test", contentLoaded: false);

        template.Content = "new content";

        Assert.AreEqual("new content", template.Content);
    }

    [Test]
    public void Content_Not_Loaded_Is_Kept_By_Deep_Clone()
    {
        var template = new Template(TestHelper.ShortStringHelper, "Test", "test", contentLoaded: false);

        var clone = (Template)template.DeepClone();

        Assert.IsNull(clone.Content);
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
