// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
public class PartialViewRepositoryTests : UmbracoIntegrationTest
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

    private IFileSystem _fileSystem;

    [Test]
    public void PathTests()
    {
        // unless noted otherwise, no changes / 7.2.8
        var fileSystems = FileSystemsCreator.CreateTestFileSystems(
            LoggerFactory,
            IOHelper,
            GetRequiredService<IOptions<GlobalSettings>>(),
            HostingEnvironment,
            null,
            _fileSystem,
            null,
            null,
            null);

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = new PartialViewRepository(fileSystems);

            IPartialView partialView =
                new PartialView(PartialViewType.PartialView, "test-path-1.cshtml") { Content = "// partialView" };
            repository.Save(partialView);
            Assert.IsTrue(_fileSystem.FileExists("test-path-1.cshtml"));
            Assert.AreEqual("test-path-1.cshtml", partialView.Path);
            Assert.AreEqual("/Views/Partials/test-path-1.cshtml", partialView.VirtualPath);

            partialView =
                new PartialView(PartialViewType.PartialView, "path-2/test-path-2.cshtml") { Content = "// partialView" };
            repository.Save(partialView);
            Assert.IsTrue(_fileSystem.FileExists("path-2/test-path-2.cshtml"));
            Assert.AreEqual("path-2\\test-path-2.cshtml".Replace("\\", $"{Path.DirectorySeparatorChar}"), partialView.Path);
            Assert.AreEqual("/Views/Partials/path-2/test-path-2.cshtml", partialView.VirtualPath);

            partialView = repository.Get("path-2/test-path-2.cshtml");
            Assert.IsNotNull(partialView);
            Assert.AreEqual("path-2\\test-path-2.cshtml".Replace("\\", $"{Path.DirectorySeparatorChar}"), partialView.Path);
            Assert.AreEqual("/Views/Partials/path-2/test-path-2.cshtml", partialView.VirtualPath);

            partialView =
                new PartialView(PartialViewType.PartialView, "path-2\\test-path-3.cshtml") { Content = "// partialView" };
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
                new PartialView(PartialViewType.PartialView, "..\\test-path-4.cshtml") { Content = "// partialView" };
            Assert.Throws<UnauthorizedAccessException>(() =>
                repository.Save(partialView));

            partialView =
                new PartialView(PartialViewType.PartialView, "\\test-path-5.cshtml") { Content = "// partialView" };
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
