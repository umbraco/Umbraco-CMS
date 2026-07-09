// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_4_0;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V17_4_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class AddDimensionsToSvgTests : UmbracoIntegrationTest
{
    private static readonly Guid _expectedWidthKey = new(Constants.Conventions.Media.PropertyTypeKeys.VectorGraphicsWidth);
    private static readonly Guid _expectedHeightKey = new(Constants.Conventions.Media.PropertyTypeKeys.VectorGraphicsHeight);

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMigrationBuilder MigrationBuilder => GetRequiredService<IMigrationBuilder>();

    private IUmbracoDatabaseFactory UmbracoDatabaseFactory => GetRequiredService<IUmbracoDatabaseFactory>();

    private IServiceScopeFactory ServiceScopeFactory => GetRequiredService<IServiceScopeFactory>();

    private DistributedCache DistributedCache => GetRequiredService<DistributedCache>();

    private IDatabaseCacheRebuilder DatabaseCacheRebuilder => GetRequiredService<IDatabaseCacheRebuilder>();

    private IPublishedContentTypeFactory PublishedContentTypeFactory => GetRequiredService<IPublishedContentTypeFactory>();

    private IMigrationPlanExecutor MigrationPlanExecutor => new MigrationPlanExecutor(
        ScopeProvider,
        ScopeAccessor,
        LoggerFactory,
        MigrationBuilder,
        UmbracoDatabaseFactory,
        DatabaseCacheRebuilder,
        DistributedCache,
        Mock.Of<IKeyValueService>(),
        ServiceScopeFactory,
        AppCaches.NoCache,
        PublishedContentTypeFactory);

    [Test]
    public async Task Adds_Width_And_Height_With_Canonical_Keys()
    {
        await RemoveDimensionPropertiesToSimulatePreMigrationState();

        await ExecuteMigration();

        IMediaType mediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.VectorGraphicsAlias)!;
        IPropertyType? width = mediaType.PropertyTypes.SingleOrDefault(x => x.Alias == Constants.Conventions.Media.Width);
        IPropertyType? height = mediaType.PropertyTypes.SingleOrDefault(x => x.Alias == Constants.Conventions.Media.Height);

        Assert.Multiple(() =>
        {
            Assert.That(width, Is.Not.Null, "Width property was not added by the migration.");
            Assert.That(height, Is.Not.Null, "Height property was not added by the migration.");

            // The migration must assign the same keys as a clean install (DatabaseDataCreator), otherwise
            // upgraded and clean-installed sites diverge and Umbraco Deploy reports schema differences.
            Assert.That(width!.Key, Is.EqualTo(_expectedWidthKey));
            Assert.That(height!.Key, Is.EqualTo(_expectedHeightKey));
        });
    }

    private async Task RemoveDimensionPropertiesToSimulatePreMigrationState()
    {
        IMediaType mediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.VectorGraphicsAlias)!;
        mediaType.RemovePropertyType(Constants.Conventions.Media.Width);
        mediaType.RemovePropertyType(Constants.Conventions.Media.Height);
        await MediaTypeService.UpdateAsync(mediaType, Constants.Security.SuperUserKey);

        IMediaType reloaded = MediaTypeService.Get(Constants.Conventions.MediaTypes.VectorGraphicsAlias)!;
        Assert.That(
            reloaded.PropertyTypes.Any(x =>
                x.Alias == Constants.Conventions.Media.Width || x.Alias == Constants.Conventions.Media.Height),
            Is.False,
            "Arrange failed: dimension properties were not removed.");
    }

    private async Task ExecuteMigration()
    {
        var upgrader = new Upgrader(
            new MigrationPlan("AddDimensionsToSvgTest")
                .From(string.Empty)
                .To<AddDimensionsToSvg>("done"));

        await upgrader.ExecuteAsync(MigrationPlanExecutor, ScopeProvider, Mock.Of<IKeyValueService>());
    }
}
