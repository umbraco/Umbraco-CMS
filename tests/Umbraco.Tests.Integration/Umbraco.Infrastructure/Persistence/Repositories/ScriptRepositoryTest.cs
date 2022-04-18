// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.None)]
    public class ScriptRepositoryTest : UmbracoIntegrationTest
    {
        private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

        private FileSystems _fileSystems;
        private IFileSystem _fileSystem;

        [SetUp]
        public void SetUpFileSystem()
        {
            string path = GlobalSettings.UmbracoScriptsPath;
            _fileSystem = new PhysicalFileSystem(IOHelper, HostingEnvironment, LoggerFactory.CreateLogger<PhysicalFileSystem>(), HostingEnvironment.MapPathWebRoot(path), HostingEnvironment.ToAbsolute(path));

            _fileSystems = FileSystemsCreator.CreateTestFileSystems(LoggerFactory, IOHelper, GetRequiredService<IOptions<GlobalSettings>>(),
                HostingEnvironment,
                null, null, null, _fileSystem, null);
            using (Stream stream = CreateStream("Umbraco.Sys.registerNamespace(\"Umbraco.Utils\");"))
            {
                _fileSystem.AddFile("test-script.js", stream);
            }
        }

        [TearDown]
        public void TearDownFileSystem()
        {
            // Delete all files
            Purge(_fileSystems.ScriptsFileSystem, string.Empty);
            _fileSystems = null;
        }

        private IScriptRepository CreateRepository()
        {
            var globalSettings = new GlobalSettings();
            return new ScriptRepository(_fileSystems);
        }

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = ScopeProvider.CreateScope())
            {
                // Act
                IScriptRepository repository = CreateRepository();

                // Assert
                Assert.That(repository, Is.Not.Null);
            }
        }

        [Test]
        public void Can_Perform_Add_On_ScriptRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = ScopeProvider.CreateScope())
            {
                IScriptRepository repository = CreateRepository();

                // Act
                var script = new Script("test-add-script.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
                repository.Save(script);

                // Assert
                Assert.That(_fileSystem.FileExists("test-add-script.js"), Is.True);
            }
        }

        [Test]
        public void Can_Perform_Update_On_ScriptRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = ScopeProvider.CreateScope())
            {
                IScriptRepository repository = CreateRepository();

                // Act
                var script = new Script("test-updated-script.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
                repository.Save(script);

                script.Content = "/// <reference name=\"MicrosoftAjax-Updated.js\"/>";
                repository.Save(script);

                IScript scriptUpdated = repository.Get("test-updated-script.js");

                // Assert
                Assert.That(_fileSystem.FileExists("test-updated-script.js"), Is.True);
                Assert.That(scriptUpdated.Content, Is.EqualTo("/// <reference name=\"MicrosoftAjax-Updated.js\"/>"));
            }
        }

        [Test]
        public void Can_Perform_Delete_On_ScriptRepository()
        {
            // Arrange
            using (IScope scope = ScopeProvider.CreateScope())
            {
                IScriptRepository repository = CreateRepository();

                // Act
                IScript script = repository.Get("test-script.js");
                repository.Delete(script);

                // Assert
                Assert.IsFalse(repository.Exists("test-script.js"));
            }
        }

        [Test]
        public void Can_Perform_Get_On_ScriptRepository()
        {
            // Arrange
            using (IScope scope = ScopeProvider.CreateScope())
            {
                IScriptRepository repository = CreateRepository();

                // Act
                IScript exists = repository.Get("test-script.js");

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
            using (IScope scope = ScopeProvider.CreateScope())
            {
                IScriptRepository repository = CreateRepository();

                var script = new Script("test-script1.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
                repository.Save(script);
                var script2 = new Script("test-script2.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
                repository.Save(script2);
                var script3 = new Script("test-script3.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
                repository.Save(script3);

                // Act
                IEnumerable<IScript> scripts = repository.GetMany();

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
            using (IScope scope = ScopeProvider.CreateScope())
            {
                IScriptRepository repository = CreateRepository();

                var script = new Script("test-script1.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
                repository.Save(script);
                var script2 = new Script("test-script2.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
                repository.Save(script2);
                var script3 = new Script("test-script3.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
                repository.Save(script3);

                // Act
                IEnumerable<IScript> scripts = repository.GetMany("test-script1.js", "test-script2.js");

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
            using (IScope scope = ScopeProvider.CreateScope())
            {
                IScriptRepository repository = CreateRepository();

                // Act
                bool exists = repository.Exists("test-script.js");

                // Assert
                Assert.That(exists, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Move_On_ScriptRepository()
        {
            const string content = "/// <reference name=\"MicrosoftAjax.js\"/>";

            // Arrange
            using (IScope scope = ScopeProvider.CreateScope())
            {
                IScriptRepository repository = CreateRepository();

                IScript script = new Script("test-move-script.js") { Content = content };
                repository.Save(script);

                // Act
                script = repository.Get("test-move-script.js");
                script.Path = "moved/test-move-script.js";
                repository.Save(script);

                bool existsOld = repository.Exists("test-move-script.js");
                bool existsNew = repository.Exists("moved/test-move-script.js");

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
            using (IScope scope = ScopeProvider.CreateScope())
            {
                IScriptRepository repository = CreateRepository();

                IScript script = new Script("test-path-1.js") { Content = "// script" };
                repository.Save(script);

                Assert.IsTrue(_fileSystem.FileExists("test-path-1.js"));
                Assert.AreEqual("test-path-1.js", script.Path);
                Assert.AreEqual("/scripts/test-path-1.js", script.VirtualPath);

                // ensure you can prefix the same path as the root path name
                script = new Script("scripts/path-2/test-path-2.js") { Content = "// script" };
                repository.Save(script);

                Assert.IsTrue(_fileSystem.FileExists("scripts/path-2/test-path-2.js"));
                Assert.AreEqual("scripts\\path-2\\test-path-2.js".Replace("\\", $"{Path.DirectorySeparatorChar}"), script.Path);
                Assert.AreEqual("/scripts/scripts/path-2/test-path-2.js", script.VirtualPath);

                script = new Script("path-2/test-path-2.js") { Content = "// script" };
                repository.Save(script);

                Assert.IsTrue(_fileSystem.FileExists("path-2/test-path-2.js"));
                Assert.AreEqual("path-2\\test-path-2.js".Replace("\\", $"{Path.DirectorySeparatorChar}"), script.Path); // fixed in 7.3 - 7.2.8 does not update the path
                Assert.AreEqual("/scripts/path-2/test-path-2.js", script.VirtualPath);

                script = repository.Get("path-2/test-path-2.js");
                Assert.IsNotNull(script);
                Assert.AreEqual("path-2\\test-path-2.js".Replace("\\", $"{Path.DirectorySeparatorChar}"), script.Path);
                Assert.AreEqual("/scripts/path-2/test-path-2.js", script.VirtualPath);

                script = new Script("path-2\\test-path-3.js") { Content = "// script" };
                repository.Save(script);

                Assert.IsTrue(_fileSystem.FileExists("path-2/test-path-3.js"));
                Assert.AreEqual("path-2\\test-path-3.js".Replace("\\", $"{Path.DirectorySeparatorChar}"), script.Path);
                Assert.AreEqual("/scripts/path-2/test-path-3.js", script.VirtualPath);

                script = repository.Get("path-2/test-path-3.js");
                Assert.IsNotNull(script);
                Assert.AreEqual("path-2\\test-path-3.js".Replace("\\", $"{Path.DirectorySeparatorChar}"), script.Path);
                Assert.AreEqual("/scripts/path-2/test-path-3.js", script.VirtualPath);

                script = repository.Get("path-2\\test-path-3.js");
                Assert.IsNotNull(script);
                Assert.AreEqual("path-2\\test-path-3.js".Replace("\\", $"{Path.DirectorySeparatorChar}"), script.Path);
                Assert.AreEqual("/scripts/path-2/test-path-3.js", script.VirtualPath);

                script = new Script("..\\test-path-4.js") { Content = "// script" };
                Assert.Throws<UnauthorizedAccessException>(() =>
                    repository.Save(script));

                script = new Script("\\test-path-5.js") { Content = "// script" };
                repository.Save(script);

                script = repository.Get("\\test-path-5.js");
                Assert.IsNotNull(script);
                Assert.AreEqual("test-path-5.js", script.Path);
                Assert.AreEqual("/scripts/test-path-5.js", script.VirtualPath);

                script = repository.Get("missing.js");
                Assert.IsNull(script);

                Assert.Throws<UnauthorizedAccessException>(() => script = repository.Get("..\\test-path-4.js"));
                Assert.Throws<UnauthorizedAccessException>(() => script = repository.Get("../packages.config"));
            }
        }

        private void Purge(IFileSystem fs, string path)
        {
            IEnumerable<string> files = fs.GetFiles(path, "*.js");
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

        protected Stream CreateStream(string contents = null)
        {
            if (string.IsNullOrEmpty(contents))
            {
                contents = "/* test */";
            }

            byte[] bytes = Encoding.UTF8.GetBytes(contents);
            var stream = new MemoryStream(bytes);

            return stream;
        }
    }
}
