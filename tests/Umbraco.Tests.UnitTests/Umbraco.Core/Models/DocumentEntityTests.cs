// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class DocumentEntityTests
{
    [SetUp]
    public void SetUp() => _builder = new DocumentEntitySlimBuilder();

    private DocumentEntitySlimBuilder _builder;

    [Test]
    public void Can_Serialize_Without_Error()
    {
        var item = _builder
            .WithId(3)
            .WithCreatorId(4)
            .WithName("Test")
            .WithParentId(5)
            .WithSortOrder(6)
            .WithLevel(7)
            .WithContentTypeAlias("test1")
            .WithContentTypeIcon("icon")
            .WithContentTypeThumbnail("thumb")
            .WithHasChildren(true)
            .WithPublished(true)
            .Build();

        var json = JsonSerializer.Serialize(item);
        Debug.Print(json);
    }
}
