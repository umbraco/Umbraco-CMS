// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.None)]
    public class PartialViewRepositoryTests : UmbracoIntegrationTest
    {
        private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

        private IFileSystem _fileSystem;

        [SetUp]
        public void SetUp() =>
            _fileSystem = new PhysicalFileSystem(IOHelper, HostingEnvironment, LoggerFactory.CreateLogger<PhysicalFileSystem>(), HostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.PartialViews), HostingEnvironment.ToAbsolute(Constants.SystemDirectories.PartialViews));

        [TearDown]
        public void TearDownFiles()
        {
            // Delete all files
            Purge((PhysicalFileSystem)_fileSystem, string.Empty);
            _fileSystem = null;
        }

        [Test]
        public void PathTests()
        {
            // unless noted otherwise, no changes / 7.2.8
            FileSystems fileSystems = FileSystemsCreator.CreateTestFileSystems(LoggerFactory, IOHelper,
                GetRequiredService<IOptions<GlobalSettings>>(), HostingEnvironment,
                null, _fileSystem, null, null, null);

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var repository = new PartialViewRepository(fileSystems);

                var partialView = new PartialView(PartialViewType.PartialView, "test-path-1.cshtml") { Content = "// partialView" };
                repository.Save(partialView);
                Assert.IsTrue(_fileSystem.FileExists("test-path-1.cshtml"));
                Assert.AreEqual("test-path-1.cshtml", partialView.Path);
                Assert.AreEqual("/Views/Partials/test-path-1.cshtml", partialView.VirtualPath);

                partialView = new PartialView(PartialViewType.PartialView, "path-2/test-path-2.cshtml") { Content = "// partialView" };
                repository.Save(partialView);
                Assert.IsTrue(_fileSystem.FileExists("path-2/test-path-2.cshtml"));
                Assert.AreEqual("path-2\\test-path-2.cshtml".Replace("\\", $"{Path.DirectorySeparatorChar}"), partialView.Path); // fixed in 7.3 - 7.2.8 does not update the path
                Assert.AreEqual("/Views/Partials/path-2/test-path-2.cshtml", partialView.VirtualPath);

                partialView = (PartialView)repository.Get("path-2/test-path-2.cshtml");
                Assert.IsNotNull(partialView);
                Assert.AreEqual("path-2\\test-path-2.cshtml".Replace("\\", $"{Path.DirectorySeparatorChar}"), partialView.Path);
                Assert.AreEqual("/Views/Partials/path-2/test-path-2.cshtml", partialView.VirtualPath);

                partialView = new PartialView(PartialViewType.PartialView, "path-2\\test-path-3.cshtml") { Content = "// partialView" };
                repository.Save(partialView);
                Assert.IsTrue(_fileSystem.FileExists("path-2/test-path-3.cshtml"));
                Assert.AreEqual("path-2\\test-path-3.cshtml".Replace("\\", $"{Path.DirectorySeparatorChar}"), partialView.Path);
                Assert.AreEqual("/Views/Partials/path-2/test-path-3.cshtml", partialView.VirtualPath);

                partialView = (PartialView)repository.Get("path-2/test-path-3.cshtml");
                Assert.IsNotNull(partialView);
                Assert.AreEqual("path-2\\test-path-3.cshtml".Replace("\\", $"{Path.DirectorySeparatorChar}"), partialView.Path);
                Assert.AreEqual("/Views/Partials/path-2/test-path-3.cshtml", partialView.VirtualPath);

                partialView = (PartialView)repository.Get("path-2\\test-path-3.cshtml");
                Assert.IsNotNull(partialView);
                Assert.AreEqual("path-2\\test-path-3.cshtml".Replace("\\", $"{Path.DirectorySeparatorChar}"), partialView.Path);
                Assert.AreEqual("/Views/Partials/path-2/test-path-3.cshtml", partialView.VirtualPath);

                partialView = new PartialView(PartialViewType.PartialView, "\\test-path-4.cshtml") { Content = "// partialView" };
                Assert.Throws<UnauthorizedAccessException>(() => // fixed in 7.3 - 7.2.8 used to strip the \
                    repository.Save(partialView));

                partialView = (PartialView)repository.Get("missing.cshtml");
                Assert.IsNull(partialView);

                // fixed in 7.3 - 7.2.8 used to...
                Assert.Throws<UnauthorizedAccessException>(() => partialView = (PartialView)repository.Get("\\test-path-4.cshtml"));
                Assert.Throws<UnauthorizedAccessException>(() => partialView = (PartialView)repository.Get("../../packages.config"));
            }
        }

        private void Purge(PhysicalFileSystem fs, string path)
        {
            IEnumerable<string> files = fs.GetFiles(path, "*.cshtml");
            foreach (string file in files)
            {
                fs.DeleteFile(file);
            }

            IEnumerable<string> dirs = fs.GetDirectories(path);
            foreach (string dir in dirs)
            {
                Purge(fs, dir);
                fs.DeleteDirectory(dir);
            }
        }
    }
}
