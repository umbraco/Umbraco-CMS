// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class MacroBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        const int id = 1;
        var key = Guid.NewGuid();
        const bool useInEditor = true;
        const int cacheDuration = 3;
        const string alias = "test";
        const string name = "Test";
        const string source = "~/script.cshtml";
        const bool cacheByPage = false;
        const bool cacheByMember = true;
        const bool dontRender = true;
        const int propertyId = 6;
        const string propertyAlias = "rewq";
        const string propertyName = "REWQ";
        const int propertySortOrder = 1;
        const string propertyEditorAlias = "asdfasdf";

        var builder = new MacroBuilder();

        // Act
        var macro = builder
            .WithId(id)
            .WithKey(key)
            .WithUseInEditor(useInEditor)
            .WithCacheDuration(cacheDuration)
            .WithAlias(alias)
            .WithName(name)
            .WithSource(source)
            .WithCacheByPage(cacheByPage)
            .WithCacheByMember(cacheByMember)
            .WithDontRender(dontRender)
            .AddProperty()
            .WithId(propertyId)
            .WithAlias(propertyAlias)
            .WithName(propertyName)
            .WithSortOrder(propertySortOrder)
            .WithEditorAlias(propertyEditorAlias)
            .Done()
            .Build();

        // Assert
        Assert.AreEqual(id, macro.Id);
        Assert.AreEqual(key, macro.Key);
        Assert.AreEqual(useInEditor, macro.UseInEditor);
        Assert.AreEqual(cacheDuration, macro.CacheDuration);
        Assert.AreEqual(alias, macro.Alias);
        Assert.AreEqual(name, macro.Name);
        Assert.AreEqual(source, macro.MacroSource);
        Assert.AreEqual(cacheByPage, macro.CacheByPage);
        Assert.AreEqual(cacheByMember, macro.CacheByMember);
        Assert.AreEqual(dontRender, macro.DontRender);
        Assert.AreEqual(1, macro.Properties.Count);
        Assert.AreEqual(propertyId, macro.Properties[0].Id);
        Assert.AreEqual(propertyAlias, macro.Properties[0].Alias);
        Assert.AreEqual(propertyName, macro.Properties[0].Name);
        Assert.AreEqual(propertySortOrder, macro.Properties[0].SortOrder);
        Assert.AreEqual(propertyEditorAlias, macro.Properties[0].EditorAlias);
    }
}
