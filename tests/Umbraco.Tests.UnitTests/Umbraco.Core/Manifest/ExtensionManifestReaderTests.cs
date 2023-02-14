using System.Text;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Manifest;

[TestFixture]
public class ExtensionManifestReaderTests
{
    private IExtensionManifestReader _reader;
    private Mock<IDirectoryContents> _rootDirectoryContentsMock;
    private Mock<ILogger<ExtensionManifestReader>> _loggerMock;
    private Mock<IFileProvider> _fileProviderMock;

    [SetUp]
    public void SetUp()
    {
        _rootDirectoryContentsMock = new Mock<IDirectoryContents>();
        _fileProviderMock = new Mock<IFileProvider>();
        _fileProviderMock
            .Setup(m => m.GetDirectoryContents(Constants.SystemDirectories.AppPlugins))
            .Returns(_rootDirectoryContentsMock.Object);
        var fileProviderFactoryMock = new Mock<IManifestFileProviderFactory>();
        fileProviderFactoryMock.Setup(m => m.Create()).Returns(_fileProviderMock.Object);

        _loggerMock = new Mock<ILogger<ExtensionManifestReader>>();
        _reader = new ExtensionManifestReader(fileProviderFactoryMock.Object, new SystemTextJsonSerializer(), _loggerMock.Object);
    }

    [Test]
    public async Task CanGetManifestAtRoot()
    {
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { CreateExtensionManifestFile() }.GetEnumerator());

        var result = await _reader.GetManifestsAsync();
        Assert.AreEqual(1, result.Count());

        var first = result.First();
        Assert.AreEqual("My Extension Manifest", first.Name);
        Assert.AreEqual("1.2.3", first.Version);
        Assert.AreEqual(2, first.Extensions.Count());
        Assert.IsTrue(first.Extensions.All(e => e is JsonElement));
    }

    [Test]
    public async Task CanGetManifestsInRootDirectories()
    {
        var directory1 = CreateDirectoryMock("/my-extension", CreateExtensionManifestFile(DefaultManifestContent("Extension One")));
        var directory2 = CreateDirectoryMock("/my-other-extension", CreateExtensionManifestFile(DefaultManifestContent("Extension Two")));
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { directory1, directory2 }.GetEnumerator());

        var result = await _reader.GetManifestsAsync();
        Assert.AreEqual(2, result.Count());
        Assert.AreEqual("Extension One", result.First().Name);
        Assert.AreEqual("Extension Two", result.Last().Name);
    }

    [Test]
    public async Task CanGetManifestsRecursively()
    {
        var childFolder = CreateDirectoryMock("/my-parent-folder/my-child-folder", CreateExtensionManifestFile(DefaultManifestContent("Nested Extension")));
        var parentFolder = CreateDirectoryMock("/my-parent-folder", childFolder);

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { parentFolder }.GetEnumerator());

        var result = await _reader.GetManifestsAsync();
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("Nested Extension", result.First().Name);
    }

    [Test]
    public async Task CanSkipEmptyDirectories()
    {
        var extensionFolder = CreateDirectoryMock("/my-extension-folder", CreateExtensionManifestFile(DefaultManifestContent("My Extension")));
        var emptyFolder = CreateDirectoryMock("/my-empty-folder");

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { emptyFolder, extensionFolder }.GetEnumerator());

        var result = await _reader.GetManifestsAsync();
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("My Extension", result.First().Name);
    }

    [Test]
    public async Task CanSkipOtherFiles()
    {
        var extensionFolder = CreateDirectoryMock(
            "/my-extension-folder",
            CreateOtherFile("my.js"),
            CreateExtensionManifestFile(DefaultManifestContent("My Extension")));
        var otherFolder = CreateDirectoryMock(
            "/my-empty-folder",
            CreateOtherFile("some.js"),
            CreateOtherFile("some.css"));

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { otherFolder, extensionFolder }.GetEnumerator());

        var result = await _reader.GetManifestsAsync();
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("My Extension", result.First().Name);
    }

    [Test]
    public async Task CanHandleAllEmptyDirectories()
    {
        var folders = Enumerable.Range(1, 10).Select(i => CreateDirectoryMock($"/my-empty-folder-{i}")).ToList();

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(folders.GetEnumerator());

        var result = await _reader.GetManifestsAsync();
        Assert.AreEqual(0, result.Count());
    }

    [Test]
    public async Task CannotGetManifestWithoutName()
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
            .Returns(new List<IFileInfo> { CreateExtensionManifestFile(content) }.GetEnumerator());

        var result = await _reader.GetManifestsAsync();
        Assert.AreEqual(0, result.Count());

        EnsureLogErrorWasCalled();
    }

    [Test]
    public async Task CannotGetManifestWithoutExtensions()
    {
        var content = @"{
    ""name"": ""Something"",
    ""version"": ""1.2.3"",
    ""allowTelemetry"": true
}";
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { CreateExtensionManifestFile(content) }.GetEnumerator());

        var result = await _reader.GetManifestsAsync();
        Assert.AreEqual(0, result.Count());

        EnsureLogErrorWasCalled();
    }

    [TestCase("This is not JSON")]
    [TestCase(@"{""name"": ""invalid-json"", ""version"": ")]
    public async Task CannotGetInvalidManifest(string content)
    {
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { CreateExtensionManifestFile(content) }.GetEnumerator());

        var result = await _reader.GetManifestsAsync();
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

    private IFileInfo CreateExtensionManifestFile(string? content = null)
    {
        content ??= DefaultManifestContent();

        var fileInfo = new Mock<IFileInfo>();
        fileInfo.SetupGet(f => f.IsDirectory).Returns(false);
        fileInfo.SetupGet(f => f.Name).Returns("extension.json");
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

    private static string DefaultManifestContent(string name = "My Extension Manifest")
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
