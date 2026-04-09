// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.None)]
internal sealed class PartialViewRepositoryTests : UmbracoIntegrationTest
{
    [SetUp]
    public void SetUp() =>
        _fileSystem = new PhysicalFileSystem(
            IOHelper,
            HostingEnvironment,
            LoggerFactory.CreateLogger<PhysicalFileSystem>(),
            HostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.PartialViews),
            HostingEnvironment.ToAbsolute(Constants.SystemDirectories.PartialViews));

    [TearDown]
    public void TearDownFiles()
    {
        // Delete all files
        Purge((PhysicalFileSystem)_fileSystem, string.Empty);
        _fileSystem = null;
    }

    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    private IOptionsMonitor<RuntimeSettings> RuntimeSettings => GetRequiredService<IOptionsMonitor<RuntimeSettings>>();

    private IFileSystem _fileSystem;

    private IOptionsMonitor<RuntimeSettings> CreateRuntimeSettingsMonitor(RuntimeMode mode)
    {
        var settings = new RuntimeSettings { Mode = mode };
        var monitor = new Mock<IOptionsMonitor<RuntimeSettings>>();
        monitor.Setup(m => m.CurrentValue).Returns(settings);
        return monitor.Object;
    }

    [Test]
    public void PathTests()
    {
        // unless noted otherwise, no changes / 7.2.8
        var fileSystems = FileSystemsCreator.CreateTestFileSystems(
            LoggerFactory,
            IOHelper,
            GetRequiredService<IOptions<GlobalSettings>>(),
            HostingEnvironment,
            _fileSystem,
            null,
            null,
            null);

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = new PartialViewRepository(fileSystems, RuntimeSettings);

            IPartialView partialView =
                new PartialView("test-path-1.cshtml") { Content = "// partialView" };
            repository.Save(partialView);
            Assert.IsTrue(_fileSystem.FileExists("test-path-1.cshtml"));
            Assert.AreEqual("test-path-1.cshtml", partialView.Path);
            Assert.AreEqual("/Views/Partials/test-path-1.cshtml", partialView.VirtualPath);

            partialView =
                new PartialView("path-2/test-path-2.cshtml") { Content = "// partialView" };
            repository.Save(partialView);
            Assert.IsTrue(_fileSystem.FileExists("path-2/test-path-2.cshtml"));
            Assert.AreEqual("path-2\\test-path-2.cshtml".Replace("\\", $"{Path.DirectorySeparatorChar}"), partialView.Path);
            Assert.AreEqual("/Views/Partials/path-2/test-path-2.cshtml", partialView.VirtualPath);

            partialView = repository.Get("path-2/test-path-2.cshtml");
            Assert.IsNotNull(partialView);
            Assert.AreEqual("path-2\\test-path-2.cshtml".Replace("\\", $"{Path.DirectorySeparatorChar}"), partialView.Path);
            Assert.AreEqual("/Views/Partials/path-2/test-path-2.cshtml", partialView.VirtualPath);

            partialView =
                new PartialView("path-2\\test-path-3.cshtml") { Content = "// partialView" };
            repository.Save(partialView);
            Assert.IsTrue(_fileSystem.FileExists("path-2/test-path-3.cshtml"));
            Assert.AreEqual("path-2\\test-path-3.cshtml".Replace("\\", $"{Path.DirectorySeparatorChar}"), partialView.Path);
            Assert.AreEqual("/Views/Partials/path-2/test-path-3.cshtml", partialView.VirtualPath);

            partialView = repository.Get("path-2/test-path-3.cshtml");
            Assert.IsNotNull(partialView);
            Assert.AreEqual("path-2\\test-path-3.cshtml".Replace("\\", $"{Path.DirectorySeparatorChar}"), partialView.Path);
            Assert.AreEqual("/Views/Partials/path-2/test-path-3.cshtml", partialView.VirtualPath);

            partialView = repository.Get("path-2\\test-path-3.cshtml");
            Assert.IsNotNull(partialView);
            Assert.AreEqual("path-2\\test-path-3.cshtml".Replace("\\", $"{Path.DirectorySeparatorChar}"), partialView.Path);
            Assert.AreEqual("/Views/Partials/path-2/test-path-3.cshtml", partialView.VirtualPath);

            partialView =
                new PartialView("..\\test-path-4.cshtml") { Content = "// partialView" };
            Assert.Throws<UnauthorizedAccessException>(() =>
                repository.Save(partialView));

            partialView =
                new PartialView("\\test-path-5.cshtml") { Content = "// partialView" };
            repository.Save(partialView);

            partialView = repository.Get("\\test-path-5.cshtml");
            Assert.IsNotNull(partialView);
            Assert.AreEqual("test-path-5.cshtml", partialView.Path);
            Assert.AreEqual("/Views/Partials/test-path-5.cshtml", partialView.VirtualPath);

            partialView = repository.Get("missing.cshtml");
            Assert.IsNull(partialView);

            Assert.Throws<UnauthorizedAccessException>(() => partialView = repository.Get("..\\test-path-4.cshtml"));
            Assert.Throws<UnauthorizedAccessException>(() => partialView = repository.Get("../../packages.config"));
        }
    }

    [Test]
    public void Save_In_Production_Mode_Does_Not_Write_New_File()
    {
        // Arrange
        var fileSystems = FileSystemsCreator.CreateTestFileSystems(
            LoggerFactory,
            IOHelper,
            GetRequiredService<IOptions<GlobalSettings>>(),
            HostingEnvironment,
            _fileSystem,
            null,
            null,
            null);

        var productionRuntimeSettings = CreateRuntimeSettingsMonitor(RuntimeMode.Production);
        var repository = new PartialViewRepository(fileSystems, productionRuntimeSettings);

        IPartialView partialView = new PartialView("production-test-new.cshtml") { Content = "// partialView" };

        // Act
        repository.Save(partialView);

        // Assert - file should NOT be created in production mode
        Assert.IsFalse(_fileSystem.FileExists("production-test-new.cshtml"));
    }

    [Test]
    public void Save_In_Production_Mode_Does_Not_Update_Existing_File()
    {
        // Arrange - create file in development mode first.
        var fileSystems = FileSystemsCreator.CreateTestFileSystems(
            LoggerFactory,
            IOHelper,
            GetRequiredService<IOptions<GlobalSettings>>(),
            HostingEnvironment,
            _fileSystem,
            null,
            null,
            null);

        var developmentRuntimeSettings = CreateRuntimeSettingsMonitor(RuntimeMode.Development);
        var developmentRepository = new PartialViewRepository(fileSystems, developmentRuntimeSettings);

        IPartialView partialView = new PartialView("production-test-update.cshtml") { Content = "// original content" };
        developmentRepository.Save(partialView);
        Assert.IsTrue(_fileSystem.FileExists("production-test-update.cshtml"));

        // Read original content.
        using var originalStream = _fileSystem.OpenFile("production-test-update.cshtml");
        using var originalReader = new StreamReader(originalStream);
        var originalContent = originalReader.ReadToEnd();
        Assert.That(originalContent, Does.Contain("original content"));

        // Act - try to update in production mode.
        var productionRuntimeSettings = CreateRuntimeSettingsMonitor(RuntimeMode.Production);
        var productionRepository = new PartialViewRepository(fileSystems, productionRuntimeSettings);

        IPartialView updatedPartialView = productionRepository.Get("production-test-update.cshtml");
        Assert.IsNotNull(updatedPartialView);

        // Modify and try to save.
        updatedPartialView.Content = "// modified content";
        productionRepository.Save(updatedPartialView);

        // Assert - file should still have original content.
        using var updatedStream = _fileSystem.OpenFile("production-test-update.cshtml");
        using var updatedReader = new StreamReader(updatedStream);
        var updatedContent = updatedReader.ReadToEnd();
        Assert.That(updatedContent, Does.Contain("original content"));
        Assert.That(updatedContent, Does.Not.Contain("modified content"));
    }

    [Test]
    public void Save_In_Development_Mode_Writes_File()
    {
        // Arrange
        var fileSystems = FileSystemsCreator.CreateTestFileSystems(
            LoggerFactory,
            IOHelper,
            GetRequiredService<IOptions<GlobalSettings>>(),
            HostingEnvironment,
            _fileSystem,
            null,
            null,
            null);

        var developmentRuntimeSettings = CreateRuntimeSettingsMonitor(RuntimeMode.Development);
        var repository = new PartialViewRepository(fileSystems, developmentRuntimeSettings);

        IPartialView partialView = new PartialView("development-test.cshtml") { Content = "// partialView" };

        // Act
        repository.Save(partialView);

        // Assert - file should be created in development mode.
        Assert.IsTrue(_fileSystem.FileExists("development-test.cshtml"));
    }

    [Test]
    public void Save_In_BackofficeDevelopment_Mode_Writes_File()
    {
        // Arrange
        var fileSystems = FileSystemsCreator.CreateTestFileSystems(
            LoggerFactory,
            IOHelper,
            GetRequiredService<IOptions<GlobalSettings>>(),
            HostingEnvironment,
            _fileSystem,
            null,
            null,
            null);

        var backofficeDevelopmentRuntimeSettings = CreateRuntimeSettingsMonitor(RuntimeMode.BackofficeDevelopment);
        var repository = new PartialViewRepository(fileSystems, backofficeDevelopmentRuntimeSettings);

        IPartialView partialView = new PartialView("backoffice-development-test.cshtml") { Content = "// partialView" };

        // Act
        repository.Save(partialView);

        // Assert - file should be created in backoffice development mode.
        Assert.IsTrue(_fileSystem.FileExists("backoffice-development-test.cshtml"));
    }

    private void Purge(PhysicalFileSystem fs, string path)
    {
        var files = fs.GetFiles(path, "*.cshtml");
        foreach (var file in files)
        {
            fs.DeleteFile(file);
        }

        var dirs = fs.GetDirectories(path);
        foreach (var dir in dirs)
        {
            Purge(fs, dir);
            fs.DeleteDirectory(dir);
        }
    }
}
