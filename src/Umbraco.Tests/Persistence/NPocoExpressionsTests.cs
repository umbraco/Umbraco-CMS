using NPoco;
using NUnit.Framework;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    public class NPocoExpressionsTests : BaseUsingSqlCeSyntax
    {
        [Test]
        public void WhereInValueFieldTest()
        {
            var sql = new Sql<SqlContext>(SqlContext)
                .Select("*")
                .From<NodeDto>()
                .WhereIn<NodeDto>(x => x.NodeId, new[] { 1, 2, 3 });
            Assert.AreEqual("SELECT *\nFROM [umbracoNode]\nWHERE ([umbracoNode].[id] IN (@0,@1,@2))", sql.SQL);
        }

        [Test]
        public void WhereInObjectFieldTest()
        {
            // this test used to fail because x => x.Text was evaluated as a lambda
            // and returned "[umbracoNode].[text] = @0"... had to fix WhereIn.

            var sql = new Sql<SqlContext>(SqlContext)
                .Select("*")
                .From<NodeDto>()
                .WhereIn<NodeDto>(x => x.Text, new[] { "a", "b", "c" });
            Assert.AreEqual("SELECT *\nFROM [umbracoNode]\nWHERE ([umbracoNode].[text] IN (@0,@1,@2))", sql.SQL);
        }
    }
}