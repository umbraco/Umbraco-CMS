using System;

using Moq;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.SyntaxProvider
{
    [NUnit.Framework.Ignore("This doesn't actually test anything")]
    [TestFixture]
    public class MySqlSyntaxProviderTests
    {
        [SetUp]
        public void SetUp()
        {
            SqlSyntaxContext.SqlSyntaxProvider = new MySqlSyntaxProvider(Mock.Of<ILogger>());
        }

        [Test]
        public void Can_Generate_Create_Table_Statement()
        {
            var type = typeof(TagRelationshipDto);
            var definition = DefinitionFactory.GetTableDefinition(type);

            string create = SqlSyntaxContext.SqlSyntaxProvider.Format(definition);
            string primaryKey = SqlSyntaxContext.SqlSyntaxProvider.FormatPrimaryKey(definition);
            var indexes = SqlSyntaxContext.SqlSyntaxProvider.Format(definition.Indexes);
            var keys = SqlSyntaxContext.SqlSyntaxProvider.Format(definition.ForeignKeys);

            Console.WriteLine(create);
            Console.WriteLine(primaryKey);
            foreach (var sql in keys)
            {
                Console.WriteLine(sql);
            }

            foreach (var sql in indexes)
            {
                Console.WriteLine(sql);
            }
        }

        [TearDown]
        public void TearDown()
        {
            SqlSyntaxContext.SqlSyntaxProvider = null;
        }
    }
}