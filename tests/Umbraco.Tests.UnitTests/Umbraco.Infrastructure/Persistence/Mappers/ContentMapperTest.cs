// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class ContentMapperTest
{
    [Test]
    public void Can_Map_Id_Property()
    {
        var column = new ContentMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(Content.Id));
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Node}].[id]"));
    }

    [Test]
    public void Can_Map_Trashed_Property()
    {
        var column =
            new ContentMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(Content.Trashed));
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Node}].[trashed]"));
    }

    [Test]
    public void Can_Map_Published_Property()
    {
        var column =
            new ContentMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(Content.Published));
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Document}].[published]"));
    }

    [Test]
    public void Can_Map_Version_Property()
    {
        var column =
            new ContentMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(Content.VersionId));
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.ContentVersion}].[id]"));
    }
}
