// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
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

        Assert.AreNotSame(clone, template);
        Assert.AreEqual(clone, template);
        Assert.AreEqual(clone.Path, template.Path);
        Assert.AreEqual(clone.IsMasterTemplate, template.IsMasterTemplate);
        Assert.AreEqual(clone.CreateDate, template.CreateDate);
        Assert.AreEqual(clone.Alias, template.Alias);
        Assert.AreEqual(clone.Id, template.Id);
        Assert.AreEqual(clone.Key, template.Key);
        Assert.AreEqual(clone.MasterTemplateAlias, template.MasterTemplateAlias);
        Assert.AreEqual(clone.MasterTemplateId.Value, ((Template)template).MasterTemplateId.Value);
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
    public void Can_Serialize_Without_Error()
    {
        var template = BuildTemplate();

        var json = JsonConvert.SerializeObject(template);
        Debug.Print(json);
    }

    private ITemplate BuildTemplate() =>
        _builder
            .WithId(3)
            .WithAlias("test")
            .WithName("Test")
            .WithCreateDate(DateTime.Now)
            .WithUpdateDate(DateTime.Now)
            .WithKey(Guid.NewGuid())
            .WithContent("blah")
            .AsMasterTemplate("master", 88)
            .Build();
}
