using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Extensions;
using Umbraco.Cms.Tests.Common;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.EFCore;

[TestFixture]
internal sealed class UseUmbracoDatabaseProviderTests
{
    private const string ProviderName = "Test.Provider";

    private const string ConnectionString = "Server=test";

    [Test]
    public void Configurators_That_Handle_The_Configured_Provider_Configure_The_Context()
    {
        var first = new RecordingConfigurator(ProviderName);
        var second = new RecordingConfigurator(ProviderName);
        using ServiceProvider serviceProvider = CreateServiceProvider(ProviderName, first, second);

        new DbContextOptionsBuilder().UseUmbracoDatabaseProvider(serviceProvider);

        Assert.Multiple(() =>
        {
            Assert.That(first.ConfiguredConnectionString, Is.EqualTo(ConnectionString));
            Assert.That(second.ConfiguredConnectionString, Is.EqualTo(ConnectionString));
        });
    }

    [Test]
    public void Configurators_That_Do_Not_Handle_The_Configured_Provider_Are_Not_Used()
    {
        var configurator = new RecordingConfigurator(ProviderName);
        using ServiceProvider serviceProvider = CreateServiceProvider(Constants.ProviderNames.SQLite, configurator);
        var builder = new DbContextOptionsBuilder();

        Assert.Multiple(() =>
        {
            Assert.That(() => builder.UseUmbracoDatabaseProvider(serviceProvider), Throws.TypeOf<InvalidDataException>());
            Assert.That(configurator.ConfiguredConnectionString, Is.Null);
            Assert.That(builder.IsConfigured, Is.False);
        });
    }

    private static ServiceProvider CreateServiceProvider(string providerName, params IDatabaseConfigurator[] configurators)
    {
        var services = new ServiceCollection()
            .AddSingleton<IOptionsMonitor<ConnectionStrings>>(
                new TestOptionsMonitor<ConnectionStrings>(new ConnectionStrings { ProviderName = providerName, ConnectionString = ConnectionString }));
        foreach (IDatabaseConfigurator configurator in configurators)
        {
            services.AddSingleton(configurator);
        }

        return services.BuildServiceProvider();
    }

    private sealed class RecordingConfigurator : IDatabaseConfigurator
    {
        private readonly string _providerName;

        public RecordingConfigurator(string providerName) => _providerName = providerName;

        public string? ConfiguredConnectionString { get; private set; }

        public bool CanHandle(string providerName) => providerName == _providerName;

        public void Configure(DbContextOptionsBuilder builder, string connectionString) => ConfiguredConnectionString = connectionString;
    }
}
