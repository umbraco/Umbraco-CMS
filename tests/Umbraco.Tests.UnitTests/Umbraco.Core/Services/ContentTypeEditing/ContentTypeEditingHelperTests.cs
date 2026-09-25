// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services.ContentTypeEditing;

[TestFixture]
public class ContentTypeEditingHelperTests
{
    private static readonly IShortStringHelper ShortStringHelper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

    [Test]
    public void GetPropertyAliasesReservedByDescendants_Excludes_Sources_Own_Composition_Contribution()
    {
        var composition = ContentTypeBuilder.CreateBasicContentType("composition", "Composition");
        composition.Id = 1;
        AddPropertyType(composition, "shared");

        var parent = ContentTypeBuilder.CreateBasicContentType("parent", "Parent");
        parent.Id = 2;
        parent.AddContentType(composition);

        var child = ContentTypeBuilder.CreateBasicContentType("child", "Child", parent);
        child.Id = 3;

        var descendantAliases = parent.GetPropertyAliasesReservedByDescendants(new IContentTypeComposition[] { composition, parent, child });

        CollectionAssert.IsEmpty(descendantAliases);
    }

    [Test]
    public void GetPropertyAliasesReservedByDescendants_Excludes_Sources_Own_Property()
    {
        var parent = ContentTypeBuilder.CreateBasicContentType("parent", "Parent");
        parent.Id = 1;
        AddPropertyType(parent, "own1");

        var child = ContentTypeBuilder.CreateBasicContentType("child", "Child", parent);
        child.Id = 2;

        var descendantAliases = parent.GetPropertyAliasesReservedByDescendants(new IContentTypeComposition[] { parent, child });

        CollectionAssert.IsEmpty(descendantAliases);
    }

    [Test]
    public void GetPropertyAliasesReservedByDescendants_Includes_Genuinely_Separate_Descendant_Alias()
    {
        var parent = ContentTypeBuilder.CreateBasicContentType("parent", "Parent");
        parent.Id = 1;

        var child = ContentTypeBuilder.CreateBasicContentType("child", "Child", parent);
        child.Id = 2;
        AddPropertyType(child, "childOwn");

        var descendantAliases = parent.GetPropertyAliasesReservedByDescendants(new IContentTypeComposition[] { parent, child });

        CollectionAssert.AreEquivalent(new[] { "childOwn" }, descendantAliases);
    }

    [Test]
    public void GetPropertyAliasesReservedByDescendants_Walks_Multiple_Inheritance_Levels()
    {
        var parent = ContentTypeBuilder.CreateBasicContentType("parent", "Parent");
        parent.Id = 1;

        var child = ContentTypeBuilder.CreateBasicContentType("child", "Child", parent);
        child.Id = 2;

        var grandchild = ContentTypeBuilder.CreateBasicContentType("grandchild", "Grandchild", child);
        grandchild.Id = 3;
        AddPropertyType(grandchild, "deep");

        var descendantAliases = parent.GetPropertyAliasesReservedByDescendants(new IContentTypeComposition[] { parent, child, grandchild });

        CollectionAssert.AreEquivalent(new[] { "deep" }, descendantAliases);
    }

    private static void AddPropertyType(ContentType contentType, string alias)
    {
        var propertyTypeCollection = new PropertyTypeCollection(true)
        {
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext)
            {
                Alias = alias,
                Name = alias,
                DataTypeId = -88,
            },
        };
        contentType.PropertyGroups.Add(new PropertyGroup(propertyTypeCollection) { Alias = alias, Name = alias, SortOrder = 1 });
    }
}
