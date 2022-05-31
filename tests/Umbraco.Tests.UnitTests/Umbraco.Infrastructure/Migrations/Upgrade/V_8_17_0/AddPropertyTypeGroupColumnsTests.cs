using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_17_0;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Upgrade.V_8_17_0;

[TestFixture]
public class AddPropertyTypeGroupColumnsTests
{
    private readonly IShortStringHelper _shortStringHelper =
        new DefaultShortStringHelper(Options.Create(new RequestHandlerSettings()));

    private readonly ILogger<MigrationContext> _contextLogger = Mock.Of<ILogger<MigrationContext>>();

    [Test]
    public void CreateColumn()
    {
        var database = new TestDatabase();
        var context = new MigrationContext(new MigrationPlan("test"), database, _contextLogger);
        var migration = new AddPropertyTypeGroupColumns(context, _shortStringHelper);

        var dtos = new[]
        {
            new PropertyTypeGroupDto { Id = 0, Text = "Content" }, new PropertyTypeGroupDto { Id = 1, Text = "Content" },
            new PropertyTypeGroupDto { Id = 2, Text = "Settings" },
            new PropertyTypeGroupDto { Id = 3, Text = "Content " }, // The trailing space is intentional
            new PropertyTypeGroupDto { Id = 4, Text = "SEO/OpenGraph" },
            new PropertyTypeGroupDto { Id = 5, Text = "Site defaults" },
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
