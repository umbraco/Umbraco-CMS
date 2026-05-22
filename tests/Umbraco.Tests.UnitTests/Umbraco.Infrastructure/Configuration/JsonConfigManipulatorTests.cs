using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Configuration;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Configuration;

[TestFixture]
public class JsonConfigManipulatorTests
{
    private const string FirstFileName = "appsettings.json";
    private const string SecondFileName = "appsettings.Development.json";
    private const string ConnectionString = "Data Source=|DataDirectory|/Umbraco.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True";
    private const string ProviderName = "Microsoft.Data.Sqlite";

    private string _tempPath = null!;
    private string _firstFilePath = null!;
    private string _secondFilePath = null!;

    [SetUp]
    public void SetUp()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), "UmbracoTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempPath);
        _firstFilePath = Path.Combine(_tempPath, FirstFileName);
        _secondFilePath = Path.Combine(_tempPath, SecondFileName);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, true);
        }
    }

    [Test]
    public async Task SaveConnectionStringAsync_WritesToLastJsonProvider()
    {
        File.WriteAllText(_firstFilePath, "{}");
        File.WriteAllText(_secondFilePath, "{}");

        JsonConfigManipulator sut = CreateSut(FirstFileName, SecondFileName);

        await sut.SaveConnectionStringAsync(ConnectionString, ProviderName);

        Assert.Multiple(() =>
        {
            Assert.IsNull(ReadConnectionString(_firstFilePath), "Connection string should not be written to the first JSON provider.");
            Assert.AreEqual(ConnectionString, ReadConnectionString(_secondFilePath), "Connection string should be written to the last JSON provider.");
            Assert.AreEqual(ProviderName, ReadProviderName(_secondFilePath));
        });
    }

    [Test]
    public async Task SaveConnectionStringAsync_WithOnlyOneProvider_StillWrites()
    {
        File.WriteAllText(_firstFilePath, "{}");

        JsonConfigManipulator sut = CreateSut(FirstFileName);

        await sut.SaveConnectionStringAsync(ConnectionString, ProviderName);

        Assert.AreEqual(ConnectionString, ReadConnectionString(_firstFilePath));
        Assert.AreEqual(ProviderName, ReadProviderName(_firstFilePath));
    }

    [Test]
    public async Task RemoveConnectionStringAsync_RemovesFromLastJsonProviderThatHasKey()
    {
        var json = $$"""
            {
              "ConnectionStrings": {
                "umbracoDbDSN": "{{ConnectionString}}",
                "umbracoDbDSN_ProviderName": "{{ProviderName}}"
              }
            }
            """;
        File.WriteAllText(_firstFilePath, json);
        File.WriteAllText(_secondFilePath, json);

        JsonConfigManipulator sut = CreateSut(FirstFileName, SecondFileName);

        await sut.RemoveConnectionStringAsync();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(ConnectionString, ReadConnectionString(_firstFilePath), "Connection string should remain in the earlier file.");
            Assert.IsNull(ReadConnectionString(_secondFilePath), "Connection string should be removed from the last file.");
            Assert.IsNull(ReadProviderName(_secondFilePath));
        });
    }

    [TestCase("secrets.json", TestName = "SaveConnectionStringAsync_SkipsMissingUserSecretsFileAndFallsThroughToAppsettings")]
    [TestCase("appsettings.Production.json", TestName = "SaveConnectionStringAsync_SkipsMissingAppsettingsFileAndFallsThroughToAppsettings")]
    public async Task SaveConnectionStringAsync_SkipsMissingSources(string strayFileName)
    {
        // Any JSON source whose backing file doesn't already exist on disk must be skipped (unless
        // explicitly allowlisted in CreatableFileNames) so we never accidentally create files in the
        // project tree. Covers un-initialized user secrets (secrets.json) and a regular
        // appsettings.{Environment}.json that hasn't been created yet.
        File.WriteAllText(_firstFilePath, "{}");
        File.WriteAllText(_secondFilePath, "{}");
        var strayFilePath = Path.Combine(_tempPath, strayFileName);
        Assert.IsFalse(File.Exists(strayFilePath), "Precondition: stray file must not already exist.");

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(_tempPath)
            .AddJsonFile(FirstFileName, optional: false, reloadOnChange: false)
            .AddJsonFile(SecondFileName, optional: false, reloadOnChange: false)
            .AddJsonFile(strayFileName, optional: true, reloadOnChange: false)
            .Build();
        var sut = new JsonConfigManipulator(configuration, Mock.Of<ILogger<JsonConfigManipulator>>());

        await sut.SaveConnectionStringAsync(ConnectionString, ProviderName);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(File.Exists(strayFilePath), $"Connection string must not be written to a non-existent {strayFileName} (no new files should be created).");
            Assert.IsNull(ReadConnectionString(_firstFilePath), "Connection string should not be written to the first JSON provider.");
            Assert.AreEqual(ConnectionString, ReadConnectionString(_secondFilePath), "Connection string should fall through to the last writable JSON provider.");
            Assert.AreEqual(ProviderName, ReadProviderName(_secondFilePath));
        });
    }

    [Test]
    public async Task SaveConnectionStringAsync_WhenAppsettingsLocalIsMissing_CreatesFileWithSchemaReference()
    {
        // appsettings.Local.json is allowlisted in CreatableFileNames so the install can materialize it
        // on first write (it's gitignored, so a fresh clone doesn't have it on disk).
        File.WriteAllText(_firstFilePath, "{}");
        File.WriteAllText(_secondFilePath, "{}");
        const string LocalFileName = "appsettings.Local.json";
        var localFilePath = Path.Combine(_tempPath, LocalFileName);
        Assert.IsFalse(File.Exists(localFilePath), "Precondition: appsettings.Local.json must not exist before the install runs.");

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(_tempPath)
            .AddJsonFile(FirstFileName, optional: false, reloadOnChange: false)
            .AddJsonFile(SecondFileName, optional: false, reloadOnChange: false)
            .AddJsonFile(LocalFileName, optional: true, reloadOnChange: false)
            .Build();
        var sut = new JsonConfigManipulator(configuration, Mock.Of<ILogger<JsonConfigManipulator>>());

        await sut.SaveConnectionStringAsync(ConnectionString, ProviderName);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(File.Exists(localFilePath), "appsettings.Local.json should be created on first write because it's allowlisted.");
            Assert.IsNull(ReadConnectionString(_firstFilePath), "Connection string should not be written to the first JSON provider.");
            Assert.IsNull(ReadConnectionString(_secondFilePath), "Connection string should not fall through to appsettings.Development.json when the allowlisted source accepts the write.");
            Assert.AreEqual(ConnectionString, ReadConnectionString(localFilePath));
            Assert.AreEqual(ProviderName, ReadProviderName(localFilePath));
            Assert.AreEqual("./appsettings-schema.json", ReadJsonValue(localFilePath, "$schema"), "Newly created appsettings.Local.json should include the $schema reference.");
        });
    }

    [Test]
    public async Task SetGlobalIdAsync_StillWritesToFirstProvider()
    {
        File.WriteAllText(_firstFilePath, "{}");
        File.WriteAllText(_secondFilePath, "{}");

        JsonConfigManipulator sut = CreateSut(FirstFileName, SecondFileName);

        var id = Guid.NewGuid().ToString();
        await sut.SetGlobalIdAsync(id);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(id, ReadJsonValue(_firstFilePath, "Umbraco", "CMS", "Global", "Id"), "Global Id should be written to the first JSON provider (existing behaviour preserved).");
            Assert.IsNull(ReadJsonValue(_secondFilePath, "Umbraco", "CMS", "Global", "Id"));
        });
    }

    private JsonConfigManipulator CreateSut(params string[] fileNames)
    {
        IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(_tempPath);
        foreach (var fileName in fileNames)
        {
            builder.AddJsonFile(fileName, optional: false, reloadOnChange: false);
        }

        IConfigurationRoot configuration = builder.Build();
        return new JsonConfigManipulator(configuration, Mock.Of<ILogger<JsonConfigManipulator>>());
    }

    private static string? ReadConnectionString(string path) =>
        ReadJsonValue(path, "ConnectionStrings", "umbracoDbDSN");

    private static string? ReadProviderName(string path) =>
        ReadJsonValue(path, "ConnectionStrings", "umbracoDbDSN_ProviderName");

    private static string? ReadJsonValue(string path, params string[] segments)
    {
        JsonNode? node = JsonNode.Parse(File.ReadAllText(path));
        foreach (var segment in segments)
        {
            if (node is null)
            {
                return null;
            }

            node = node[segment];
        }

        return node?.GetValue<string>();
    }
}
