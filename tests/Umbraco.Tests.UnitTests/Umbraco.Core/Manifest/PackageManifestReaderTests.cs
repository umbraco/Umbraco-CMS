using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Routing;
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
        _reader = new AppPluginsPackageManifestReader(
            fileProviderFactoryMock.Object,
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            _loggerMock.Object);
    }

    // ---------------------------------------------------------------------
    // umbraco-package.json source
    // ---------------------------------------------------------------------

    [Test]
    public async Task Can_Read_PackageManifest_In_Root_Directories()
    {
        var directoryOne = CreatePackageFolderWithManifest("my-extension", DefaultPackageManifestContent("Package One"));
        var directoryTwo = CreatePackageFolderWithManifest("my-other-extension", DefaultPackageManifestContent("Package Two"));
        SetRootContents(directoryOne, directoryTwo);

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Package One", result[0].Name);
        Assert.AreEqual("Package Two", result[1].Name);
    }

    [Test]
    public async Task Can_Read_Importmap_From_Manifest()
    {
        var packageFolder = CreatePackageFolderWithManifest("my-extension", DefaultPackageManifestContent());
        SetRootContents(packageFolder);

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();

        Assert.AreEqual(1, result.Count);
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
        var packageFolder = CreatePackageFolderWithManifest("my-extension", content);
        SetRootContents(packageFolder);

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();

        Assert.AreEqual(1, result.Count);

        var first = result.First();
        Assert.IsTrue(first.Extensions.All(e => e is JsonObject));

        JsonObject firstExtension = (JsonObject)first.Extensions.First();
        Assert.AreEqual("tree", firstExtension["type"].GetValue<string>());
        var meta = firstExtension["meta"];
        Assert.AreEqual("My Tree", meta["label"].GetValue<string>());
        var someArray = meta["someArray"];
        Assert.AreEqual(1, someArray[0].GetValue<int>());
    }

    [Test]
    public async Task Can_Skip_Empty_Directories()
    {
        var packageFolder = CreatePackageFolderWithManifest("my-package-folder", DefaultPackageManifestContent());
        var emptyFolder = CreateEmptyPackageFolder("my-empty-folder");
        SetRootContents(emptyFolder, packageFolder);

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("My Package", result.First().Name);
    }

    [Test]
    public async Task Can_Skip_Non_Package_Json_Files_In_Package_Folder()
    {
        var packageFolder = CreatePackageFolderWithManifestAndExtras(
            "my-package-folder",
            DefaultPackageManifestContent(),
            CreateFile("my.js"));
        var otherFolder = CreateEmptyPackageFolder("my-other-folder", CreateFile("some.js"), CreateFile("some.css"));
        SetRootContents(otherFolder, packageFolder);

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("My Package", result.First().Name);
    }

    [Test]
    public async Task Can_Handle_All_Empty_Directories()
    {
        var folders = Enumerable.Range(1, 10)
            .Select(i => CreateEmptyPackageFolder($"my-empty-folder-{i}"))
            .ToArray();
        SetRootContents(folders);

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();

        Assert.AreEqual(0, result.Count);
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
        var packageFolder = CreatePackageFolderWithManifest("my-extension", content);
        SetRootContents(packageFolder);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _reader.ReadPackageManifestsAsync());
        Assert.NotNull(exception);
        Assert.IsInstanceOf<JsonException>(exception.InnerException);
    }

    [Test]
    public void Cannot_Read_PackageManifest_Without_Extensions()
    {
        const string content = @"{
    ""name"": ""Something"",
    ""version"": ""1.2.3"",
    ""allowTelemetry"": true
}";
        var packageFolder = CreatePackageFolderWithManifest("my-extension", content);
        SetRootContents(packageFolder);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _reader.ReadPackageManifestsAsync());
        Assert.NotNull(exception);
        Assert.IsInstanceOf<JsonException>(exception.InnerException);
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
        var packageFolder = CreatePackageFolderWithManifest("my-extension", content);
        SetRootContents(packageFolder);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _reader.ReadPackageManifestsAsync());
        Assert.NotNull(exception);
        Assert.IsInstanceOf<JsonException>(exception.InnerException);
    }

    [TestCase("This is not JSON")]
    [TestCase(@"{""name"": ""invalid-json"", ""version"": ")]
    public void Cannot_Read_Invalid_PackageManifest(string content)
    {
        var packageFolder = CreatePackageFolderWithManifest("my-extension", content);
        SetRootContents(packageFolder);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _reader.ReadPackageManifestsAsync());
        Assert.NotNull(exception);
        Assert.IsInstanceOf<JsonException>(exception.InnerException);
    }

    // ---------------------------------------------------------------------
    // extensions/ folder source
    // ---------------------------------------------------------------------

    [Test]
    public async Task Can_Discover_Extensions_In_Extensions_Folder()
    {
        var packageFolder = CreatePackageFolderWithExtensions("MyPackage", "my-dashboard.js");
        SetRootContents(packageFolder);

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();
        Assert.AreEqual(1, result.Count);

        var manifest = result.First();
        Assert.AreEqual("MyPackage", manifest.Name);
        Assert.IsNull(manifest.Version);
        Assert.AreEqual(1, manifest.Extensions.Length);

        var extension = manifest.Extensions.First();
        Assert.AreEqual("bundle", GetProperty(extension, "type"));
        Assert.AreEqual("MyPackage.Extensions.Bundle.my-dashboard", GetProperty(extension, "alias"));
        Assert.That(GetProperty(extension, "js")!.ToString(), Does.Contain("/App_Plugins/MyPackage/extensions/my-dashboard.js"));
        Assert.That(GetProperty(extension, "js")!.ToString(), Does.Contain("?v=%CACHE_BUSTER%"));
    }

    [Test]
    public async Task Can_Discover_Multiple_Extensions_In_One_Package()
    {
        var packageFolder = CreatePackageFolderWithExtensions("MyPackage", "dashboard.js", "action.js", "editor.js");
        SetRootContents(packageFolder);

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(3, result.First().Extensions.Length);

        var aliases = result.First().Extensions
            .Select(e => GetProperty(e, "alias")!.ToString())
            .ToList();

        Assert.That(aliases, Does.Contain("MyPackage.Extensions.Bundle.action"));
        Assert.That(aliases, Does.Contain("MyPackage.Extensions.Bundle.dashboard"));
        Assert.That(aliases, Does.Contain("MyPackage.Extensions.Bundle.editor"));
    }

    [Test]
    public async Task Can_Discover_Extensions_From_Multiple_Packages()
    {
        var packageOne = CreatePackageFolderWithExtensions("PackageOne", "dashboard.js");
        var packageTwo = CreatePackageFolderWithExtensions("PackageTwo", "editor.js");
        SetRootContents(packageOne, packageTwo);

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("PackageOne", result[0].Name);
        Assert.AreEqual("PackageTwo", result[1].Name);
    }

    [Test]
    public async Task Can_Skip_Packages_Without_Extensions_Folder()
    {
        var packageWithExtensions = CreatePackageFolderWithExtensions("WithExtensions", "dashboard.js");
        var packageWithoutExtensions = CreateEmptyPackageFolder("WithoutExtensions");
        SetRootContents(packageWithoutExtensions, packageWithExtensions);

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("WithExtensions", result.First().Name);
    }

    [Test]
    public async Task Can_Skip_Non_Js_Files_In_Extensions_Folder()
    {
        var packageFolder = CreatePackageFolderWithExtensions(
            "MyPackage",
            "dashboard.js",
            "readme.md",
            "styles.css",
            "types.d.ts");
        SetRootContents(packageFolder);

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result.First().Extensions.Length);
        Assert.That(GetProperty(result.First().Extensions.First(), "js")!.ToString(), Does.Contain("dashboard.js"));
    }

    [Test]
    public async Task Can_Skip_Subdirectories_In_Extensions_Folder()
    {
        var packageFolder = CreatePackageFolderWithExtensionFiles(
            "MyPackage",
            CreateDirectory("subfolder"),
            CreateFile("dashboard.js"));
        SetRootContents(packageFolder);

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result.First().Extensions.Length);
    }

    [Test]
    public async Task Can_Skip_Empty_Extensions_Folder()
    {
        var packageFolder = CreatePackageFolderWithExtensions("MyPackage");
        SetRootContents(packageFolder);

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();

        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public async Task Can_Handle_No_Package_Folders()
    {
        SetRootContents();

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();

        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public async Task Can_Skip_Root_Level_Files()
    {
        var rootFile = CreateFile("some-file.js");
        var packageFolder = CreatePackageFolderWithExtensions("MyPackage", "dashboard.js");
        SetRootContents(rootFile, packageFolder);

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("MyPackage", result.First().Name);
    }

    [Test]
    public async Task Generates_Correct_Js_Paths()
    {
        var packageFolder = CreatePackageFolderWithExtensions("My.Package", "my-extension.js");
        SetRootContents(packageFolder);

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();

        Assert.AreEqual(
            "/App_Plugins/My.Package/extensions/my-extension.js?v=%CACHE_BUSTER%",
            GetProperty(result.First().Extensions.First(), "js")!.ToString());
    }

    // ---------------------------------------------------------------------
    // Both sources together
    // ---------------------------------------------------------------------

    [Test]
    public async Task Produces_Only_PackageManifest_When_Package_Has_Both_PackageManifest_And_Extensions_Folder()
    {
        // Package has BOTH an umbraco-package.json AND an extensions/ folder.
        // The reader is NOT additive — it prioritizes the package manifest over the bundle manifest.
        var packageFolder = CreatePackageFolderWithManifestAndExtensions(
            "Hybrid",
            DefaultPackageManifestContent("Hybrid Package"),
            "dashboard.js");
        SetRootContents(packageFolder);

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();

        Assert.AreEqual(1, result.Count);

        var manifest = result.Single(m => m.Name == "Hybrid Package");
        Assert.AreEqual("1.2.3", manifest.Version);
        Assert.AreEqual(2, manifest.Extensions.Length);
    }

    // ---------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------

    private void SetRootContents(params IFileInfo[] items) =>
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(((IEnumerable<IFileInfo>)items).GetEnumerator());

    private IFileInfo CreatePackageFolderWithManifest(string packageName, string manifestContent)
    {
        SetupPackageContents(packageName, CreateManifestFile(manifestContent));
        return CreateDirectory(packageName);
    }

    private IFileInfo CreatePackageFolderWithManifestAndExtras(
        string packageName,
        string manifestContent,
        params IFileInfo[] extras)
    {
        IFileInfo[] contents = [.. extras, CreateManifestFile(manifestContent)];
        SetupPackageContents(packageName, contents);
        return CreateDirectory(packageName);
    }

    private IFileInfo CreatePackageFolderWithExtensions(string packageName, params string[] jsFileNames)
    {
        IFileInfo[] extensionFiles = jsFileNames.Select(n => CreateFile(n)).ToArray();
        return CreatePackageFolderWithExtensionFiles(packageName, extensionFiles);
    }

    private IFileInfo CreatePackageFolderWithExtensionFiles(string packageName, params IFileInfo[] extensionFiles)
    {
        SetupPackageContents(packageName, CreateDirectory("extensions"));
        SetupExtensionsContents(packageName, extensionFiles);
        return CreateDirectory(packageName);
    }

    private IFileInfo CreatePackageFolderWithManifestAndExtensions(
        string packageName,
        string manifestContent,
        params string[] jsFileNames)
    {
        SetupPackageContents(packageName, CreateManifestFile(manifestContent), CreateDirectory("extensions"));
        SetupExtensionsContents(packageName, jsFileNames.Select(n => CreateFile(n)).ToArray());
        return CreateDirectory(packageName);
    }

    private IFileInfo CreateEmptyPackageFolder(string packageName, params IFileInfo[] contents)
    {
        SetupPackageContents(packageName, contents);
        return CreateDirectory(packageName);
    }

    private void SetupPackageContents(string packageName, params IFileInfo[] contents)
    {
        var mock = new Mock<IDirectoryContents>();
        mock.Setup(f => f.GetEnumerator()).Returns(((IEnumerable<IFileInfo>)contents).GetEnumerator());
        var packagePath = WebPath.Combine(Constants.SystemDirectories.AppPlugins, packageName);
        _fileProviderMock.Setup(m => m.GetDirectoryContents(packagePath)).Returns(mock.Object);
    }

    private void SetupExtensionsContents(string packageName, params IFileInfo[] files)
    {
        var mock = new Mock<IDirectoryContents>();
        mock.SetupGet(d => d.Exists).Returns(true);
        mock.Setup(f => f.GetEnumerator()).Returns(((IEnumerable<IFileInfo>)files).GetEnumerator());
        var extensionsPath = WebPath.Combine(Constants.SystemDirectories.AppPlugins, packageName, "extensions");
        _fileProviderMock.Setup(m => m.GetDirectoryContents(extensionsPath)).Returns(mock.Object);
    }

    private static IFileInfo CreateManifestFile(string content)
    {
        var fileInfo = new Mock<IFileInfo>();
        fileInfo.SetupGet(f => f.IsDirectory).Returns(false);
        fileInfo.SetupGet(f => f.Name).Returns("umbraco-package.json");
        fileInfo.Setup(f => f.CreateReadStream()).Returns(() => new MemoryStream(Encoding.UTF8.GetBytes(content)));
        return fileInfo.Object;
    }

    private static IFileInfo CreateFile(string name)
    {
        var fileInfo = new Mock<IFileInfo>();
        fileInfo.SetupGet(f => f.IsDirectory).Returns(false);
        fileInfo.SetupGet(f => f.Name).Returns(name);
        return fileInfo.Object;
    }

    private static IFileInfo CreateDirectory(string name)
    {
        var fileInfo = new Mock<IFileInfo>();
        fileInfo.SetupGet(f => f.IsDirectory).Returns(true);
        fileInfo.SetupGet(f => f.Name).Returns(name);
        return fileInfo.Object;
    }

    private static object? GetProperty(object obj, string name) =>
        obj.GetType().GetProperty(name)?.GetValue(obj);

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
