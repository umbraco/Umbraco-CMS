using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.SchemaLockdown;

[TestFixture]
public class SchemaLockdownSettingsTests
{
    private static IConfigurationSection Section(string json)
        => new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
            .Build()
            .GetSection("Umbraco:CMS:SchemaLockdown");

    private static SchemaLockdownSettings Bind(string json)
    {
        var settings = new SchemaLockdownSettings();
        Section(json).Bind(settings);
        return settings;
    }

    [Test]
    public void Defaults_To_Disabled_With_The_Default_Locked_Set()
    {
        var settings = new SchemaLockdownSettings();

        Assert.Multiple(() =>
        {
            Assert.That(settings.Enabled, Is.False);
            Assert.That(settings.LockedEntityTypes, Does.Contain(Constants.UdiEntityType.DocumentType));
            Assert.That(settings.LockedEntityTypes, Does.Not.Contain(Constants.UdiEntityType.Webhook));
        });
    }

    [Test]
    public void Binds_Kebab_Wire_Form()
    {
        SchemaLockdownSettings settings = Bind("""
        { "Umbraco": { "CMS": { "SchemaLockdown": { "Enabled": true, "LockedEntityTypes": [ "webhook" ] } } } }
        """);

        Assert.Multiple(() =>
        {
            Assert.That(settings.Enabled, Is.True);
            Assert.That(settings.LockedEntityTypes, Does.Contain(Constants.UdiEntityType.Webhook));
        });
    }

    [Test]
    public void Validation_Fails_When_An_Entry_Names_No_Governed_Entity_Type()
    {
        IConfigurationSection section = Section("""
        { "Umbraco": { "CMS": { "SchemaLockdown": { "LockedEntityTypes": [ "not-a-type" ] } } } }
        """);

        Assert.Throws<InvalidOperationException>(() => SchemaLockdownSettings.ValidateBinding(section));
    }

    [Test]
    public void Validation_Accepts_An_Entry_Whatever_Case_It_Was_Written_In()
    {
        IConfigurationSection section = Section("""
        { "Umbraco": { "CMS": { "SchemaLockdown": { "LockedEntityTypes": [ "WebHook" ] } } } }
        """);

        Assert.DoesNotThrow(() => SchemaLockdownSettings.ValidateBinding(section));
    }
}
