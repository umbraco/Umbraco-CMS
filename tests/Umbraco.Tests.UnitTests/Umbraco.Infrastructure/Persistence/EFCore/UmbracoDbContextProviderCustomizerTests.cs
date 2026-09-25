using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Persistence.EFCore.Sqlite;
using Umbraco.Cms.Persistence.EFCore.SqlServer;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.EFCore;

/// <summary>
///     Builds the <see cref="UmbracoDbContext"/> model for each provider with the customizers that provider's
///     <c>AddUmbracoEFCore*Support</c> registration adds, and checks that the model ends up carrying what the
///     customizers declare and nothing from the other provider.
/// </summary>
[TestFixture]
public class UmbracoDbContextProviderCustomizerTests
{
    private const string SqlServerAnnotationPrefix = "SqlServer:";

    private static IEnumerable<TestCaseData> SqlServerIncludedColumnIndexes()
    {
        yield return new TestCaseData(typeof(NodeDto), $"IX_{NodeDto.TableName}_UniqueId", nameof(NodeDto.ParentId));
        yield return new TestCaseData(typeof(NodeDto), $"IX_{NodeDto.TableName}_parentId_nodeObjectType", nameof(NodeDto.Trashed));
        yield return new TestCaseData(typeof(NodeDto), $"IX_{NodeDto.TableName}_Level", nameof(NodeDto.UserId));
        yield return new TestCaseData(typeof(NodeDto), $"IX_{NodeDto.TableName}_ObjectType_trashed_sorted", nameof(NodeDto.UniqueId));
        yield return new TestCaseData(typeof(NodeDto), $"IX_{NodeDto.TableName}_ObjectType", nameof(NodeDto.UniqueId));
        yield return new TestCaseData(typeof(ContentVersionDto), $"IX_{ContentVersionDto.TableName}_NodeId", nameof(ContentVersionDto.VersionDate));
        yield return new TestCaseData(typeof(ContentVersionDto), $"IX_{ContentVersionDto.TableName}_Current", nameof(ContentVersionDto.NodeId));
        yield return new TestCaseData(typeof(ContentVersionCultureVariationDto), $"IX_{ContentVersionCultureVariationDto.TableName}_VersionId", nameof(ContentVersionCultureVariationDto.Name));
        yield return new TestCaseData(typeof(DocumentVersionDto), $"IX_{DocumentVersionDto.TableName}_id_published", nameof(DocumentVersionDto.TemplateId));
        yield return new TestCaseData(typeof(DocumentVersionDto), $"IX_{DocumentVersionDto.TableName}_published", nameof(DocumentVersionDto.TemplateId));
        yield return new TestCaseData(typeof(TagDto), $"IX_{TagDto.TableName}_languageId_group", nameof(TagDto.Text));
        yield return new TestCaseData(typeof(TagRelationshipDto), $"IX_{TagRelationshipDto.TableName}_tagId_nodeId", nameof(TagRelationshipDto.PropertyTypeId));
        yield return new TestCaseData(typeof(RedirectUrlDto), $"IX_{RedirectUrlDto.TableName}_culture_hash", nameof(RedirectUrlDto.UrlHash));
        yield return new TestCaseData(typeof(UserGroup2GranularPermissionDto), $"IX_{UserGroup2GranularPermissionDto.TableName}_UserGroupKey_UniqueId", nameof(UserGroup2GranularPermissionDto.UniqueId));
    }

    [Test]
    public void AddUmbracoEFCoreSqlServerSupport_RegistersCustomizers_TargetingTheSqlServerEFCoreProvider()
    {
        IEFCoreModelCustomizer[] customizers = GetSqlServerCustomizers();

        Assert.Multiple(() =>
        {
            Assert.That(customizers, Is.Not.Empty);
            foreach (IEFCoreModelCustomizer customizer in customizers)
            {
                Assert.That(
                    customizer.ProviderName,
                    Is.EqualTo(Constants.ProviderNames.EFCore.SQLServer),
                    $"{customizer.GetType().Name} must name the EF Core provider so UmbracoDbContext applies it.");
            }
        });
    }

    [Test]
    public void AddUmbracoEFCoreSqliteSupport_RegistersCustomizers_TargetingTheSqliteEFCoreProvider()
    {
        IEFCoreModelCustomizer[] customizers = GetSqliteCustomizers();

        Assert.Multiple(() =>
        {
            Assert.That(customizers, Is.Not.Empty);
            foreach (IEFCoreModelCustomizer customizer in customizers)
            {
                Assert.That(
                    customizer.ProviderName,
                    Is.EqualTo(Constants.ProviderNames.EFCore.SQLite),
                    $"{customizer.GetType().Name} must name the EF Core provider so UmbracoDbContext applies it.");
            }
        });
    }

    [Test]
    public void OnModelCreating_ForSqlServer_MakesTheDocumentUrlPrimaryKeyNonClusteredAndItsUniqueIndexClustered()
    {
        using UmbracoDbContext context = CreateSqlServerContext();

        IEntityType documentUrl = DesignTimeModel(context).FindEntityType(typeof(DocumentUrlDto))!;
        IIndex uniqueIndex = FindIndex(documentUrl, $"IX_{DocumentUrlDto.TableName}");

        Assert.Multiple(() =>
        {
            Assert.That(documentUrl.FindPrimaryKey()!.IsClustered(), Is.False);
            Assert.That(uniqueIndex.IsClustered(), Is.True);
        });
    }

    [Test]
    public void OnModelCreating_ForSqlServer_MakesTheDocumentUrlAliasPrimaryKeyNonClustered()
    {
        using UmbracoDbContext context = CreateSqlServerContext();

        IEntityType documentUrlAlias = DesignTimeModel(context).FindEntityType(typeof(DocumentUrlAliasDto))!;

        Assert.That(documentUrlAlias.FindPrimaryKey()!.IsClustered(), Is.False);
    }

    [TestCaseSource(nameof(SqlServerIncludedColumnIndexes))]
    public void OnModelCreating_ForSqlServer_AddsIncludedColumnsToTheIndex(Type entityType, string indexName, string includedProperty)
    {
        using UmbracoDbContext context = CreateSqlServerContext();

        IIndex index = FindIndex(DesignTimeModel(context).FindEntityType(entityType)!, indexName);

        Assert.That(index.GetIncludeProperties(), Is.Not.Null.And.Contains(includedProperty));
    }

    [Test]
    public void OnModelCreating_ForSqlServer_DoesNotApplySqliteCollation()
    {
        using UmbracoDbContext context = CreateSqlServerContext();

        IEnumerable<string> collatedProperties = DesignTimeModel(context).GetEntityTypes()
            .SelectMany(entityType => entityType.GetProperties())
            .Where(property => property.GetCollation() is not null)
            .Select(property => $"{property.DeclaringType.Name}.{property.Name}");

        Assert.That(collatedProperties, Is.Empty);
    }

    [Test]
    public void OnModelCreating_ForSqlite_AppliesNocaseCollationToStringProperties()
    {
        using UmbracoDbContext context = CreateSqliteContext();

        IProperty path = DesignTimeModel(context).FindEntityType(typeof(NodeDto))!.FindProperty(nameof(NodeDto.Path))!;

        Assert.That(path.GetCollation(), Is.EqualTo("NOCASE"));
    }

    [Test]
    public void OnModelCreating_ForSqlite_DoesNotApplySqlServerAnnotations()
    {
        using UmbracoDbContext context = CreateSqliteContext();

        IEnumerable<string> sqlServerAnnotations = DesignTimeModel(context).GetEntityTypes()
            .SelectMany(GetAllAnnotations)
            .Where(annotation => annotation.Name.StartsWith(SqlServerAnnotationPrefix, StringComparison.Ordinal))
            .Select(annotation => annotation.Name)
            .Distinct();

        Assert.That(sqlServerAnnotations, Is.Empty);
    }

    /// <summary>
    ///     Provider-specific configuration such as clustering, included columns and collation is only kept on the
    ///     design-time model, which is also the one the migration snapshot is generated from.
    /// </summary>
    private static IModel DesignTimeModel(DbContext context)
        => context.GetService<IDesignTimeModel>().Model;

    private static IEnumerable<IAnnotation> GetAllAnnotations(IEntityType entityType)
        => entityType.GetAnnotations()
            .Concat(entityType.GetKeys().SelectMany(key => key.GetAnnotations()))
            .Concat(entityType.GetIndexes().SelectMany(index => index.GetAnnotations()))
            .Concat(entityType.GetProperties().SelectMany(property => property.GetAnnotations()));

    private static IIndex FindIndex(IEntityType entityType, string databaseName)
        => entityType.GetIndexes().Single(index => index.GetDatabaseName() == databaseName);

    private static IEFCoreModelCustomizer[] GetSqlServerCustomizers()
        => GetRegisteredCustomizers(builder => builder.AddUmbracoEFCoreSqlServerSupport());

    private static IEFCoreModelCustomizer[] GetSqliteCustomizers()
        => GetRegisteredCustomizers(builder => builder.AddUmbracoEFCoreSqliteSupport());

    private static IEFCoreModelCustomizer[] GetRegisteredCustomizers(Action<IUmbracoBuilder> addProviderSupport)
    {
        var services = new ServiceCollection();
        var builder = new UmbracoBuilder(services, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        addProviderSupport(builder);

        return services.BuildServiceProvider().GetServices<IEFCoreModelCustomizer>().ToArray();
    }

    private static UmbracoDbContext CreateSqlServerContext()
        => CreateContext(
            optionsBuilder => optionsBuilder.UseSqlServer("Server=.;Database=x;"),
            GetSqlServerCustomizers());

    private static UmbracoDbContext CreateSqliteContext()
        => CreateContext(
            optionsBuilder => optionsBuilder.UseSqlite("Data Source=:memory:"),
            GetSqliteCustomizers());

    /// <summary>
    ///     Builds a context whose model can be read without opening a connection. EF Core caches the model per
    ///     options set, so a per-instance cache key keeps each test's model independent of the others.
    /// </summary>
    private static UmbracoDbContext CreateContext(
        Action<DbContextOptionsBuilder<UmbracoDbContext>> useProvider,
        IEnumerable<IEFCoreModelCustomizer> customizers)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.Configure<ConnectionStrings>(connectionStrings =>
        {
            connectionStrings.ConnectionString = "placeholder";
            connectionStrings.ProviderName = null;
        });

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        var optionsBuilder = new DbContextOptionsBuilder<UmbracoDbContext>()
            .UseApplicationServiceProvider(serviceProvider)
            .ReplaceService<IModelCacheKeyFactory, PerInstanceModelCacheKeyFactory>();
        useProvider(optionsBuilder);

        return new UmbracoDbContext(optionsBuilder.Options, customizers);
    }

    private class PerInstanceModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime)
            => (context.ContextId, designTime);
    }
}
