﻿using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class StylesheetRepositoryTest : TestWithDatabaseBase
    {
        private IFileSystems _fileSystems;
        private IFileSystem _fileSystem;

        public override void SetUp()
        {
            base.SetUp();

            _fileSystems = Mock.Of<IFileSystems>();
            _fileSystem = new PhysicalFileSystem(SystemDirectories.Css);
            Mock.Get(_fileSystems).Setup(x => x.StylesheetsFileSystem).Returns(_fileSystem);
            var stream = CreateStream("body {background:#EE7600; color:#FFF;}");
            _fileSystem.AddFile("styles.css", stream);
        }

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                // Act
                var repository = new StylesheetRepository(_fileSystems);


                // Assert
                Assert.That(repository, Is.Not.Null);
            }
        }

        [Test]
        public void Can_Perform_Add()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = new StylesheetRepository(_fileSystems);

                // Act
                var stylesheet = new Stylesheet("test-add.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
                repository.Save(stylesheet);


                //Assert
                Assert.That(_fileSystem.FileExists("test-add.css"), Is.True);
            }
        }

        [Test]
        public void Can_Perform_Update()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = new StylesheetRepository(_fileSystems);

                // Act
                var stylesheet = new Stylesheet("test-update.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
                repository.Save(stylesheet);


                var stylesheetUpdate = repository.Get("test-update.css");
                stylesheetUpdate.Content = "body { color:#000; }";
                repository.Save(stylesheetUpdate);


                var stylesheetUpdated = repository.Get("test-update.css");

                //Assert
                Assert.That(stylesheetUpdated, Is.Not.Null);
                Assert.That(stylesheetUpdated.HasIdentity, Is.True);
                Assert.That(stylesheetUpdated.Content, Is.EqualTo("body { color:#000; }"));
            }
        }

        [Test]
        public void Can_Perform_Update_With_Property()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = new StylesheetRepository(_fileSystems);

                // Act
                var stylesheet = new Stylesheet("test-update.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
                repository.Save(stylesheet);


                stylesheet.AddProperty(new StylesheetProperty("Test", "p", "font-size:2em;"));

                repository.Save(stylesheet);


                //re-get
                stylesheet = repository.Get(stylesheet.Name);

                //Assert
                Assert.That(stylesheet.Content, Is.EqualTo("body { color:#000; } .bold {font-weight:bold;}\r\n\r\n/**umb_name:Test*/\r\np {\r\n\tfont-size:2em;\r\n}"));
                Assert.AreEqual(1, stylesheet.Properties.Count());
            }
        }

        [Test]
        public void Throws_When_Adding_Duplicate_Properties()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = new StylesheetRepository(_fileSystems);

                // Act
                var stylesheet = new Stylesheet("test-update.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
                repository.Save(stylesheet);


                stylesheet.AddProperty(new StylesheetProperty("Test", "p", "font-size:2em;"));

                Assert.Throws<DuplicateNameException>(() => stylesheet.AddProperty(new StylesheetProperty("test", "p", "font-size:2em;")));
            }
        }

        [Test]
        public void Can_Perform_Delete()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = new StylesheetRepository(_fileSystems);

                // Act
                var stylesheet = new Stylesheet("test-delete.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
                repository.Save(stylesheet);


                repository.Delete(stylesheet);


                //Assert
                Assert.That(_fileSystem.FileExists("test-delete.css"), Is.False);
            }
        }

        [Test]
        public void Can_Perform_Get()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = new StylesheetRepository(_fileSystems);

                // Act
                var stylesheet = repository.Get("styles.css");

                // Assert
                Assert.That(stylesheet, Is.Not.Null);
                Assert.That(stylesheet.HasIdentity, Is.True);
                Assert.That(stylesheet.Content, Is.EqualTo("body {background:#EE7600; color:#FFF;}"));
                Assert.That(repository.ValidateStylesheet(stylesheet), Is.True);
            }
        }

        [Test]
        public void Can_Perform_GetAll()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = new StylesheetRepository(_fileSystems);

                var stylesheet = new Stylesheet("styles-v2.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
                repository.Save(stylesheet);


                // Act
                var stylesheets = repository.GetMany();

                // Assert
                Assert.That(stylesheets, Is.Not.Null);
                Assert.That(stylesheets.Any(), Is.True);
                Assert.That(stylesheets.Any(x => x == null), Is.False);
                Assert.That(stylesheets.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_GetAll_With_Params()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = new StylesheetRepository(_fileSystems);

                var stylesheet = new Stylesheet("styles-v2.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
                repository.Save(stylesheet);


                // Act
                var stylesheets = repository.GetMany("styles-v2.css", "styles.css");

                // Assert
                Assert.That(stylesheets, Is.Not.Null);
                Assert.That(stylesheets.Any(), Is.True);
                Assert.That(stylesheets.Any(x => x == null), Is.False);
                Assert.That(stylesheets.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_Exists()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = new StylesheetRepository(_fileSystems);

                // Act
                var exists = repository.Exists("styles.css");

                // Assert
                Assert.That(exists, Is.True);
            }
        }

        [Test]
        public void PathTests()
        {
            // unless noted otherwise, no changes / 7.2.8

            using (ScopeProvider.CreateScope())
            {
                var repository = new StylesheetRepository(_fileSystems);

                var stylesheet = new Stylesheet("test-path-1.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
                repository.Save(stylesheet);

                Assert.IsTrue(_fileSystem.FileExists("test-path-1.css"));
                Assert.AreEqual("test-path-1.css", stylesheet.Path);
                Assert.AreEqual("/css/test-path-1.css", stylesheet.VirtualPath);

                stylesheet = new Stylesheet("path-2/test-path-2.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
                repository.Save(stylesheet);

                Assert.IsTrue(_fileSystem.FileExists("path-2/test-path-2.css"));
                Assert.AreEqual("path-2\\test-path-2.css", stylesheet.Path); // fixed in 7.3 - 7.2.8 does not update the path
                Assert.AreEqual("/css/path-2/test-path-2.css", stylesheet.VirtualPath);

                stylesheet = repository.Get("path-2/test-path-2.css");
                Assert.IsNotNull(stylesheet);
                Assert.AreEqual("path-2\\test-path-2.css", stylesheet.Path);
                Assert.AreEqual("/css/path-2/test-path-2.css", stylesheet.VirtualPath);

                stylesheet = new Stylesheet("path-2\\test-path-3.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
                repository.Save(stylesheet);

                Assert.IsTrue(_fileSystem.FileExists("path-2/test-path-3.css"));
                Assert.AreEqual("path-2\\test-path-3.css", stylesheet.Path);
                Assert.AreEqual("/css/path-2/test-path-3.css", stylesheet.VirtualPath);

                stylesheet = repository.Get("path-2/test-path-3.css");
                Assert.IsNotNull(stylesheet);
                Assert.AreEqual("path-2\\test-path-3.css", stylesheet.Path);
                Assert.AreEqual("/css/path-2/test-path-3.css", stylesheet.VirtualPath);

                stylesheet = repository.Get("path-2\\test-path-3.css");
                Assert.IsNotNull(stylesheet);
                Assert.AreEqual("path-2\\test-path-3.css", stylesheet.Path);
                Assert.AreEqual("/css/path-2/test-path-3.css", stylesheet.VirtualPath);

                stylesheet = new Stylesheet("\\test-path-4.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
                Assert.Throws<UnauthorizedAccessException>(() => // fixed in 7.3 - 7.2.8 used to strip the \
                {
                    repository.Save(stylesheet);
                });

                // fixed in 7.3 - 7.2.8 used to throw
                stylesheet = repository.Get("missing.css");
                Assert.IsNull(stylesheet);

                // fixed in 7.3 - 7.2.8 used to...
                Assert.Throws<UnauthorizedAccessException>(() =>
                {
                    stylesheet = repository.Get("\\test-path-4.css"); // outside the filesystem, does not exist
                });
                Assert.Throws<UnauthorizedAccessException>(() =>
                {
                    stylesheet = repository.Get("../packages.config"); // outside the filesystem, exists
                });
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            //Delete all files
            Purge((PhysicalFileSystem) _fileSystem, "");
            _fileSystem = null;
        }

        private void Purge(PhysicalFileSystem fs, string path)
        {
            var files = fs.GetFiles(path, "*.css");
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
