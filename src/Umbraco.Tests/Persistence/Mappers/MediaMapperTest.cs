using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class MediaMapperTest
    {
        [Test]
        public void Can_Map_Id_Property()
        {
            var column = new MediaMapper().Map(new SqlCeSyntaxProvider(), nameof(Media.Id));
            Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Node}].[id]"));
        }

        [Test]
        public void Can_Map_Trashed_Property()
        {
            var column = new MediaMapper().Map(new SqlCeSyntaxProvider(), nameof(Media.Trashed));
            Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Node}].[trashed]"));
        }

        [Test]
        public void Can_Map_UpdateDate_Property()
        {
            var column = new MediaMapper().Map(new SqlCeSyntaxProvider(), nameof(Media.UpdateDate));
            Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.ContentVersion}].[versionDate]"));
        }

        [Test]
        public void Can_Map_Version_Property()
        {
            var column = new MediaMapper().Map(new SqlCeSyntaxProvider(), nameof(Media.VersionId));
            Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.ContentVersion}].[id]"));
        }
    }
}
