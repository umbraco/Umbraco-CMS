// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Querying;

/// <summary>
/// Contains unit tests for verifying the SQL clauses used within the Media Repository.
/// </summary>
[TestFixture]
public class MediaRepositorySqlClausesTest : BaseUsingSqlSyntax
{
    /// <summary>
    /// Verifies that the base SQL clause generated for media repository queries matches the expected SQL statement.
    /// This includes checking the correct table joins, selection, and where clause for the media object type.
    /// Ensures that both the SQL string and its arguments are constructed as intended.
    /// </summary>
    [Test]
    public void Can_Verify_Base_Clause()
    {
        var nodeObjectTypeId = Constants.ObjectTypes.Media;

        var expected = new Sql();
        expected.Select("*")
            .From($"[{Constants.DatabaseSchema.Tables.ContentVersion}]")
            .InnerJoin($"[{Constants.DatabaseSchema.Tables.Content}]").On(
                $"[{Constants.DatabaseSchema.Tables.ContentVersion}].[nodeId] = [{Constants.DatabaseSchema.Tables.Content}].[nodeId]")
            .InnerJoin("[umbracoNode]").On($"[{Constants.DatabaseSchema.Tables.Content}].[nodeId] = [umbracoNode].[id]")
            .Where("([umbracoNode].[nodeObjectType] = @0)", new Guid("b796f64c-1f99-4ffb-b886-4bf4bc011a9c"));

        var sql = Sql();
        sql.SelectAll()
            .From<ContentVersionDto>()
            .InnerJoin<ContentDto>()
            .On<ContentVersionDto, ContentDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<NodeDto>()
            .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .Where<NodeDto>(x => x.NodeObjectType == nodeObjectTypeId);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Assert.AreEqual(expected.Arguments.Length, sql.Arguments.Length);
        for (var i = 0; i < expected.Arguments.Length; i++)
        {
            Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
        }

        Debug.Print(sql.SQL);
    }
}
