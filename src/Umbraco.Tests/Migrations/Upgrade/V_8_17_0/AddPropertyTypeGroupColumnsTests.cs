using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Upgrade.V_8_17_0;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Migrations.Upgrade.V_8_17_0
{
    [TestFixture]
    public class AddPropertyTypeGroupColumnsTests : UmbracoTestBase
    {
        protected override void Compose()
        {
            base.Compose();
            Composition.RegisterUnique<IShortStringHelper>(_ => new DefaultShortStringHelper(SettingsForTests.GenerateMockUmbracoSettings()));
        }

        [Test]
        public void CreateColumn()
        {
            var database = new TestDatabase();
            var context = new MigrationContext(database, Logger);
            var migration = new AddPropertyTypeGroupColumns(context);

            var dtos = new[]
            {
                new PropertyTypeGroupDto() { Id = 0, Text = "Content" },
                new PropertyTypeGroupDto() { Id = 1, Text = "Content" },
                new PropertyTypeGroupDto() { Id = 2, Text = "Settings" },
                new PropertyTypeGroupDto() { Id = 3, Text = "Content " }, // The trailing space is intentional
                new PropertyTypeGroupDto() { Id = 4, Text = "SEO/OpenGraph" },
                new PropertyTypeGroupDto() { Id = 5, Text = "Site defaults" }
            };

            var populatedDtos = migration.PopulateAliases(dtos)
                .OrderBy(x => x.Id) // The populated DTOs can be returned in a different order
                .ToArray();

            // All DTOs should be returned and Id and Text should be unaltered
            Assert.That(dtos.Select(x => (x.Id, x.Text)), Is.EquivalentTo(populatedDtos.Select(x => (x.Id, x.Text))));

            // Check populated aliases
            Assert.That(populatedDtos[0].Alias, Is.EqualTo("content"));
            Assert.That(populatedDtos[1].Alias, Is.EqualTo("content"));
            Assert.That(populatedDtos[2].Alias, Is.EqualTo("settings"));
            Assert.That(populatedDtos[3].Alias, Is.EqualTo("content2"));
            Assert.That(populatedDtos[4].Alias, Is.EqualTo("sEOOpenGraph"));
            Assert.That(populatedDtos[5].Alias, Is.EqualTo("siteDefaults"));
        }
    }
}
