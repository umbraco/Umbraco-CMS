using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(WithApplication = true, Database = UmbracoTestOptions.Database.NewEmptyPerFixture)]
    public class PartialViewRepositoryTests : TestWithDatabaseBase
    {
        private IFileSystem _fileSystem;

        public override void SetUp()
        {
            base.SetUp();

            _fileSystem = new PhysicalFileSystem(SystemDirectories.MvcViews + "/Partials/");
        }

        protected override void Compose()
        {
            base.Compose();

            Composition.RegisterSingleton(f => new DataEditorCollection(Enumerable.Empty<DataEditor>()));
        }

        [Test]
        public void PathTests()
        {
            // unless noted otherwise, no changes / 7.2.8

            var fileSystems = Mock.Of<IFileSystems>();
            Mock.Get(fileSystems).Setup(x => x.PartialViewsFileSystem).Returns(_fileSystem);

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
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
                Assert.AreEqual("path-2\\test-path-2.cshtml", partialView.Path); // fixed in 7.3 - 7.2.8 does not update the path
                Assert.AreEqual("/Views/Partials/path-2/test-path-2.cshtml", partialView.VirtualPath);

                partialView = (PartialView) repository.Get("path-2/test-path-2.cshtml");
                Assert.IsNotNull(partialView);
                Assert.AreEqual("path-2\\test-path-2.cshtml", partialView.Path);
                Assert.AreEqual("/Views/Partials/path-2/test-path-2.cshtml", partialView.VirtualPath);

                partialView = new PartialView(PartialViewType.PartialView, "path-2\\test-path-3.cshtml") { Content = "// partialView" };
                repository.Save(partialView);
                Assert.IsTrue(_fileSystem.FileExists("path-2/test-path-3.cshtml"));
                Assert.AreEqual("path-2\\test-path-3.cshtml", partialView.Path);
                Assert.AreEqual("/Views/Partials/path-2/test-path-3.cshtml", partialView.VirtualPath);

                partialView = (PartialView) repository.Get("path-2/test-path-3.cshtml");
                Assert.IsNotNull(partialView);
                Assert.AreEqual("path-2\\test-path-3.cshtml", partialView.Path);
                Assert.AreEqual("/Views/Partials/path-2/test-path-3.cshtml", partialView.VirtualPath);

                partialView = (PartialView) repository.Get("path-2\\test-path-3.cshtml");
                Assert.IsNotNull(partialView);
                Assert.AreEqual("path-2\\test-path-3.cshtml", partialView.Path);
                Assert.AreEqual("/Views/Partials/path-2/test-path-3.cshtml", partialView.VirtualPath);

                partialView = new PartialView(PartialViewType.PartialView, "\\test-path-4.cshtml") { Content = "// partialView" };
                Assert.Throws<FileSecurityException>(() => // fixed in 7.3 - 7.2.8 used to strip the \
                {
                    repository.Save(partialView);
                });

                partialView = (PartialView) repository.Get("missing.cshtml");
                Assert.IsNull(partialView);

                // fixed in 7.3 - 7.2.8 used to...
                Assert.Throws<FileSecurityException>(() =>
                {
                    partialView = (PartialView) repository.Get("\\test-path-4.cshtml"); // outside the filesystem, does not exist
                });
                Assert.Throws<FileSecurityException>(() =>
                {
                    partialView = (PartialView) repository.Get("../../packages.config"); // outside the filesystem, exists
                });
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            //Delete all files
            Purge((PhysicalFileSystem)_fileSystem, "");
            _fileSystem = null;
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
}
