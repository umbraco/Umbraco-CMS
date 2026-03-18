// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

/// <summary>
/// Contains unit tests for the <see cref="ContentMapper"/> class within the Umbraco CMS persistence infrastructure.
/// These tests verify the correct mapping behavior for content entities.
/// </summary>
[TestFixture]
public class ContentMapperTest
{
    /// <summary>
    /// Tests that the Id property can be correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_Id_Property()
    {
        var column = new ContentMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(Content.Id));
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Node}].[id]"));
    }

    /// <summary>
    /// Tests that the "Trashed" property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_Trashed_Property()
    {
        var column =
            new ContentMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(Content.Trashed));
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Node}].[trashed]"));
    }

    /// <summary>
    /// Tests that the Published property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_Published_Property()
    {
        var column =
            new ContentMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(Content.Published));
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Document}].[published]"));
    }

    /// <summary>
    /// Tests that the Version property maps correctly to the database column.
    /// </summary>
    [Test]
    public void Can_Map_Version_Property()
    {
        var column =
            new ContentMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(Content.VersionId));
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.ContentVersion}].[id]"));
    }
}
