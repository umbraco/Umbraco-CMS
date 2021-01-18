// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
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
            IFileSystems fileSystems = Mock.Of<IFileSystems>();
            Mock.Get(fileSystems).Setup(x => x.PartialViewsFileSystem).Returns(_fileSystem);

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var repository = new PartialViewRepository(fileSystems, IOHelper);

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
