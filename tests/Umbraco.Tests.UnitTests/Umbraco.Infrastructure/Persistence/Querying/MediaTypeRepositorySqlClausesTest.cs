// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Diagnostics;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Querying;

[TestFixture]
public class MediaTypeRepositorySqlClausesTest : BaseUsingSqlSyntax
{
    [Test]
    public void Can_Verify_Base_Clause()
    {
        var nodeObjectTypeId = Constants.ObjectTypes.MediaType;

        var expected = new Sql();
        expected.Select("*")
            .From("[cmsContentType]")
            .InnerJoin("[umbracoNode]").On("[cmsContentType].[nodeId] = [umbracoNode].[id]")
            .Where("([umbracoNode].[nodeObjectType] = @0)", new Guid("4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e"));

        var sql = Sql();
        sql.SelectAll()
            .From<ContentTypeDto>()
            .InnerJoin<NodeDto>()
            .On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
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
