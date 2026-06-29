// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class ElementMapperTest
{
    [Test]
    public void Can_Map_Id_Property()
    {
        var column = new ElementMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(Element.Id));
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Node}].[id]"));
    }

    [Test]
    public void Can_Map_Trashed_Property()
    {
        var column =
            new ElementMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(Element.Trashed));
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Node}].[trashed]"));
    }

    [Test]
    public void Can_Map_Published_Property()
    {
        var column =
            new ElementMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(Element.Published));
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Element}].[published]"));
    }

    [Test]
    public void Can_Map_Version_Property()
    {
        var column =
            new ElementMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(Element.VersionId));
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.ContentVersion}].[id]"));
    }
}
