using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class ContentMapperTest
    {
        [Test]
        public void Can_Map_Id_Property()
        {
            var column = new ContentMapper().Map(new SqlCeSyntaxProvider(), nameof(Content.Id));
            Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Node}].[id]"));
        }

        [Test]
        public void Can_Map_Trashed_Property()
        {
            var column = new ContentMapper().Map(new SqlCeSyntaxProvider(), nameof(Content.Trashed));
            Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Node}].[trashed]"));
        }

        [Test]
        public void Can_Map_Published_Property()
        {
            var column = new ContentMapper().Map(new SqlCeSyntaxProvider(), nameof(Content.Published));
            Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Document}].[published]"));
        }

        [Test]
        public void Can_Map_Version_Property()
        {
            var column = new ContentMapper().Map(new SqlCeSyntaxProvider(), nameof(Content.VersionId));
            Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.ContentVersion}].[id]"));
        }
    }
}
