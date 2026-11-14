using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Persistence.EFCore;
using Umbraco.Cms.Tests.Common;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

[TestFixture]
internal sealed class UseUmbracoDatabaseProviderTests
{
    private const string ProviderName = "Test.Provider";

    private const string ConnectionString = "Server=test";

    [Test]
    public void A_Provider_Setup_Registered_For_The_Configured_Provider_Configures_The_Context()
    {
        var setup = new RecordingProviderSetup(ProviderName);
        var builder = new DbContextOptionsBuilder();

        builder.UseUmbracoDatabaseProvider(CreateServiceProvider(ProviderName, setup));

        Assert.That(setup.ConfiguredConnectionString, Is.EqualTo(ConnectionString));
    }

    [Test]
    public void A_Provider_Setup_Registered_For_Another_Provider_Is_Not_Used()
    {
        var setup = new RecordingProviderSetup(ProviderName);
        var builder = new DbContextOptionsBuilder();

        builder.UseUmbracoDatabaseProvider(CreateServiceProvider(Constants.ProviderNames.SQLLite, setup));

        Assert.Multiple(() =>
        {
            Assert.That(setup.ConfiguredConnectionString, Is.Null);
            Assert.That(builder.IsConfigured, Is.True);
        });
    }

    private static ServiceProvider CreateServiceProvider(string providerName, IEFCoreDatabaseProviderSetup setup)
        => new ServiceCollection()
            .AddSingleton<IOptionsMonitor<ConnectionStrings>>(
                new TestOptionsMonitor<ConnectionStrings>(new ConnectionStrings { ProviderName = providerName, ConnectionString = ConnectionString }))
            .AddSingleton(setup)
            .BuildServiceProvider();

    private sealed class RecordingProviderSetup : IEFCoreDatabaseProviderSetup
    {
        public RecordingProviderSetup(string providerName) => ProviderName = providerName;

        public string ProviderName { get; }

        public string? ConfiguredConnectionString { get; private set; }

        public void Setup(DbContextOptionsBuilder builder, string connectionString) => ConfiguredConnectionString = connectionString;
    }
}
