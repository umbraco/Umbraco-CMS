using System;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.NPocoTests
{
    [TestFixture]
    public class NPocoSqlTemplateTests
    {
        [Test]
        public void TestSqlTemplates()
        {
            var sqlContext = new SqlContext(new SqlCeSyntaxProvider(), DatabaseType.SQLCe, Mock.Of<IPocoDataFactory>());
            var sqlTemplates = new SqlTemplates(sqlContext);

            // this can be used for queries that we know we'll use a *lot* and
            // want to cache as a (static) template for ever, and ever - note
            // that using a MemoryCache would allow us to set a size limit, or
            // something equivalent, to reduce risk of memory explosion
            var sql = sqlTemplates.Get("xxx", s => s
                .SelectAll()
                .From("zbThing1")
                .Where("id=@id", new { id = "id" })).SqlNamed(new { id = 1 });

            sql.WriteToConsole();

            var sql2 = sqlTemplates.Get("xxx", x => throw new InvalidOperationException("Should be cached.")).Sql(1);

            sql2.WriteToConsole();

            var sql3 = sqlTemplates.Get("xxx", x => throw new InvalidOperationException("Should be cached.")).SqlNamed(new { id = 1 });

            sql3.WriteToConsole();
        }
    }
}
