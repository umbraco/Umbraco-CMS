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
public class DataTypeDefinitionRepositorySqlClausesTest : BaseUsingSqlSyntax
{
    [Test]
    public void Can_Verify_Base_Clause()
    {
        var nodeObjectTypeId = Constants.ObjectTypes.DataType;

        var expected = new Sql();
        expected.Select("*")
            .From($"[{Constants.DatabaseSchema.Tables.DataType}]")
            .InnerJoin("[umbracoNode]")
            .On($"[{Constants.DatabaseSchema.Tables.DataType}].[nodeId] = [umbracoNode].[id]")
            .Where("([umbracoNode].[nodeObjectType] = @0)", new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"));

        var sql = Sql();
        sql.SelectAll()
            .From<DataTypeDto>()
            .InnerJoin<NodeDto>()
            .On<DataTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
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
