// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V19_0_0;

/// <summary>
/// Shared harness for the min/max-to-range migration integration tests: seeds a data type with a specific
/// pre-migration configuration JSON, runs a single migration end-to-end, and reads the resulting configuration back.
/// </summary>
internal abstract class MigrateMinMaxToRangeTestBase : UmbracoIntegrationTest
{
    protected IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private PropertyEditorCollection PropertyEditors => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer
        => GetRequiredService<IConfigurationEditorJsonSerializer>();

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

    protected static decimal? Min(JsonObject configuration, string rangeKey)
        => (configuration[rangeKey] as JsonObject)?["min"]?.GetValue<decimal>();

    protected static decimal? Max(JsonObject configuration, string rangeKey)
        => (configuration[rangeKey] as JsonObject)?["max"]?.GetValue<decimal>();

    protected async Task<int> CreateDataTypeWithRawConfig(string editorAlias, string configJson)
    {
        IDataEditor editor = PropertyEditors.First(e => e.Alias == editorAlias);
        var dataType = new DataType(editor, ConfigurationEditorJsonSerializer)
        {
            Name = $"Test {editorAlias}",
            DatabaseType = ValueStorageType.Nvarchar,
        };

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        // Overwrite with the exact pre-migration configuration JSON, mirroring an upgraded database.
        using IScope scope = ScopeProvider.CreateScope();
        scope.Database.Execute(
            "UPDATE umbracoDataType SET config = @0 WHERE nodeId = @1",
            configJson,
            dataType.Id);
        scope.Complete();

        return dataType.Id;
    }

    protected Task<JsonObject> GetRawConfig(int nodeId)
    {
        using IScope scope = ScopeProvider.CreateScope();
        var config = scope.Database.ExecuteScalar<string>(
            "SELECT config FROM umbracoDataType WHERE nodeId = @0",
            nodeId);
        scope.Complete();

        return Task.FromResult(JsonNode.Parse(config!)!.AsObject());
    }

    protected async Task RunMigration<TMigration>()
        where TMigration : AsyncMigrationBase
    {
        var plan = new MigrationPlan(typeof(TMigration).Name)
            .From(string.Empty)
            .To<TMigration>("done");
        var upgrader = new Upgrader(plan);
        await upgrader.ExecuteAsync(MigrationPlanExecutor, ScopeProvider, Mock.Of<IKeyValueService>());
    }
}
