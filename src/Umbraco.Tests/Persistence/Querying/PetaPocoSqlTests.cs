using System;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Querying
{
    [TestFixture]
    public class PetaPocoSqlTests : BaseUsingSqlCeSyntax
    {
        [Test]
        public void Generate_Replace_Entity_Permissions_Test()
        {
            // Act
            var sql = PermissionRepository<IContent>.GenerateReplaceEntityPermissionsSql(123, "A", new object[] {10, 11, 12});

            // Assert
            Assert.AreEqual(@"SET [permission]='A' WHERE (([nodeId]=123) AND ([userId]=10 OR [userId]=11 OR [userId]=12))", sql);
        }

        [Test]
        public void Generate_Replace_Entity_Permissions_With_Descendants_Test()
        {
            // Act
            var sql = PermissionRepository<IContent>.GenerateReplaceEntityPermissionsSql(new[] { 123, 456 },"A", new object[] { 10, 11, 12 });

            // Assert
            Assert.AreEqual(@"SET [permission]='A' WHERE (([nodeId]=123 OR [nodeId]=456) AND ([userId]=10 OR [userId]=11 OR [userId]=12))", sql);
        }

        [Test]
        public void Can_Select_From_With_Type()
        {
            var expected = new Sql();
            expected.Select("*").From("[cmsContent]");

            var sql = new Sql();
            sql.Select("*").From<ContentDto>();

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_InnerJoin_With_Types()
        {
            var expected = new Sql();
            expected.Select("*")
                .From("[cmsDocument]")
                .InnerJoin("[cmsContentVersion]")
                .On("[cmsDocument].[versionId] = [cmsContentVersion].[VersionId]");

            var sql = new Sql();
            sql.Select("*").From<DocumentDto>()
                .InnerJoin<ContentVersionDto>()
                .On<DocumentDto, ContentVersionDto>(left => left.VersionId, right => right.VersionId);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_OrderBy_With_Type()
        {
            var expected = new Sql();
            expected.Select("*").From("[cmsContent]").OrderBy("[cmsContent].[contentType]");

            var sql = new Sql();
            sql.Select("*").From<ContentDto>().OrderBy<ContentDto>(x => x.ContentTypeId);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_GroupBy_With_Type()
        {
            var expected = new Sql();
            expected.Select("*").From("[cmsContent]").GroupBy("[contentType]");

            var sql = new Sql();
            sql.Select("*").From<ContentDto>().GroupBy<ContentDto>(x => x.ContentTypeId);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_Use_Where_Predicate()
        {
            var expected = new Sql();
            expected.Select("*").From("[cmsContent]").Where("[cmsContent].[nodeId] = 1045");

            var sql = new Sql();
            sql.Select("*").From<ContentDto>().Where<ContentDto>(x => x.NodeId == 1045);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_Use_Where_And_Predicate()
        {
            var expected = new Sql();
            expected.Select("*")
                .From("[cmsContent]")
                .Where("[cmsContent].[nodeId] = 1045")
                .Where("[cmsContent].[contentType] = 1050");

            var sql = new Sql();
            sql.Select("*")
                .From<ContentDto>()
                .Where<ContentDto>(x => x.NodeId == 1045)
                .Where<ContentDto>(x => x.ContentTypeId == 1050);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }
    }
}