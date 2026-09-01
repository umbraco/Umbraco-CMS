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
    private const string GlobalFileName = "appsettings.json";
    private const string EnvironmentFileName = "appsettings.Development.json";
    private const string ConnectionString = "Data Source=|DataDirectory|/Umbraco.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True";
    private const string ProviderName = "Microsoft.Data.Sqlite";

    private string _tempPath = null!;
    private string _globalFilePath = null!;
    private string _environmentFilePath = null!;

    [SetUp]
    public void SetUp()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), "UmbracoTests", Guid.NewGuid().ToString());
        _globalFilePath = Path.Combine(_tempPath, GlobalFileName);
        _environmentFilePath = Path.Combine(_tempPath, EnvironmentFileName);

        // Ensure the directory exists
        Directory.CreateDirectory(_tempPath);
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
        File.WriteAllText(_globalFilePath, "{}");
        File.WriteAllText(_environmentFilePath, "{}");

        JsonConfigManipulator sut = CreateSut(GlobalFileName, EnvironmentFileName);

        await sut.SaveConnectionStringAsync(ConnectionString, ProviderName);

        Assert.Multiple(() =>
        {
            Assert.That(ReadConnectionString(_globalFilePath), Is.Null, "Connection string should not be written to the first JSON provider.");
            Assert.That(ReadConnectionString(_environmentFilePath), Is.EqualTo(ConnectionString), "Connection string should be written to the last JSON provider.");
            Assert.That(ReadProviderName(_environmentFilePath), Is.EqualTo(ProviderName));
        });
    }

    [Test]
    public async Task SaveConnectionStringAsync_WithOnlyOneProvider_StillWrites()
    {
        File.WriteAllText(_globalFilePath, "{}");

        JsonConfigManipulator sut = CreateSut(GlobalFileName);

        await sut.SaveConnectionStringAsync(ConnectionString, ProviderName);

        Assert.That(ReadConnectionString(_globalFilePath), Is.EqualTo(ConnectionString));
        Assert.That(ReadProviderName(_globalFilePath), Is.EqualTo(ProviderName));
    }

    [Test]
    public async Task RemoveConnectionStringAsync_RemovesFromLastJsonProviderThatHasKey()
    {
        const string json = $$"""
            {
              "ConnectionStrings": {
                "umbracoDbDSN": "{{ConnectionString}}",
                "umbracoDbDSN_ProviderName": "{{ProviderName}}"
              }
            }
            """;

        File.WriteAllText(_globalFilePath, json);
        File.WriteAllText(_environmentFilePath, json);

        JsonConfigManipulator sut = CreateSut(GlobalFileName, EnvironmentFileName);

        await sut.RemoveConnectionStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(ReadConnectionString(_globalFilePath), Is.EqualTo(ConnectionString), "Connection string should remain in the earlier file.");
            Assert.That(ReadConnectionString(_environmentFilePath), Is.Null, "Connection string should be removed from the last file.");
            Assert.That(ReadProviderName(_environmentFilePath), Is.Null);
        });
    }

    [TestCase("secrets.json", TestName = "SaveConnectionStringAsync_SkipsMissingUserSecretsFileAndFallsThroughToAppsettings")]
    [TestCase("appsettings.Production.json", TestName = "SaveConnectionStringAsync_SkipsMissingAppsettingsFileAndFallsThroughToAppsettings")]
    public async Task SaveConnectionStringAsync_SkipsMissingSources(string strayFileName)
    {
        // Non-allowlisted JSON sources whose backing file is missing must be skipped, never created.
        Assert.That(
            JsonConfigManipulator.CreatableFileNames.Contains(strayFileName, StringComparer.OrdinalIgnoreCase),
            Is.False,
            $"Precondition: this case exercises the skip path, so {strayFileName} must not be in JsonConfigManipulator.CreatableFileNames.");

        File.WriteAllText(_globalFilePath, "{}");
        File.WriteAllText(_environmentFilePath, "{}");
        var strayFilePath = Path.Combine(_tempPath, strayFileName);
        Assert.That(File.Exists(strayFilePath), Is.False, "Precondition: stray file must not already exist.");

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(_tempPath)
            .AddJsonFile(GlobalFileName, optional: false, reloadOnChange: false)
            .AddJsonFile(EnvironmentFileName, optional: false, reloadOnChange: false)
            .AddJsonFile(strayFileName, optional: true, reloadOnChange: false)
            .Build();
        var sut = new JsonConfigManipulator(configuration, Mock.Of<ILogger<JsonConfigManipulator>>());

        await sut.SaveConnectionStringAsync(ConnectionString, ProviderName);

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(strayFilePath), Is.False, $"Connection string must not be written to a non-existent {strayFileName} (no new files should be created).");
            Assert.That(ReadConnectionString(_globalFilePath), Is.Null, "Connection string should not be written to the first JSON provider.");
            Assert.That(ReadConnectionString(_environmentFilePath), Is.EqualTo(ConnectionString), "Connection string should fall through to the last writable JSON provider.");
            Assert.That(ReadProviderName(_environmentFilePath), Is.EqualTo(ProviderName));
        });
    }

    [TestCaseSource(typeof(JsonConfigManipulator), nameof(JsonConfigManipulator.CreatableFileNames))]
    public async Task SaveConnectionStringAsync_WhenCreatableSourceIsMissing_CreatesFileWithSchemaReference(string creatableFileName)
    {
        // Allowlisted sources are materialized on first write so a fresh clone still lands the
        // connection string in the local override rather than appsettings.{Environment}.json.
        File.WriteAllText(_globalFilePath, "{}");
        File.WriteAllText(_environmentFilePath, "{}");
        var creatableFilePath = Path.Combine(_tempPath, creatableFileName);
        Assert.That(File.Exists(creatableFilePath), Is.False, $"Precondition: {creatableFileName} must not exist before the install runs.");

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(_tempPath)
            .AddJsonFile(GlobalFileName, optional: false, reloadOnChange: false)
            .AddJsonFile(EnvironmentFileName, optional: false, reloadOnChange: false)
            .AddJsonFile(creatableFileName, optional: true, reloadOnChange: false)
            .Build();
        var sut = new JsonConfigManipulator(configuration, Mock.Of<ILogger<JsonConfigManipulator>>());

        await sut.SaveConnectionStringAsync(ConnectionString, ProviderName);

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(creatableFilePath), Is.True, $"{creatableFileName} should be created on first write because it's allowlisted.");
            Assert.That(ReadConnectionString(_globalFilePath), Is.Null, "Connection string should not be written to the first JSON provider.");
            Assert.That(ReadConnectionString(_environmentFilePath), Is.Null, $"Connection string should not fall through to {EnvironmentFileName} when the allowlisted source accepts the write.");
            Assert.That(ReadConnectionString(creatableFilePath), Is.EqualTo(ConnectionString));
            Assert.That(ReadProviderName(creatableFilePath), Is.EqualTo(ProviderName));
            Assert.That(ReadJsonValue(creatableFilePath, "$schema"), Is.EqualTo("./appsettings-schema.json"), $"Newly created {creatableFileName} should include the $schema reference.");
        });
    }

    [Test]
    public async Task SetGlobalIdAsync_StillWritesToFirstProvider()
    {
        File.WriteAllText(_globalFilePath, "{}");
        File.WriteAllText(_environmentFilePath, "{}");

        JsonConfigManipulator sut = CreateSut(GlobalFileName, EnvironmentFileName);

        var id = Guid.NewGuid().ToString();
        await sut.SetGlobalIdAsync(id);

        Assert.Multiple(() =>
        {
            Assert.That(ReadJsonValue(_globalFilePath, "Umbraco", "CMS", "Global", "Id"), Is.EqualTo(id), "Global Id should be written to the first JSON provider (existing behaviour preserved).");
            Assert.That(ReadJsonValue(_environmentFilePath, "Umbraco", "CMS", "Global", "Id"), Is.Null);
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
