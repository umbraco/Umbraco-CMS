using System.Text;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Infrastructure.Plugin;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Plugin;

[TestFixture]
public class PluginConfigurationReaderTests
{
    private IPluginConfigurationReader _reader;
    private Mock<IDirectoryContents> _rootDirectoryContentsMock;
    private Mock<ILogger<PluginConfigurationReader>> _loggerMock;
    private Mock<IFileProvider> _fileProviderMock;

    [SetUp]
    public void SetUp()
    {
        _rootDirectoryContentsMock = new Mock<IDirectoryContents>();
        _fileProviderMock = new Mock<IFileProvider>();
        _fileProviderMock
            .Setup(m => m.GetDirectoryContents(Constants.SystemDirectories.AppPlugins))
            .Returns(_rootDirectoryContentsMock.Object);
        var fileProviderFactoryMock = new Mock<IPluginConfigurationFileProviderFactory>();
        fileProviderFactoryMock.Setup(m => m.Create()).Returns(_fileProviderMock.Object);

        _loggerMock = new Mock<ILogger<PluginConfigurationReader>>();
        _reader = new PluginConfigurationReader(fileProviderFactoryMock.Object, new SystemTextJsonSerializer(), _loggerMock.Object);
    }

    [Test]
    public async Task Can_Read_PluginConfigurations_At_Root()
    {
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { CreatePluginConfigurationFile() }.GetEnumerator());

        var result = await _reader.ReadPluginConfigurationsAsync();
        Assert.AreEqual(1, result.Count());

        var first = result.First();
        Assert.AreEqual("My Plugin Configuration", first.Name);
        Assert.AreEqual("1.2.3", first.Version);
        Assert.AreEqual(2, first.Extensions.Count());
        Assert.IsTrue(first.Extensions.All(e => e is JsonElement));
    }

    [Test]
    public async Task Can_Read_PluginConfiguration_In_Root_Directories()
    {
        var plugin1 = CreateDirectoryMock("/my-extension", CreatePluginConfigurationFile(DefaultPluginConfigurationContent("Plugin One")));
        var plugin2 = CreateDirectoryMock("/my-other-extension", CreatePluginConfigurationFile(DefaultPluginConfigurationContent("Plugin Two")));
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { plugin1, plugin2 }.GetEnumerator());

        var result = await _reader.ReadPluginConfigurationsAsync();
        Assert.AreEqual(2, result.Count());
        Assert.AreEqual("Plugin One", result.First().Name);
        Assert.AreEqual("Plugin Two", result.Last().Name);
    }

    [Test]
    public async Task Can_Read_PluginConfigurations_Recursively()
    {
        var childFolder = CreateDirectoryMock("/my-parent-folder/my-child-folder", CreatePluginConfigurationFile(DefaultPluginConfigurationContent("Nested Plugin")));
        var parentFolder = CreateDirectoryMock("/my-parent-folder", childFolder);

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { parentFolder }.GetEnumerator());

        var result = await _reader.ReadPluginConfigurationsAsync();
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("Nested Plugin", result.First().Name);
    }

    [Test]
    public async Task Can_Skip_Empty_Directories()
    {
        var pluginFolder = CreateDirectoryMock("/my-plugin-folder", CreatePluginConfigurationFile(DefaultPluginConfigurationContent("My Plugin")));
        var emptyFolder = CreateDirectoryMock("/my-empty-folder");

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { emptyFolder, pluginFolder }.GetEnumerator());

        var result = await _reader.ReadPluginConfigurationsAsync();
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("My Plugin", result.First().Name);
    }

    [Test]
    public async Task Can_Skip_Other_Files()
    {
        var pluginFolder = CreateDirectoryMock(
            "/my-plugin-folder",
            CreateOtherFile("my.js"),
            CreatePluginConfigurationFile(DefaultPluginConfigurationContent("My Plugin")));
        var otherFolder = CreateDirectoryMock(
            "/my-empty-folder",
            CreateOtherFile("some.js"),
            CreateOtherFile("some.css"));

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { otherFolder, pluginFolder }.GetEnumerator());

        var result = await _reader.ReadPluginConfigurationsAsync();
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("My Plugin", result.First().Name);
    }

    [Test]
    public async Task Can_Handle_All_Empty_Directories()
    {
        var folders = Enumerable.Range(1, 10).Select(i => CreateDirectoryMock($"/my-empty-folder-{i}")).ToList();

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(folders.GetEnumerator());

        var result = await _reader.ReadPluginConfigurationsAsync();
        Assert.AreEqual(0, result.Count());
    }

    [Test]
    public async Task Cannot_Read_PluginConfiguration_Without_Name()
    {
        var content = @"{
    ""version"": ""1.2.3"",
    ""allowTelemetry"": true,
    ""extensions"": [{
            ""type"": ""tree""
        }, {
            ""type"": ""headerApp""
        }
    ]
}";
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { CreatePluginConfigurationFile(content) }.GetEnumerator());

        var result = await _reader.ReadPluginConfigurationsAsync();
        Assert.AreEqual(0, result.Count());

        EnsureLogErrorWasCalled();
    }

    [Test]
    public async Task Cannot_Read_PluginConfiguration_Without_Extensions()
    {
        var content = @"{
    ""name"": ""Something"",
    ""version"": ""1.2.3"",
    ""allowTelemetry"": true
}";
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { CreatePluginConfigurationFile(content) }.GetEnumerator());

        var result = await _reader.ReadPluginConfigurationsAsync();
        Assert.AreEqual(0, result.Count());

        EnsureLogErrorWasCalled();
    }

    [TestCase("This is not JSON")]
    [TestCase(@"{""name"": ""invalid-json"", ""version"": ")]
    public async Task Cannot_Read_Invalid_PluginConfiguration(string content)
    {
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { CreatePluginConfigurationFile(content) }.GetEnumerator());

        var result = await _reader.ReadPluginConfigurationsAsync();
        Assert.AreEqual(0, result.Count());

        EnsureLogErrorWasCalled();
    }

    private void EnsureLogErrorWasCalled(int numberOfTimes = 1) =>
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Exactly(numberOfTimes));

    private IFileInfo CreateDirectoryMock(string path, params IFileInfo[] children)
    {
        var directoryContentsMock = new Mock<IDirectoryContents>();
        directoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(children.ToList().GetEnumerator());

        _fileProviderMock
            .Setup(m => m.GetDirectoryContents($"{Constants.SystemDirectories.AppPlugins}{path}"))
            .Returns(directoryContentsMock.Object);

        var fileInfo = new Mock<IFileInfo>();
        fileInfo.SetupGet(f => f.IsDirectory).Returns(true);
        fileInfo.SetupGet(f => f.Name).Returns(path.Split(Constants.CharArrays.ForwardSlash).Last());

        return fileInfo.Object;
    }

    private IFileInfo CreatePluginConfigurationFile(string? content = null)
    {
        content ??= DefaultPluginConfigurationContent();

        var fileInfo = new Mock<IFileInfo>();
        fileInfo.SetupGet(f => f.IsDirectory).Returns(false);
        fileInfo.SetupGet(f => f.Name).Returns("umbraco-package.json");
        fileInfo.Setup(f => f.CreateReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes(content)));

        return fileInfo.Object;
    }

    private IFileInfo CreateOtherFile(string name)
    {
        var fileInfo = new Mock<IFileInfo>();
        fileInfo.SetupGet(f => f.IsDirectory).Returns(false);
        fileInfo.SetupGet(f => f.Name).Returns(name);
        fileInfo.Setup(f => f.CreateReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("this is some file content")));

        return fileInfo.Object;
    }

    private static string DefaultPluginConfigurationContent(string name = "My Plugin Configuration")
        => @"{
    ""name"": ""##NAME##"",
    ""version"": ""1.2.3"",
    ""allowTelemetry"": true,
    ""extensions"": [{
            ""type"": ""tree""
        }, {
            ""type"": ""headerApp""
        }
    ]
}".Replace("##NAME##", name);
}
