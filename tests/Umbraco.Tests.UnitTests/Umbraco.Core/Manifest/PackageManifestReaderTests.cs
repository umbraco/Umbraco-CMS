using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Infrastructure.Manifest;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Manifest;

[TestFixture]
public class PackageManifestReaderTests
{
    private IPackageManifestReader _reader;
    private Mock<IDirectoryContents> _rootDirectoryContentsMock;
    private Mock<ILogger<AppPluginsPackageManifestReader>> _loggerMock;
    private Mock<IFileProvider> _fileProviderMock;

    [SetUp]
    public void SetUp()
    {
        _rootDirectoryContentsMock = new Mock<IDirectoryContents>();
        _fileProviderMock = new Mock<IFileProvider>();
        _fileProviderMock
            .Setup(m => m.GetDirectoryContents(Constants.SystemDirectories.AppPlugins))
            .Returns(_rootDirectoryContentsMock.Object);
        var fileProviderFactoryMock = new Mock<IPackageManifestFileProviderFactory>();
        fileProviderFactoryMock.Setup(m => m.Create()).Returns(_fileProviderMock.Object);

        _loggerMock = new Mock<ILogger<AppPluginsPackageManifestReader>>();
        _reader = new AppPluginsPackageManifestReader(fileProviderFactoryMock.Object, new SystemTextJsonSerializer(), _loggerMock.Object);
    }

    [Test]
    public async Task Can_Read_PackageManifests_At_Root()
    {
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { CreatePackageManifestFile() }.GetEnumerator());

        var result = await _reader.ReadPackageManifestsAsync();
        Assert.AreEqual(1, result.Count());

        var first = result.First();
        Assert.AreEqual("My Package", first.Name);
        Assert.AreEqual("1.2.3", first.Version);
        Assert.AreEqual(2, first.Extensions.Length);

        Assert.NotNull(first.Importmap);
        var importmap = first.Importmap;
        Assert.AreEqual(1, importmap.Imports.Count());
        Assert.AreEqual("./module/shapes/square.js", importmap.Imports["square"]);

        Assert.NotNull(importmap.Scopes);
        Assert.AreEqual(1, importmap.Scopes.Count());
        var scope = importmap.Scopes.First();
        Assert.AreEqual("/modules/customshapes", scope.Key);
        Assert.NotNull(scope.Value);
        var firstScope = scope.Value.First();
        Assert.NotNull(firstScope);
        Assert.AreEqual("square", firstScope.Key);
        Assert.AreEqual("https://example.com/modules/shapes/square.js", firstScope.Value);
    }

    [Test]
    public async Task Can_Deserialize_Extensions()
    {
        const string content = @"{
    ""name"": ""My Package"",
    ""version"": ""1.2.3"",
    ""allowTelemetry"": true,
    ""extensions"": [{
            ""type"": ""tree"",
            ""meta"": {
                ""label"": ""My Tree"",
                ""someArray"": [1, 2, 3]
            }
        }, {
            ""type"": ""headerApp""
        }
    ]
    }";
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { CreatePackageManifestFile(content) }.GetEnumerator());

        var result = await _reader.ReadPackageManifestsAsync();
        Assert.AreEqual(1, result.Count());

        var first = result.First();

        // Ensure that the extensions are deserialized as JsonElement
        Assert.IsTrue(first.Extensions.All(e => e is JsonObject));

        // Test the deserialization of the first extension to make sure we don't break the JSON parsing
        JsonObject firstExtension = (JsonObject)first.Extensions.First();
         Assert.AreEqual("tree", firstExtension["type"].GetValue<string>());
         var meta = firstExtension["meta"];
         Assert.AreEqual("My Tree", meta["label"].GetValue<string>());
         var someArray = meta["someArray"];
         Assert.AreEqual(1, someArray[0].GetValue<int>());
    }

    [Test]
    public async Task Can_Read_PackageManifest_In_Root_Directories()
    {
        var directoryOne = CreateDirectoryMock("/my-extension", CreatePackageManifestFile(DefaultPackageManifestContent("Package One")));
        var directoryTwo = CreateDirectoryMock("/my-other-extension", CreatePackageManifestFile(DefaultPackageManifestContent("Package Two")));
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { directoryOne, directoryTwo }.GetEnumerator());

        var result = await _reader.ReadPackageManifestsAsync();
        Assert.AreEqual(2, result.Count());
        Assert.AreEqual("Package One", result.First().Name);
        Assert.AreEqual("Package Two", result.Last().Name);
    }

    [Test]
    public async Task Can_Skip_Empty_Directories()
    {
        var packageFolder = CreateDirectoryMock("/my-package-folder", CreatePackageManifestFile(DefaultPackageManifestContent("My Package")));
        var emptyFolder = CreateDirectoryMock("/my-empty-folder");

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { emptyFolder, packageFolder }.GetEnumerator());

        var result = await _reader.ReadPackageManifestsAsync();
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("My Package", result.First().Name);
    }

    [Test]
    public async Task Can_Skip_Other_Files()
    {
        var packageFolder = CreateDirectoryMock(
            "/my-package-folder",
            CreateOtherFile("my.js"),
            CreatePackageManifestFile(DefaultPackageManifestContent("My Package")));
        var otherFolder = CreateDirectoryMock(
            "/my-empty-folder",
            CreateOtherFile("some.js"),
            CreateOtherFile("some.css"));

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { otherFolder, packageFolder }.GetEnumerator());

        var result = await _reader.ReadPackageManifestsAsync();
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("My Package", result.First().Name);
    }

    [Test]
    public async Task Can_Handle_All_Empty_Directories()
    {
        var folders = Enumerable.Range(1, 10).Select(i => CreateDirectoryMock($"/my-empty-folder-{i}")).ToList();

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(folders.GetEnumerator());

        var result = await _reader.ReadPackageManifestsAsync();
        Assert.AreEqual(0, result.Count());
    }

    [Test]
    public void Cannot_Read_PackageManifest_Without_Name()
    {
        const string content = @"{
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
            .Returns(new List<IFileInfo> { CreatePackageManifestFile(content) }.GetEnumerator());

        Assert.ThrowsAsync<JsonException>(() => _reader.ReadPackageManifestsAsync());
        EnsureLogErrorWasCalled();
    }

    [Test]
    public void Cannot_Read_PackageManifest_Without_Extensions()
    {
        const string content = @"{
    ""name"": ""Something"",
    ""version"": ""1.2.3"",
    ""allowTelemetry"": true
}";
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { CreatePackageManifestFile(content) }.GetEnumerator());

        Assert.ThrowsAsync<JsonException>(() => _reader.ReadPackageManifestsAsync());
        EnsureLogErrorWasCalled();
    }

    [Test]
    public void Cannot_Read_PackageManifest_Without_Importmap_Imports()
    {
        const string content = @"{
    ""name"": ""Something"",
    ""extensions"": [],
    ""importmap"": {
        ""scopes"": {
            ""/modules/customshapes"": {
                ""square"": ""https://example.com/modules/shapes/square.js""
            }
        }
    }
}";
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { CreatePackageManifestFile(content) }.GetEnumerator());

        Assert.ThrowsAsync<JsonException>(() => _reader.ReadPackageManifestsAsync());
        EnsureLogErrorWasCalled();
    }

    [TestCase("This is not JSON")]
    [TestCase(@"{""name"": ""invalid-json"", ""version"": ")]
    public void Cannot_Read_Invalid_PackageManifest(string content)
    {
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { CreatePackageManifestFile(content) }.GetEnumerator());

        Assert.ThrowsAsync<JsonException>(() => _reader.ReadPackageManifestsAsync());
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

    private IFileInfo CreatePackageManifestFile(string? content = null)
    {
        content ??= DefaultPackageManifestContent();

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

    private static string DefaultPackageManifestContent(string name = "My Package")
        => @"{
    ""name"": ""##NAME##"",
    ""version"": ""1.2.3"",
    ""allowTelemetry"": true,
    ""extensions"": [{
            ""type"": ""tree""
        }, {
            ""type"": ""headerApp""
        }
    ],
    ""importmap"": {
        ""imports"": {
            ""square"": ""./module/shapes/square.js""
        },
        ""scopes"": {
            ""/modules/customshapes"": {
                ""square"": ""https://example.com/modules/shapes/square.js""
            }
        }
    }
}".Replace("##NAME##", name);
}
