// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class MacroTests
{
    [SetUp]
    public void SetUp() => _builder = new MacroBuilder();

    private MacroBuilder _builder;

    [Test]
    public void Can_Deep_Clone()
    {
        var macro = _builder
            .WithId(1)
            .WithUseInEditor(true)
            .WithCacheDuration(3)
            .WithAlias("test")
            .WithName("Test")
            .WithSource("~/script.cshtml")
            .WithCacheByMember(true)
            .WithDontRender(true)
            .AddProperty()
            .WithId(6)
            .WithAlias("rewq")
            .WithName("REWQ")
            .WithSortOrder(1)
            .WithEditorAlias("asdfasdf")
            .Done()
            .Build();

        var clone = (Macro)macro.DeepClone();

        Assert.AreNotSame(clone, macro);
        Assert.AreEqual(clone, macro);
        Assert.AreEqual(clone.Id, macro.Id);

        Assert.AreEqual(clone.Properties.Count, macro.Properties.Count);

        for (var i = 0; i < clone.Properties.Count; i++)
        {
            Assert.AreEqual(clone.Properties[i], macro.Properties[i]);
            Assert.AreNotSame(clone.Properties[i], macro.Properties[i]);
        }

        Assert.AreNotSame(clone.Properties, macro.Properties);
        Assert.AreNotSame(clone.AddedProperties, macro.AddedProperties);
        Assert.AreNotSame(clone.RemovedProperties, macro.RemovedProperties);

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(macro, null));
        }

        // Need to ensure the event handlers are wired.
        var asDirty = (ICanBeDirty)clone;

        Assert.IsFalse(asDirty.IsPropertyDirty("Properties"));
        clone.Properties.Add(new MacroProperty(3, Guid.NewGuid(), "asdf", "SDF", 3, "asdfasdf"));
        Assert.IsTrue(asDirty.IsPropertyDirty("Properties"));
        Assert.AreEqual(1, clone.AddedProperties.Count());
        clone.Properties.Remove("rewq");
        Assert.AreEqual(1, clone.RemovedProperties.Count());
    }
}
