using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(WithApplication = true, Database = UmbracoTestOptions.Database.NewEmptyPerFixture)]
    public class ScriptRepositoryTest : TestWithDatabaseBase
    {
        private IFileSystems _fileSystems;
        private IFileSystem _fileSystem;

        public override void SetUp()
        {
            base.SetUp();

            _fileSystems = Mock.Of<IFileSystems>();
            _fileSystem = new PhysicalFileSystem(SystemDirectories.Scripts);
            Mock.Get(_fileSystems).Setup(x => x.ScriptsFileSystem).Returns(_fileSystem);
            using (var stream = CreateStream("Umbraco.Sys.registerNamespace(\"Umbraco.Utils\");"))
            {
                _fileSystem.AddFile("test-script.js", stream);
            }
        }

        protected override void Compose()
        {
            base.Compose();

            Composition.RegisterSingleton(f => new DataEditorCollection(Enumerable.Empty<DataEditor>()));
        }

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                // Act
                var repository = new ScriptRepository(_fileSystems, Mock.Of<IContentSection>());

                // Assert
                Assert.That(repository, Is.Not.Null);
            }
        }

        [Test]
        public void Can_Perform_Add_On_ScriptRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = new ScriptRepository(_fileSystems, Mock.Of<IContentSection>());

                // Act
                var script = new Script("test-add-script.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
                repository.Save(script);


                //Assert
                Assert.That(_fileSystem.FileExists("test-add-script.js"), Is.True);
            }
        }

        [Test]
        public void Can_Perform_Update_On_ScriptRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = new ScriptRepository(_fileSystems, Mock.Of<IContentSection>());

                // Act
                var script = new Script("test-updated-script.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
                repository.Save(script);


                script.Content = "/// <reference name=\"MicrosoftAjax-Updated.js\"/>";
                repository.Save(script);


                var scriptUpdated = repository.Get("test-updated-script.js");

                // Assert
                Assert.That(_fileSystem.FileExists("test-updated-script.js"), Is.True);
                Assert.That(scriptUpdated.Content, Is.EqualTo("/// <reference name=\"MicrosoftAjax-Updated.js\"/>"));
            }
        }

        [Test]
        public void Can_Perform_Delete_On_ScriptRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = new ScriptRepository(_fileSystems, Mock.Of<IContentSection>());

                // Act
                var script = repository.Get("test-script.js");
                repository.Delete(script);


                // Assert

                Assert.IsFalse(repository.Exists("test-script.js"));
            }
        }

        [Test]
        public void Can_Perform_Get_On_ScriptRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = new ScriptRepository(_fileSystems, Mock.Of<IContentSection>());

                // Act
                var exists = repository.Get("test-script.js");

                // Assert
                Assert.That(exists, Is.Not.Null);
                Assert.That(exists.Alias, Is.EqualTo("test-script"));
                Assert.That(exists.Name, Is.EqualTo("test-script.js"));
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_ScriptRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = new ScriptRepository(_fileSystems, Mock.Of<IContentSection>());

                var script = new Script("test-script1.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
                repository.Save(script);
                var script2 = new Script("test-script2.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
                repository.Save(script2);
                var script3 = new Script("test-script3.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
                repository.Save(script3);


                // Act
                var scripts = repository.GetMany();

                // Assert
                Assert.That(scripts, Is.Not.Null);
                Assert.That(scripts.Any(), Is.True);
                Assert.That(scripts.Any(x => x == null), Is.False);
                Assert.That(scripts.Count(), Is.EqualTo(4));
            }
        }

        [Test]
        public void Can_Perform_GetAll_With_Params_On_ScriptRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = new ScriptRepository(_fileSystems, Mock.Of<IContentSection>());

                var script = new Script("test-script1.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
                repository.Save(script);
                var script2 = new Script("test-script2.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
                repository.Save(script2);
                var script3 = new Script("test-script3.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
                repository.Save(script3);


                // Act
                var scripts = repository.GetMany("test-script1.js", "test-script2.js");

                // Assert
                Assert.That(scripts, Is.Not.Null);
                Assert.That(scripts.Any(), Is.True);
                Assert.That(scripts.Any(x => x == null), Is.False);
                Assert.That(scripts.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_Exists_On_ScriptRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = new ScriptRepository(_fileSystems, Mock.Of<IContentSection>());

                // Act
                var exists = repository.Exists("test-script.js");

                // Assert
                Assert.That(exists, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Move_On_ScriptRepository()
        {
            const string content = "/// <reference name=\"MicrosoftAjax.js\"/>";

            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = new ScriptRepository(_fileSystems, Mock.Of<IContentSection>());

                var script = new Script("test-move-script.js") { Content = content };
                repository.Save(script);


                // Act
                script = repository.Get("test-move-script.js");
                script.Path = "moved/test-move-script.js";
                repository.Save(script);


                var existsOld = repository.Exists("test-move-script.js");
                var existsNew = repository.Exists("moved/test-move-script.js");

                script = repository.Get("moved/test-move-script.js");

                // Assert
                Assert.IsNotNull(script);
                Assert.IsFalse(existsOld);
                Assert.IsTrue(existsNew);
                Assert.AreEqual(content, script.Content);
            }
        }

        [Test]
        public void PathTests()
        {
            // unless noted otherwise, no changes / 7.2.8

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = new ScriptRepository(_fileSystems, Mock.Of<IContentSection>());

                var script = new Script("test-path-1.js") { Content = "// script" };
                repository.Save(script);

                Assert.IsTrue(_fileSystem.FileExists("test-path-1.js"));
                Assert.AreEqual("test-path-1.js", script.Path);
                Assert.AreEqual("/scripts/test-path-1.js", script.VirtualPath);

                //ensure you can prefix the same path as the root path name
                script = new Script("scripts/path-2/test-path-2.js") { Content = "// script" };
                repository.Save(script);

                Assert.IsTrue(_fileSystem.FileExists("scripts/path-2/test-path-2.js"));
                Assert.AreEqual("scripts\\path-2\\test-path-2.js", script.Path);
                Assert.AreEqual("/scripts/scripts/path-2/test-path-2.js", script.VirtualPath);

                script = new Script("path-2/test-path-2.js") { Content = "// script" };
                repository.Save(script);

                Assert.IsTrue(_fileSystem.FileExists("path-2/test-path-2.js"));
                Assert.AreEqual("path-2\\test-path-2.js", script.Path); // fixed in 7.3 - 7.2.8 does not update the path
                Assert.AreEqual("/scripts/path-2/test-path-2.js", script.VirtualPath);

                script = repository.Get("path-2/test-path-2.js");
                Assert.IsNotNull(script);
                Assert.AreEqual("path-2\\test-path-2.js", script.Path);
                Assert.AreEqual("/scripts/path-2/test-path-2.js", script.VirtualPath);

                script = new Script("path-2\\test-path-3.js") { Content = "// script" };
                repository.Save(script);

                Assert.IsTrue(_fileSystem.FileExists("path-2/test-path-3.js"));
                Assert.AreEqual("path-2\\test-path-3.js", script.Path);
                Assert.AreEqual("/scripts/path-2/test-path-3.js", script.VirtualPath);

                script = repository.Get("path-2/test-path-3.js");
                Assert.IsNotNull(script);
                Assert.AreEqual("path-2\\test-path-3.js", script.Path);
                Assert.AreEqual("/scripts/path-2/test-path-3.js", script.VirtualPath);

                script = repository.Get("path-2\\test-path-3.js");
                Assert.IsNotNull(script);
                Assert.AreEqual("path-2\\test-path-3.js", script.Path);
                Assert.AreEqual("/scripts/path-2/test-path-3.js", script.VirtualPath);

                script = new Script("\\test-path-4.js") { Content = "// script" };
                Assert.Throws<FileSecurityException>(() => // fixed in 7.3 - 7.2.8 used to strip the \
                {
                    repository.Save(script);
                });

                script = repository.Get("missing.js");
                Assert.IsNull(script);

                // fixed in 7.3 - 7.2.8 used to...
                Assert.Throws<FileSecurityException>(() =>
                {
                    script = repository.Get("\\test-path-4.js"); // outside the filesystem, does not exist
                });
                Assert.Throws<FileSecurityException>(() =>
                {
                    script = repository.Get("../packages.config"); // outside the filesystem, exists
                });
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            //Delete all files
            Purge(_fileSystems.ScriptsFileSystem, "");
            _fileSystems = null;
        }

        private void Purge(IFileSystem fs, string path)
        {
            var files = fs.GetFiles(path, "*.js");
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

        protected Stream CreateStream(string contents = null)
        {
            if (string.IsNullOrEmpty(contents))
                contents = "/* test */";

            var bytes = Encoding.UTF8.GetBytes(contents);
            var stream = new MemoryStream(bytes);

            return stream;
        }
    }
}
