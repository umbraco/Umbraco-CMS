using NUnit.Framework;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    public class PetaPocoExpressionsTests
    {
        [Test]
        public void WhereInValueFieldTest()
        {
            var syntax = SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();
            var sql = new Sql()
                .Select("*")
                .From<NodeDto>(syntax)
                .WhereIn<NodeDto>(x => x.NodeId, new[] { 1, 2, 3 }, syntax);
            Assert.AreEqual("SELECT *\nFROM [umbracoNode]\nWHERE ([umbracoNode].[id] IN (@0,@1,@2))", sql.SQL);
        }

        [Test]
        public void WhereInObjectFieldTest()
        {
            // this test used to fail because x => x.Text was evaluated as a lambda
            // and returned "[umbracoNode].[text] = @0"... had to fix WhereIn.

            var syntax = SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();
            var sql = new Sql()
                .Select("*")
                .From<NodeDto>(syntax)
                .WhereIn<NodeDto>(x => x.Text, new[] { "a", "b", "c" }, syntax);
            Assert.AreEqual("SELECT *\nFROM [umbracoNode]\nWHERE ([umbracoNode].[text] IN (@0,@1,@2))", sql.SQL);
        }
    }
}