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
        _reader = new AppPluginsPackageManifestReader(fileProviderFactoryMock.Object, new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()), _loggerMock.Object);
    }

    [Test]
    public async Task Can_Read_PackageManifests_At_Root()
    {
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { CreatePackageManifestFile() }.GetEnumerator());

        var result = await _reader.ReadPackageManifestsAsync();
        Assert.That(result.Count(), Is.EqualTo(1));

        var first = result.First();
        Assert.That(first.Name, Is.EqualTo("My Package"));
        Assert.That(first.Version, Is.EqualTo("1.2.3"));
        Assert.That(first.Extensions.Length, Is.EqualTo(2));

        Assert.That(first.Importmap, Is.Not.Null);
        var importmap = first.Importmap;
        Assert.That(importmap.Imports.Count(), Is.EqualTo(1));
        Assert.That(importmap.Imports["square"], Is.EqualTo("./module/shapes/square.js"));

        Assert.That(importmap.Scopes, Is.Not.Null);
        Assert.That(importmap.Scopes.Count(), Is.EqualTo(1));
        var scope = importmap.Scopes.First();
        Assert.That(scope.Key, Is.EqualTo("/modules/customshapes"));
        Assert.That(scope.Value, Is.Not.Null);
        var firstScope = scope.Value.First();
        Assert.That(firstScope, Is.Not.Null);
        Assert.That(firstScope.Key, Is.EqualTo("square"));
        Assert.That(firstScope.Value, Is.EqualTo("https://example.com/modules/shapes/square.js"));
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
        Assert.That(result.Count(), Is.EqualTo(1));

        var first = result.First();

        // Ensure that the extensions are deserialized as JsonElement
        Assert.That(first.Extensions.All(e => e is JsonObject), Is.True);

        // Test the deserialization of the first extension to make sure we don't break the JSON parsing
        JsonObject firstExtension = (JsonObject)first.Extensions.First();
        Assert.That(firstExtension["type"].GetValue<string>(), Is.EqualTo("tree"));
         var meta = firstExtension["meta"];
        Assert.That(meta["label"].GetValue<string>(), Is.EqualTo("My Tree"));
         var someArray = meta["someArray"];
        Assert.That(someArray[0].GetValue<int>(), Is.EqualTo(1));
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
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Name, Is.EqualTo("Package One"));
        Assert.That(result.Last().Name, Is.EqualTo("Package Two"));
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
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("My Package"));
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
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("My Package"));
    }

    [Test]
    public async Task Can_Handle_All_Empty_Directories()
    {
        var folders = Enumerable.Range(1, 10).Select(i => CreateDirectoryMock($"/my-empty-folder-{i}")).ToList();

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(folders.GetEnumerator());

        var result = await _reader.ReadPackageManifestsAsync();
        Assert.That(result.Count(), Is.EqualTo(0));
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

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _reader.ReadPackageManifestsAsync());
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.InnerException, Is.InstanceOf<JsonException>());
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

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _reader.ReadPackageManifestsAsync());
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.InnerException, Is.InstanceOf<JsonException>());
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

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _reader.ReadPackageManifestsAsync());
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.InnerException, Is.InstanceOf<JsonException>());
    }

    [TestCase("This is not JSON")]
    [TestCase(@"{""name"": ""invalid-json"", ""version"": ")]
    public void Cannot_Read_Invalid_PackageManifest(string content)
    {
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { CreatePackageManifestFile(content) }.GetEnumerator());

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _reader.ReadPackageManifestsAsync());
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.InnerException, Is.InstanceOf<JsonException>());
    }

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
