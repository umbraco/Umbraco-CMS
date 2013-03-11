using System;
using NUnit.Framework;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.SyntaxProvider
{
    [TestFixture]
    public class SqlSyntaxProviderTests
    {
        [SetUp]
        public void SetUp()
        {
            SyntaxConfig.SqlSyntaxProvider = SqlCeSyntaxProvider.Instance;
        }

        [Test]
        public void Can_Generate_Create_Table_Statement()
        {
            var type = typeof (NodeDto);
            var definition = DefinitionFactory.GetTableDefinition(type);

            string create = SyntaxConfig.SqlSyntaxProvider.Format(definition);
            string primaryKey = SyntaxConfig.SqlSyntaxProvider.FormatPrimaryKey(definition);
            var indexes = SyntaxConfig.SqlSyntaxProvider.Format(definition.Indexes);
            var keys = SyntaxConfig.SqlSyntaxProvider.Format(definition.ForeignKeys);

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
            SyntaxConfig.SqlSyntaxProvider = null;
        }
    }
}