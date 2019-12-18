using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class ContentMapperTest : MapperTestBase
    {
        [Test]
        public void Can_Map_Id_Property()
        {
            var column = new ContentMapper(MockSqlContext(), CreateMaps()).Map(nameof(Content.Id));
            Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Node}].[id]"));
        }

        [Test]
        public void Can_Map_Trashed_Property()
        {
            var column = new ContentMapper(MockSqlContext(), CreateMaps()).Map(nameof(Content.Trashed));
            Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Node}].[trashed]"));
        }

        [Test]
        public void Can_Map_Published_Property()
        {
            var column = new ContentMapper(MockSqlContext(), CreateMaps()).Map(nameof(Content.Published));
            Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.Document}].[published]"));
        }

        [Test]
        public void Can_Map_Version_Property()
        {
            var column = new ContentMapper(MockSqlContext(), CreateMaps()).Map(nameof(Content.VersionId));
            Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.ContentVersion}].[id]"));
        }
    }
}
