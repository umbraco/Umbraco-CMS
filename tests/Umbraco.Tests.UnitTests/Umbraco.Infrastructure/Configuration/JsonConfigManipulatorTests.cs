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
            Assert.IsNull(ReadConnectionString(_globalFilePath), "Connection string should not be written to the first JSON provider.");
            Assert.AreEqual(ConnectionString, ReadConnectionString(_environmentFilePath), "Connection string should be written to the last JSON provider.");
            Assert.AreEqual(ProviderName, ReadProviderName(_environmentFilePath));
        });
    }

    [Test]
    public async Task SaveConnectionStringAsync_WithOnlyOneProvider_StillWrites()
    {
        File.WriteAllText(_globalFilePath, "{}");

        JsonConfigManipulator sut = CreateSut(GlobalFileName);

        await sut.SaveConnectionStringAsync(ConnectionString, ProviderName);

        Assert.AreEqual(ConnectionString, ReadConnectionString(_globalFilePath));
        Assert.AreEqual(ProviderName, ReadProviderName(_globalFilePath));
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
            Assert.AreEqual(ConnectionString, ReadConnectionString(_globalFilePath), "Connection string should remain in the earlier file.");
            Assert.IsNull(ReadConnectionString(_environmentFilePath), "Connection string should be removed from the last file.");
            Assert.IsNull(ReadProviderName(_environmentFilePath));
        });
    }

    [TestCase("secrets.json", TestName = "SaveConnectionStringAsync_SkipsMissingUserSecretsFileAndFallsThroughToAppsettings")]
    [TestCase("appsettings.Production.json", TestName = "SaveConnectionStringAsync_SkipsMissingAppsettingsFileAndFallsThroughToAppsettings")]
    public async Task SaveConnectionStringAsync_SkipsMissingSources(string strayFileName)
    {
        // Non-allowlisted JSON sources whose backing file is missing must be skipped, never created.
        Assert.IsFalse(
            JsonConfigManipulator.CreatableFileNames.Contains(strayFileName, StringComparer.OrdinalIgnoreCase),
            $"Precondition: this case exercises the skip path, so {strayFileName} must not be in JsonConfigManipulator.CreatableFileNames.");

        File.WriteAllText(_globalFilePath, "{}");
        File.WriteAllText(_environmentFilePath, "{}");
        var strayFilePath = Path.Combine(_tempPath, strayFileName);
        Assert.IsFalse(File.Exists(strayFilePath), "Precondition: stray file must not already exist.");

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
            Assert.IsFalse(File.Exists(strayFilePath), $"Connection string must not be written to a non-existent {strayFileName} (no new files should be created).");
            Assert.IsNull(ReadConnectionString(_globalFilePath), "Connection string should not be written to the first JSON provider.");
            Assert.AreEqual(ConnectionString, ReadConnectionString(_environmentFilePath), "Connection string should fall through to the last writable JSON provider.");
            Assert.AreEqual(ProviderName, ReadProviderName(_environmentFilePath));
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
        Assert.IsFalse(File.Exists(creatableFilePath), $"Precondition: {creatableFileName} must not exist before the install runs.");

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
            Assert.IsTrue(File.Exists(creatableFilePath), $"{creatableFileName} should be created on first write because it's allowlisted.");
            Assert.IsNull(ReadConnectionString(_globalFilePath), "Connection string should not be written to the first JSON provider.");
            Assert.IsNull(ReadConnectionString(_environmentFilePath), $"Connection string should not fall through to {EnvironmentFileName} when the allowlisted source accepts the write.");
            Assert.AreEqual(ConnectionString, ReadConnectionString(creatableFilePath));
            Assert.AreEqual(ProviderName, ReadProviderName(creatableFilePath));
            Assert.AreEqual("./appsettings-schema.json", ReadJsonValue(creatableFilePath, "$schema"), $"Newly created {creatableFileName} should include the $schema reference.");
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
            Assert.AreEqual(id, ReadJsonValue(_globalFilePath, "Umbraco", "CMS", "Global", "Id"), "Global Id should be written to the first JSON provider (existing behaviour preserved).");
            Assert.IsNull(ReadJsonValue(_environmentFilePath, "Umbraco", "CMS", "Global", "Id"));
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
