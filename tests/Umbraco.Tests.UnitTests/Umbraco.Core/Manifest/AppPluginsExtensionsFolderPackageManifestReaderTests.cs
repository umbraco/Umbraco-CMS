using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Infrastructure.Manifest;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Manifest;

[TestFixture]
public class AppPluginsExtensionsFolderPackageManifestReaderTests
{
    private IPackageManifestReader _reader;
    private Mock<IFileProvider> _fileProviderMock;
    private Mock<IDirectoryContents> _rootDirectoryContentsMock;

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

        var loggerMock = new Mock<ILogger<AppPluginsExtensionsFolderPackageManifestReader>>();

        _reader = new AppPluginsExtensionsFolderPackageManifestReader(
            fileProviderFactoryMock.Object,
            loggerMock.Object);
    }

    [Test]
    public async Task Can_Discover_Extensions_In_Extensions_Folder()
    {
        var packageFolder = CreatePackageFolderWithExtensions("MyPackage", "my-dashboard.js");

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { packageFolder }.GetEnumerator());

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();
        Assert.AreEqual(1, result.Count);

        var manifest = result.First();
        Assert.AreEqual("MyPackage", manifest.Name);
        Assert.IsNull(manifest.Version);
        Assert.AreEqual(1, manifest.Extensions.Length);

        var extension = manifest.Extensions.First();
        Assert.AreEqual("bundle", GetProperty(extension, "type"));
        Assert.AreEqual("MyPackage.Extensions.Bundle.0", GetProperty(extension, "alias"));
        Assert.That(GetProperty(extension, "js")!.ToString(), Does.Contain("/App_Plugins/MyPackage/extensions/my-dashboard.js"));
        Assert.That(GetProperty(extension, "js")!.ToString(), Does.Contain("?v=%CACHE_BUSTER%"));
    }

    [Test]
    public async Task Can_Discover_Multiple_Extensions_In_One_Package()
    {
        var packageFolder = CreatePackageFolderWithExtensions("MyPackage", "dashboard.js", "action.js", "editor.js");

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { packageFolder }.GetEnumerator());

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(3, result.First().Extensions.Length);

        var aliases = result.First().Extensions
            .Select(e => GetProperty(e, "alias")!.ToString())
            .ToList();

        Assert.That(aliases, Does.Contain("MyPackage.Extensions.Bundle.0"));
        Assert.That(aliases, Does.Contain("MyPackage.Extensions.Bundle.1"));
        Assert.That(aliases, Does.Contain("MyPackage.Extensions.Bundle.2"));
    }

    [Test]
    public async Task Can_Discover_Extensions_From_Multiple_Packages()
    {
        var packageOne = CreatePackageFolderWithExtensions("PackageOne", "dashboard.js");
        var packageTwo = CreatePackageFolderWithExtensions("PackageTwo", "editor.js");

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { packageOne, packageTwo }.GetEnumerator());

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("PackageOne", result[0].Name);
        Assert.AreEqual("PackageTwo", result[1].Name);
    }

    [Test]
    public async Task Can_Skip_Packages_Without_Extensions_Folder()
    {
        var packageWithExtensions = CreatePackageFolderWithExtensions("WithExtensions", "dashboard.js");
        var packageWithoutExtensions = CreatePackageFolder("WithoutExtensions");

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { packageWithoutExtensions, packageWithExtensions }.GetEnumerator());

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("WithExtensions", result.First().Name);
    }

    [Test]
    public async Task Can_Skip_Non_Js_Files_In_Extensions_Folder()
    {
        var packageFolder = CreatePackageFolderWithExtensions("MyPackage", "dashboard.js", "readme.md", "styles.css", "types.d.ts");

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { packageFolder }.GetEnumerator());

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result.First().Extensions.Length);

        Assert.That(GetProperty(result.First().Extensions.First(), "js")!.ToString(), Does.Contain("dashboard.js"));
    }

    [Test]
    public async Task Can_Skip_Subdirectories_In_Extensions_Folder()
    {
        var subDirectory = CreateFileInfoMock("subfolder", isDirectory: true);
        var jsFile = CreateFileInfoMock("dashboard.js");
        var packageFolder = CreatePackageFolderWithExtensionFiles("MyPackage", subDirectory, jsFile);

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { packageFolder }.GetEnumerator());

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result.First().Extensions.Length);
    }

    [Test]
    public async Task Can_Skip_Empty_Extensions_Folder()
    {
        var packageFolder = CreatePackageFolderWithExtensions("MyPackage");

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { packageFolder }.GetEnumerator());

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public async Task Can_Handle_No_Package_Folders()
    {
        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo>().GetEnumerator());

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public async Task Can_Skip_Root_Level_Files()
    {
        var rootFile = CreateFileInfoMock("some-file.js");
        var packageFolder = CreatePackageFolderWithExtensions("MyPackage", "dashboard.js");

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { rootFile, packageFolder }.GetEnumerator());

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("MyPackage", result.First().Name);
    }

    [Test]
    public async Task Can_Skip_Packages_With_Existing_UmbracoPackageJson()
    {
        var packageWithJson = CreatePackageFolderWithExtensionsAndManifest("HasJson", "dashboard.js");
        var packageWithoutJson = CreatePackageFolderWithExtensions("NoJson", "editor.js");

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { packageWithJson, packageWithoutJson }.GetEnumerator());

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("NoJson", result.First().Name);
    }

    [Test]
    public async Task Generates_Correct_Js_Paths()
    {
        var packageFolder = CreatePackageFolderWithExtensions("My.Package", "my-extension.js");

        _rootDirectoryContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { packageFolder }.GetEnumerator());

        var result = (await _reader.ReadPackageManifestsAsync()).ToList();
        Assert.AreEqual(
            "/App_Plugins/My.Package/extensions/my-extension.js?v=%CACHE_BUSTER%",
            GetProperty(result.First().Extensions.First(), "js")!.ToString());
    }

    private static object? GetProperty(object obj, string name) =>
        obj.GetType().GetProperty(name)?.GetValue(obj);

    private IFileInfo CreatePackageFolderWithExtensions(string packageName, params string[] jsFileNames)
    {
        var extensionFiles = jsFileNames.Select(name => CreateFileInfoMock(name)).ToArray();
        return CreatePackageFolderWithExtensionFiles(packageName, extensionFiles);
    }

    private IFileInfo CreatePackageFolderWithExtensionsAndManifest(string packageName, params string[] jsFileNames)
    {
        var result = CreatePackageFolderWithExtensionFiles(packageName, jsFileNames.Select(name => CreateFileInfoMock(name)).ToArray());

        // Override the package folder contents to include an umbraco-package.json
        var packagePath = WebPath.Combine(Constants.SystemDirectories.AppPlugins, packageName);
        var packageContentsMock = new Mock<IDirectoryContents>();
        packageContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo> { CreateFileInfoMock("umbraco-package.json") }.GetEnumerator());
        _fileProviderMock
            .Setup(m => m.GetDirectoryContents(packagePath))
            .Returns(packageContentsMock.Object);

        return result;
    }

    private IFileInfo CreatePackageFolderWithExtensionFiles(string packageName, params IFileInfo[] extensionFiles)
    {
        // Set up the package folder contents (no umbraco-package.json by default)
        var packagePath = WebPath.Combine(Constants.SystemDirectories.AppPlugins, packageName);
        var packageContentsMock = new Mock<IDirectoryContents>();
        packageContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo>().GetEnumerator());
        _fileProviderMock
            .Setup(m => m.GetDirectoryContents(packagePath))
            .Returns(packageContentsMock.Object);

        // Set up the extensions subfolder contents
        var extensionsContentsMock = new Mock<IDirectoryContents>();
        extensionsContentsMock.SetupGet(d => d.Exists).Returns(true);
        extensionsContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(extensionFiles.ToList().GetEnumerator());

        var extensionsPath = WebPath.Combine(Constants.SystemDirectories.AppPlugins, packageName, "extensions");
        _fileProviderMock
            .Setup(m => m.GetDirectoryContents(extensionsPath))
            .Returns(extensionsContentsMock.Object);

        return CreateDirectoryFileInfo(packageName);
    }

    private IFileInfo CreatePackageFolder(string packageName)
    {
        // Set up the package folder contents (no umbraco-package.json)
        var packagePath = WebPath.Combine(Constants.SystemDirectories.AppPlugins, packageName);
        var packageContentsMock = new Mock<IDirectoryContents>();
        packageContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo>().GetEnumerator());
        _fileProviderMock
            .Setup(m => m.GetDirectoryContents(packagePath))
            .Returns(packageContentsMock.Object);

        // Set up non-existent extensions subfolder
        var emptyContentsMock = new Mock<IDirectoryContents>();
        emptyContentsMock.SetupGet(d => d.Exists).Returns(false);
        emptyContentsMock
            .Setup(f => f.GetEnumerator())
            .Returns(new List<IFileInfo>().GetEnumerator());

        var extensionsPath = WebPath.Combine(Constants.SystemDirectories.AppPlugins, packageName, "extensions");
        _fileProviderMock
            .Setup(m => m.GetDirectoryContents(extensionsPath))
            .Returns(emptyContentsMock.Object);

        return CreateDirectoryFileInfo(packageName);
    }

    private static IFileInfo CreateDirectoryFileInfo(string name)
    {
        var fileInfo = new Mock<IFileInfo>();
        fileInfo.SetupGet(f => f.IsDirectory).Returns(true);
        fileInfo.SetupGet(f => f.Name).Returns(name);
        return fileInfo.Object;
    }

    private static IFileInfo CreateFileInfoMock(string name, bool isDirectory = false)
    {
        var fileInfo = new Mock<IFileInfo>();
        fileInfo.SetupGet(f => f.IsDirectory).Returns(isDirectory);
        fileInfo.SetupGet(f => f.Name).Returns(name);
        return fileInfo.Object;
    }
}
