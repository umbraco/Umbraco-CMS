using System;
using System.Data.SqlServerCe;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    public class MacroRepositoryTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            CreateTestData();
        }

        [Test]
        public void Cannot_Add_Duplicate_Macros()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            using (var repository = new MacroRepository(unitOfWork))
            {
                var macro = new Macro("test", "Test", "~/usercontrol/blah.ascx", "MyAssembly", "test.xslt", "~/views/macropartials/test.cshtml");
                repository.AddOrUpdate(macro);

                Assert.Throws<SqlCeException>(unitOfWork.Commit);
            }

        }

        [Test]
        public void Cannot_Update_To_Duplicate_Macro_Alias()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            using (var repository = new MacroRepository(unitOfWork))
            {
                var macro = repository.Get(1);
                macro.Alias = "test1";
                repository.AddOrUpdate(macro);
                Assert.Throws<SqlCeException>(unitOfWork.Commit);
            }

        }

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            using (var repository = new MacroRepository(unitOfWork))
            {
                // Assert
                Assert.That(repository, Is.Not.Null);
            }
        }

        [Test]
        public void Can_Perform_Get_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork))
            {
                // Act
                var macro = repository.Get(1);

                // Assert
                Assert.That(macro, Is.Not.Null);
                Assert.That(macro.HasIdentity, Is.True);
                Assert.That(macro.Alias, Is.EqualTo("test1"));
                Assert.That(macro.CacheByPage, Is.EqualTo(false));
                Assert.That(macro.CachePersonalized, Is.EqualTo(false));
                Assert.That(macro.ControlAssembly, Is.EqualTo("MyAssembly1"));
                Assert.That(macro.ControlType, Is.EqualTo("~/usercontrol/test1.ascx"));
                Assert.That(macro.DontRender, Is.EqualTo(true));
                Assert.That(macro.Name, Is.EqualTo("Test1"));
                Assert.That(macro.RefreshRate, Is.EqualTo(0));
                Assert.That(macro.ScriptPath, Is.EqualTo("~/views/macropartials/test1.cshtml"));
                Assert.That(macro.UseInEditor, Is.EqualTo(false));
                Assert.That(macro.XsltPath, Is.EqualTo("test1.xslt"));
            }


        }

        [Test]
        public void Can_Perform_GetAll_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork))
            {
                // Act
                var macros = repository.GetAll();

                // Assert
                Assert.That(macros.Count(), Is.EqualTo(3));
            }

        }

        [Test]
        public void Can_Perform_GetByQuery_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork))
            {
                // Act
                var query = Query<Macro>.Builder.Where(x => x.Alias.ToUpper() == "TEST1");
                var result = repository.GetByQuery(query);

                // Assert
                Assert.AreEqual(1, result.Count());
            }
        }

        [Test]
        public void Can_Perform_Count_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork))
            {
                // Act
                var query = Query<Macro>.Builder.Where(x => x.Name.StartsWith("Test"));
                int count = repository.Count(query);

                // Assert
                Assert.That(count, Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_Add_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork))
            {
                // Act
                var macro = new Macro("test", "Test", "~/usercontrol/blah.ascx", "MyAssembly", "test.xslt", "~/views/macropartials/test.cshtml");
                repository.AddOrUpdate(macro);
                unitOfWork.Commit();

                // Assert
                Assert.That(macro.HasIdentity, Is.True);
                Assert.That(macro.Id, Is.EqualTo(4));//With 3 existing entries the Id should be 4   
            }
        }

        [Test]
        public void Can_Perform_Update_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork))
            {
                // Act
                var macro = repository.Get(2);
                macro.Name = "Hello";
                macro.RefreshRate = 1234;
                macro.CacheByPage = true;
                macro.CachePersonalized = true;
                macro.ControlAssembly = "";
                macro.ControlType = "";
                macro.DontRender = false;
                macro.ScriptPath = "~/newpath.cshtml";
                macro.UseInEditor = true;
                macro.XsltPath = "";

                repository.AddOrUpdate(macro);
                unitOfWork.Commit();

                var macroUpdated = repository.Get(2);

                // Assert
                Assert.That(macroUpdated, Is.Not.Null);
                Assert.That(macroUpdated.Name, Is.EqualTo("Hello"));
                Assert.That(macroUpdated.RefreshRate, Is.EqualTo(1234));
                Assert.That(macroUpdated.CacheByPage, Is.EqualTo(true));
                Assert.That(macroUpdated.CachePersonalized, Is.EqualTo(true));
                Assert.That(macroUpdated.ControlAssembly, Is.EqualTo(""));
                Assert.That(macroUpdated.ControlType, Is.EqualTo(""));
                Assert.That(macroUpdated.DontRender, Is.EqualTo(false));
                Assert.That(macroUpdated.ScriptPath, Is.EqualTo("~/newpath.cshtml"));
                Assert.That(macroUpdated.UseInEditor, Is.EqualTo(true));
                Assert.That(macroUpdated.XsltPath, Is.EqualTo(""));
            }
        }

        [Test]
        public void Can_Perform_Delete_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork))
            {
                // Act
                var macro = repository.Get(3);
                Assert.IsNotNull(macro);
                repository.Delete(macro);
                unitOfWork.Commit();

                var exists = repository.Exists(3);

                // Assert
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Exists_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork))
            {
                // Act
                var exists = repository.Exists(3);
                var doesntExist = repository.Exists(10);

                // Assert
                Assert.That(exists, Is.True);
                Assert.That(doesntExist, Is.False);
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public void CreateTestData()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            using (var unitOfWork = provider.GetUnitOfWork())
            using (var repository = new MacroRepository(unitOfWork))
            {
                repository.AddOrUpdate(new Macro("test1", "Test1", "~/usercontrol/test1.ascx", "MyAssembly1", "test1.xslt", "~/views/macropartials/test1.cshtml"));
                repository.AddOrUpdate(new Macro("test2", "Test2", "~/usercontrol/test2.ascx", "MyAssembly2", "test2.xslt", "~/views/macropartials/test2.cshtml"));
                repository.AddOrUpdate(new Macro("test3", "Test3", "~/usercontrol/test3.ascx", "MyAssembly3", "test3.xslt", "~/views/macropartials/test3.cshtml"));
                unitOfWork.Commit();
            }

        }
    }

    [TestFixture]
    public class ServerRegistrationRepositoryTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            CreateTestData();
        }

        [Test]
        public void Cannot_Add_Duplicate_Computer_Names()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            using (var repository = new ServerRegistrationRepository(unitOfWork))
            {
                var server = new ServerRegistration("http://shazwazza.com", "COMPUTER1", DateTime.Now);
                repository.AddOrUpdate(server);

                Assert.Throws<SqlCeException>(unitOfWork.Commit);
            }

        }

        [Test]
        public void Cannot_Update_To_Duplicate_Computer_Names()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            using (var repository = new ServerRegistrationRepository(unitOfWork))
            {
                var server = repository.Get(1);
                server.ComputerName = "COMPUTER2";
                repository.AddOrUpdate(server);
                Assert.Throws<SqlCeException>(unitOfWork.Commit);
            }

        }

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            using (var repository = new ServerRegistrationRepository(unitOfWork))
            {
                // Assert
                Assert.That(repository, Is.Not.Null);    
            }
        }

        [Test]
        public void Can_Perform_Get_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new ServerRegistrationRepository(unitOfWork))
            {
                // Act
                var server = repository.Get(1);

                // Assert
                Assert.That(server, Is.Not.Null);
                Assert.That(server.HasIdentity, Is.True);
                Assert.That(server.ServerAddress, Is.EqualTo("http://localhost"));    
            }

            
        }

        [Test]
        public void Can_Perform_GetAll_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new ServerRegistrationRepository(unitOfWork))
            {
                // Act
                var servers = repository.GetAll();

                // Assert
                Assert.That(servers.Count(), Is.EqualTo(3));    
            }
            
        }

        [Test]
        public void Can_Perform_GetByQuery_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new ServerRegistrationRepository(unitOfWork))
            {
                // Act
                var query = Query<ServerRegistration>.Builder.Where(x => x.ComputerName.ToUpper() == "COMPUTER3");
                var result = repository.GetByQuery(query);

                // Assert
                Assert.AreEqual(1, result.Count());    
            }
        }

        [Test]
        public void Can_Perform_Count_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new ServerRegistrationRepository(unitOfWork))
            {
                // Act
                var query = Query<ServerRegistration>.Builder.Where(x => x.ServerAddress.StartsWith("http://"));
                int count = repository.Count(query);

                // Assert
                Assert.That(count, Is.EqualTo(2));    
            }
        }

        [Test]
        public void Can_Perform_Add_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new ServerRegistrationRepository(unitOfWork))
            {
                // Act
                var server = new ServerRegistration("http://shazwazza.com", "COMPUTER4", DateTime.Now);
                repository.AddOrUpdate(server);
                unitOfWork.Commit();

                // Assert
                Assert.That(server.HasIdentity, Is.True);
                Assert.That(server.Id, Is.EqualTo(4));//With 3 existing entries the Id should be 4   
            }            
        }

        [Test]
        public void Can_Perform_Update_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new ServerRegistrationRepository(unitOfWork))
            {
                // Act
                var server = repository.Get(2);
                server.ServerAddress = "https://umbraco.com";
                server.IsActive = true;

                repository.AddOrUpdate(server);
                unitOfWork.Commit();

                var serverUpdated = repository.Get(2);

                // Assert
                Assert.That(serverUpdated, Is.Not.Null);
                Assert.That(serverUpdated.ServerAddress, Is.EqualTo("https://umbraco.com"));
                Assert.That(serverUpdated.IsActive, Is.EqualTo(true));   
            }            
        }

        [Test]
        public void Can_Perform_Delete_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new ServerRegistrationRepository(unitOfWork))
            {
                // Act
                var server = repository.Get(3);
                Assert.IsNotNull(server);
                repository.Delete(server);
                unitOfWork.Commit();

                var exists = repository.Exists(3);

                // Assert
                Assert.That(exists, Is.False);   
            }            
        }

        [Test]
        public void Can_Perform_Exists_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new ServerRegistrationRepository(unitOfWork))
            {
                // Act
                var exists = repository.Exists(3);
                var doesntExist = repository.Exists(10);

                // Assert
                Assert.That(exists, Is.True);
                Assert.That(doesntExist, Is.False);    
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public void CreateTestData()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            using (var unitOfWork = provider.GetUnitOfWork())
            using (var repository = new ServerRegistrationRepository(unitOfWork))
            {
                repository.AddOrUpdate(new ServerRegistration("http://localhost", "COMPUTER1", DateTime.Now) { IsActive = true });
                repository.AddOrUpdate(new ServerRegistration("http://www.mydomain.com", "COMPUTER2", DateTime.Now));
                repository.AddOrUpdate(new ServerRegistration("https://www.another.domain.com", "Computer3", DateTime.Now));
                unitOfWork.Commit();
            }

        }
    }
}