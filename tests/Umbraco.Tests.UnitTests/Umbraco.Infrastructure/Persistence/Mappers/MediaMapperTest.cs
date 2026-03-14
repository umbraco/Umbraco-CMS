// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using MediaModel = Umbraco.Cms.Core.Models.Media;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

    /// <summary>
    /// Contains unit tests for the <see cref="MediaMapper"/> class, verifying its mapping functionality within the persistence layer.
    /// </summary>
[TestFixture]
public class MediaMapperTest
{
    /// <summary>
    /// Tests that the Id property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_Id_Property()
    {
        var column =
            new MediaMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(MediaModel.Id));
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Node}].[id]"));
    }

    /// <summary>
    /// Tests that the MediaMapper correctly maps the "Trashed" property to the corresponding database column.
    /// </summary>
    [Test]
    public void Can_Map_Trashed_Property()
    {
        var column =
            new MediaMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(MediaModel.Trashed));
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Node}].[trashed]"));
    }

    /// <summary>
    /// Tests that the UpdateDate property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_UpdateDate_Property()
    {
        var column =
            new MediaMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(MediaModel.UpdateDate));
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.ContentVersion}].[versionDate]"));
    }

    /// <summary>
    /// Tests that the Version property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_Version_Property()
    {
        var column =
            new MediaMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(MediaModel.VersionId));
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.ContentVersion}].[id]"));
    }
}
