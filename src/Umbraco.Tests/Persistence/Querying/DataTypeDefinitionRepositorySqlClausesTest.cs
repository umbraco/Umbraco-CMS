using System;
using System.Diagnostics;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Querying
{
    [TestFixture]
    public class DataTypeDefinitionRepositorySqlClausesTest : BaseUsingSqlCeSyntax
    {
        [Test]
        public void Can_Verify_Base_Clause()
        {
            var NodeObjectTypeId = Constants.ObjectTypes.DataType;

            var expected = new Sql();
            expected.Select("*")
                .From($"[{Constants.DatabaseSchema.Tables.DataType}]")
                .InnerJoin("[umbracoNode]").On($"[{Constants.DatabaseSchema.Tables.DataType}].[nodeId] = [umbracoNode].[id]")
                .Where("([umbracoNode].[nodeObjectType] = @0)", new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"));

            var sql = Sql();
            sql.SelectAll()
               .From<DataTypeDto>()
               .InnerJoin<NodeDto>()
               .On<DataTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
               .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Assert.AreEqual(expected.Arguments.Length, sql.Arguments.Length);
            for (int i = 0; i < expected.Arguments.Length; i++)
            {
                Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
            }

            Debug.Print(sql.SQL);
        }
    }
}
