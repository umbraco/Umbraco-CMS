using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
    [TestFixture]
    public class StylesheetRepositoryTest : BaseDatabaseFactoryTest
    {
        private IFileSystem _fileSystem;

        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            _fileSystem = new PhysicalFileSystem(SystemDirectories.Css);
            var stream = CreateStream("body {background:#EE7600; color:#FFF;}");
            _fileSystem.AddFile("styles.css", stream);
        }

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            var repository = new StylesheetRepository(unitOfWork, _fileSystem);


            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Perform_Add()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            var repository = new StylesheetRepository(unitOfWork, _fileSystem);

            // Act
            var stylesheet = new Stylesheet("test-add.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
            repository.AddOrUpdate(stylesheet);
            unitOfWork.Commit();

            //Assert
            Assert.That(_fileSystem.FileExists("test-add.css"), Is.True);
        }

        [Test]
        public void Can_Perform_Update()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            
            var repository = new StylesheetRepository(unitOfWork, _fileSystem);

            // Act
            var stylesheet = new Stylesheet("test-update.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
            repository.AddOrUpdate(stylesheet);
            unitOfWork.Commit();

            var stylesheetUpdate = repository.Get("test-update.css");
            stylesheetUpdate.Content = "body { color:#000; }";
            repository.AddOrUpdate(stylesheetUpdate);
            unitOfWork.Commit();

            var stylesheetUpdated = repository.Get("test-update.css");

            //Assert
            Assert.That(stylesheetUpdated, Is.Not.Null);
            Assert.That(stylesheetUpdated.HasIdentity, Is.True);
            Assert.That(stylesheetUpdated.Content, Is.EqualTo("body { color:#000; }"));
        }

        [Test]
        public void Can_Perform_Update_With_Property()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            
            var repository = new StylesheetRepository(unitOfWork, _fileSystem);

            // Act
            var stylesheet = new Stylesheet("test-update.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
            repository.AddOrUpdate(stylesheet);
            unitOfWork.Commit();

            stylesheet.AddProperty(new StylesheetProperty("Test", "p", "font-size:2em;"));

            repository.AddOrUpdate(stylesheet);
            unitOfWork.Commit();

            //re-get
            stylesheet = repository.Get(stylesheet.Name);

            //Assert           
            Assert.That(stylesheet.Content, Is.EqualTo(@"body { color:#000; } .bold {font-weight:bold;}

/**umb_name:Test*/
p{font-size:2em;}"));
            Assert.AreEqual(1, stylesheet.Properties.Count());
        }

        [Test]
        public void Throws_When_Adding_Duplicate_Properties()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            var repository = new StylesheetRepository(unitOfWork, _fileSystem);

            // Act
            var stylesheet = new Stylesheet("test-update.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
            repository.AddOrUpdate(stylesheet);
            unitOfWork.Commit();

            stylesheet.AddProperty(new StylesheetProperty("Test", "p", "font-size:2em;"));

            Assert.Throws<DuplicateNameException>(() => stylesheet.AddProperty(new StylesheetProperty("test", "p", "font-size:2em;")));

        }

        [Test]
        public void Can_Perform_Delete()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            
            var repository = new StylesheetRepository(unitOfWork, _fileSystem);

            // Act
            var stylesheet = new Stylesheet("test-delete.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
            repository.AddOrUpdate(stylesheet);
            unitOfWork.Commit();

            repository.Delete(stylesheet);
            unitOfWork.Commit();

            //Assert
            Assert.That(_fileSystem.FileExists("test-delete.css"), Is.False);
        }

        [Test]
        public void Can_Perform_Get()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            
            var repository = new StylesheetRepository(unitOfWork, _fileSystem);

            // Act
            var stylesheet = repository.Get("styles.css");

            // Assert
            Assert.That(stylesheet, Is.Not.Null);
            Assert.That(stylesheet.HasIdentity, Is.True);
            Assert.That(stylesheet.Content, Is.EqualTo("body {background:#EE7600; color:#FFF;}"));
            Assert.That(repository.ValidateStylesheet(stylesheet), Is.True);
        }

        [Test]
        public void Can_Perform_GetAll()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            
            var repository = new StylesheetRepository(unitOfWork, _fileSystem);

            var stylesheet = new Stylesheet("styles-v2.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
            repository.AddOrUpdate(stylesheet);
            unitOfWork.Commit();

            // Act
            var stylesheets = repository.GetAll();

            // Assert
            Assert.That(stylesheets, Is.Not.Null);
            Assert.That(stylesheets.Any(), Is.True);
            Assert.That(stylesheets.Any(x => x == null), Is.False);
            Assert.That(stylesheets.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Can_Perform_GetAll_With_Params()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            
            var repository = new StylesheetRepository(unitOfWork, _fileSystem);

            var stylesheet = new Stylesheet("styles-v2.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
            repository.AddOrUpdate(stylesheet);
            unitOfWork.Commit();

            // Act
            var stylesheets = repository.GetAll("styles-v2.css", "styles.css");

            // Assert
            Assert.That(stylesheets, Is.Not.Null);
            Assert.That(stylesheets.Any(), Is.True);
            Assert.That(stylesheets.Any(x => x == null), Is.False);
            Assert.That(stylesheets.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Can_Perform_Exists()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            
            var repository = new StylesheetRepository(unitOfWork, _fileSystem);

            // Act
            var exists = repository.Exists("styles.css");

            // Assert
            Assert.That(exists, Is.True);
        }

        [Test]
        public void PathTests()
        {
            // unless noted otherwise, no changes / 7.2.8

            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new StylesheetRepository(unitOfWork, _fileSystem);

            var stylesheet = new Stylesheet("test-path-1.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
            repository.AddOrUpdate(stylesheet);
            unitOfWork.Commit();
            Assert.IsTrue(_fileSystem.FileExists("test-path-1.css"));
            Assert.AreEqual("test-path-1.css", stylesheet.Path);
            Assert.AreEqual("/css/test-path-1.css", stylesheet.VirtualPath);

            stylesheet = new Stylesheet("path-2/test-path-2.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
            repository.AddOrUpdate(stylesheet);
            unitOfWork.Commit();
            Assert.IsTrue(_fileSystem.FileExists("path-2/test-path-2.css"));
            Assert.AreEqual("path-2\\test-path-2.css", stylesheet.Path); // fixed in 7.3 - 7.2.8 does not update the path
            Assert.AreEqual("/css/path-2/test-path-2.css", stylesheet.VirtualPath);

            stylesheet = repository.Get("path-2/test-path-2.css");
            Assert.IsNotNull(stylesheet);
            Assert.AreEqual("path-2\\test-path-2.css", stylesheet.Path);
            Assert.AreEqual("/css/path-2/test-path-2.css", stylesheet.VirtualPath);

            stylesheet = new Stylesheet("path-2\\test-path-3.css") { Content = "body { color:#000; } .bold {font-weight:bold;}" };
            repository.AddOrUpdate(stylesheet);
            unitOfWork.Commit();
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
            Assert.Throws<FileSecurityException>(() => // fixed in 7.3 - 7.2.8 used to strip the \
            {
                repository.AddOrUpdate(stylesheet);
            });

            // fixed in 7.3 - 7.2.8 used to throw
            stylesheet = repository.Get("missing.css");
            Assert.IsNull(stylesheet);

            // fixed in 7.3 - 7.2.8 used to...
            Assert.Throws<FileSecurityException>(() =>
            {
                stylesheet = repository.Get("\\test-path-4.css"); // outside the filesystem, does not exist
            });
            Assert.Throws<FileSecurityException>(() =>
            {
                stylesheet = repository.Get("../packages.config"); // outside the filesystem, exists
            });
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