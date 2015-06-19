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

        [TearDown]
        public void TearDown()
        {
            base.TearDown();

            //Delete all files
            var files = _fileSystem.GetFiles("", "*.css");
            foreach (var file in files)
            {
                _fileSystem.DeleteFile(file);
            }
            _fileSystem = null;
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