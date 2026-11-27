using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models;

[TestFixture]
public class RuntimeSettingsTests
{
    [Test]
    public void MigrationBehavior_Defaults_To_Upgrade()
        => Assert.That(new RuntimeSettings().MigrationBehavior, Is.EqualTo(MigrationBehavior.Upgrade));

    [TestCase("None", MigrationBehavior.None)]
    [TestCase("IgnoreUnknown", MigrationBehavior.IgnoreUnknown)]
    [TestCase("Upgrade", MigrationBehavior.Upgrade)]
    [TestCase("Upgrade, IgnoreUnknown", MigrationBehavior.RollingUpgrade)]
    [TestCase("RollingUpgrade", MigrationBehavior.RollingUpgrade)]
    public void MigrationBehavior_Binds_Combined_Flags_From_Configuration(string value, MigrationBehavior expected)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [nameof(RuntimeSettings.MigrationBehavior)] = value })
            .Build();

        Assert.That(configuration.Get<RuntimeSettings>()!.MigrationBehavior, Is.EqualTo(expected));
    }
}
