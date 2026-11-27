using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
internal sealed class SharedConnectionInitializerTests : UmbracoIntegrationTest
{
    private readonly RecordingInitializer _first = new(matchesUmbracoProvider: true);
    private readonly RecordingInitializer _second = new(matchesUmbracoProvider: true);
    private readonly RecordingInitializer _otherProvider = new(matchesUmbracoProvider: false);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        foreach (RecordingInitializer initializer in new[] { _first, _second, _otherProvider })
        {
            builder.Services.AddSingleton<IEFCoreSharedConnectionInitializer>(serviceProvider =>
            {
                initializer.UmbracoProviderName = serviceProvider.GetRequiredService<IOptionsMonitor<ConnectionStrings>>().CurrentValue.ProviderName;
                return initializer;
            });
        }

        builder.Services.AddUmbracoDbContext<SharedContext>(
            (serviceProvider, options, _, _) => options.UseUmbracoDatabaseProvider(serviceProvider),
            shareUmbracoConnection: true);
    }

    [Test]
    public async Task Every_Initializer_For_The_Umbraco_Provider_Prepares_The_Shared_Connection()
    {
        using (IEfCoreScope<SharedContext> scope = GetRequiredService<IEFCoreScopeProvider<SharedContext>>().CreateScope())
        {
            await scope.ExecuteWithContextAsync<int>(db => db.Database.ExecuteSqlRawAsync("SELECT 1"));
            scope.Complete();
        }

        Assert.Multiple(() =>
        {
            Assert.That(_first.Connections, Is.Not.Empty);
            Assert.That(_second.Connections, Is.Not.Empty);
            Assert.That(_otherProvider.Connections, Is.Empty);
        });
    }

    private sealed class RecordingInitializer : IEFCoreSharedConnectionInitializer
    {
        private readonly bool _matchesUmbracoProvider;

        public RecordingInitializer(bool matchesUmbracoProvider) => _matchesUmbracoProvider = matchesUmbracoProvider;

        public string? UmbracoProviderName { get; set; }

        public List<DbConnection> Connections { get; } = [];

        public string ProviderName => _matchesUmbracoProvider ? UmbracoProviderName! : "Umbraco.Tests.OtherProvider";

        public void Initialize(Microsoft.EntityFrameworkCore.DbContext dbContext, DbConnection connection) => Connections.Add(connection);
    }

    internal sealed class SharedContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public SharedContext(DbContextOptions<SharedContext> options)
            : base(options)
        {
        }
    }
}
