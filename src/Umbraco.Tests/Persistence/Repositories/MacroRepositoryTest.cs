using System.Data.SqlServerCe;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;

using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
            {
                var macro = new Macro("test1", "Test", "~/usercontrol/blah.ascx", "MyAssembly", "test.xslt", "~/views/macropartials/test.cshtml");
                repository.AddOrUpdate(macro);

                Assert.Throws<SqlCeException>(unitOfWork.Commit);
            }

        }

        [Test]
        public void Cannot_Update_To_Duplicate_Macro_Alias()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
            {
                var macro = repository.Get(1);
                macro.Alias = "test2";
                repository.AddOrUpdate(macro);
                Assert.Throws<SqlCeException>(unitOfWork.Commit);
            }

        }

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
            {
                // Assert
                Assert.That(repository, Is.Not.Null);
            }
        }

        [Test]
        public void Can_Perform_Get_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
            {
                // Act
                var macro = repository.Get(1);

                // Assert
                Assert.That(macro, Is.Not.Null);
                Assert.That(macro.HasIdentity, Is.True);
                Assert.That(macro.Alias, Is.EqualTo("test1"));
                Assert.That(macro.CacheByPage, Is.EqualTo(false));
                Assert.That(macro.CacheByMember, Is.EqualTo(false));
                Assert.That(macro.ControlAssembly, Is.EqualTo("MyAssembly1"));
                Assert.That(macro.ControlType, Is.EqualTo("~/usercontrol/test1.ascx"));
                Assert.That(macro.DontRender, Is.EqualTo(true));
                Assert.That(macro.Name, Is.EqualTo("Test1"));
                Assert.That(macro.CacheDuration, Is.EqualTo(0));
                Assert.That(macro.ScriptPath, Is.EqualTo("~/views/macropartials/test1.cshtml"));
                Assert.That(macro.UseInEditor, Is.EqualTo(false));
                Assert.That(macro.XsltPath, Is.EqualTo("test1.xslt"));
            }


        }

        [Test]
        public void Can_Perform_GetAll_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
            {
                // Act
                var query = Query<IMacro>.Builder.Where(x => x.Alias.ToUpper() == "TEST1");
                var result = repository.GetByQuery(query);

                // Assert
                Assert.AreEqual(1, result.Count());
            }
        }

        [Test]
        public void Can_Perform_Count_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
            {
                // Act
                var query = Query<IMacro>.Builder.Where(x => x.Name.StartsWith("Test"));
                int count = repository.Count(query);

                // Assert
                Assert.That(count, Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_Add_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
            {
                // Act
                var macro = new Macro("test", "Test", "~/usercontrol/blah.ascx", "MyAssembly", "test.xslt", "~/views/macropartials/test.cshtml");
                macro.Properties.Add(new MacroProperty("test", "Test", 0, "test"));
                repository.AddOrUpdate(macro);
                unitOfWork.Commit();

                // Assert
                Assert.That(macro.HasIdentity, Is.True);
                Assert.That(macro.Id, Is.EqualTo(4));//With 3 existing entries the Id should be 4   
                Assert.Greater(macro.Properties.Single().Id, 0);
            }
        }

        [Test]
        public void Can_Perform_Update_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
            {
                // Act
                var macro = repository.Get(2);
                macro.Name = "Hello";
                macro.CacheDuration = 1234;
                macro.CacheByPage = true;
                macro.CacheByMember = true;
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
                Assert.That(macroUpdated.CacheDuration, Is.EqualTo(1234));
                Assert.That(macroUpdated.CacheByPage, Is.EqualTo(true));
                Assert.That(macroUpdated.CacheByMember, Is.EqualTo(true));
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
            {
                // Act
                var exists = repository.Exists(3);
                var doesntExist = repository.Exists(10);

                // Assert
                Assert.That(exists, Is.True);
                Assert.That(doesntExist, Is.False);
            }
        }

        [Test]
        public void Can_Add_Property_For_Macro()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
            {
                var macro = repository.Get(1);
                macro.Properties.Add(new MacroProperty("new1", "New1", 3, "test"));
                
                repository.AddOrUpdate(macro);

                unitOfWork.Commit();

                // Assert
                Assert.Greater(macro.Properties.First().Id, 0); //ensure id is returned
                var result = repository.Get(1);
                Assert.Greater(result.Properties.First().Id, 0);
                Assert.AreEqual(1, result.Properties.Count());
                Assert.AreEqual("new1", result.Properties.First().Alias);
                Assert.AreEqual("New1", result.Properties.First().Name);
                Assert.AreEqual(3, result.Properties.First().SortOrder);
                
            }            
        }

        [Test]
        public void Can_Add_New_Macro_With_Property()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
            {
                var macro = new Macro("newmacro", "A new macro", "~/usercontrol/test1.ascx", "MyAssembly1", "test1.xslt", "~/views/macropartials/test1.cshtml");
                macro.Properties.Add(new MacroProperty("blah1", "New1", 4, "test.editor"));

                repository.AddOrUpdate(macro);

                unitOfWork.Commit();

                // Assert
                var result = repository.Get(macro.Id);
                Assert.AreEqual(1, result.Properties.Count());
                Assert.AreEqual("blah1", result.Properties.First().Alias);
                Assert.AreEqual("New1", result.Properties.First().Name);
                Assert.AreEqual(4, result.Properties.First().SortOrder);

            }
        }

        [Test]
        public void Can_Remove_Macro_Property()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
            {
                var macro = new Macro("newmacro", "A new macro", "~/usercontrol/test1.ascx", "MyAssembly1", "test1.xslt", "~/views/macropartials/test1.cshtml");
                macro.Properties.Add(new MacroProperty("blah1", "New1", 4, "test.editor"));
                repository.AddOrUpdate(macro);
                unitOfWork.Commit();

                var result = repository.Get(macro.Id);
                result.Properties.Remove("blah1");
                repository.AddOrUpdate(result);
                unitOfWork.Commit();

                // Assert
                result = repository.Get(macro.Id);
                Assert.AreEqual(0, result.Properties.Count());
                
            }
        }

        [Test]
        public void Can_Add_Remove_Macro_Properties()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
            {
                var macro = new Macro("newmacro", "A new macro", "~/usercontrol/test1.ascx", "MyAssembly1", "test1.xslt", "~/views/macropartials/test1.cshtml");
                var prop1 = new MacroProperty("blah1", "New1", 4, "test.editor");
                var prop2 = new MacroProperty("blah2", "New2", 3, "test.editor");

                //add/remove a few to test the collection observable
                macro.Properties.Add(prop1);
                macro.Properties.Add(prop2);
                macro.Properties.Remove(prop1);
                macro.Properties.Remove("blah2");
                macro.Properties.Add(prop2);
                
                repository.AddOrUpdate(macro);
                unitOfWork.Commit();

                // Assert
                var result = repository.Get(macro.Id);
                
                Assert.AreEqual(1, result.Properties.Count());
                Assert.AreEqual("blah2", result.Properties.Single().Alias);

            }
        }

        [Test]
        public void Can_Update_Property_For_Macro()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
            {
                var macro = repository.Get(1);
                macro.Properties.Add(new MacroProperty("new1", "New1", 3, "test"));
                repository.AddOrUpdate(macro);
                unitOfWork.Commit();

                //Act 
                macro = repository.Get(1);
                macro.Properties["new1"].Name = "this is a new name";
                repository.AddOrUpdate(macro);
                unitOfWork.Commit();


                // Assert
                var result = repository.Get(1);                
                Assert.AreEqual("new1", result.Properties.First().Alias);
                Assert.AreEqual("this is a new name", result.Properties.First().Name);

            }
        }

        [Test]
        public void Can_Update_Macro_Property_Alias()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
            {
                var macro = repository.Get(1);
                macro.Properties.Add(new MacroProperty("new1", "New1", 3, "test"));
                repository.AddOrUpdate(macro);
                unitOfWork.Commit();

                //Act 
                macro = repository.Get(1);
                macro.Properties.UpdateProperty("new1", newAlias: "newAlias");
                repository.AddOrUpdate(macro);
                unitOfWork.Commit();
                
                // Assert
                var result = repository.Get(1);
                Assert.AreEqual("newAlias", result.Properties.First().Alias);
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public void CreateTestData()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.GetUnitOfWork())
            using (var repository = new MacroRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
            {
                repository.AddOrUpdate(new Macro("test1", "Test1", "~/usercontrol/test1.ascx", "MyAssembly1", "test1.xslt", "~/views/macropartials/test1.cshtml"));
                repository.AddOrUpdate(new Macro("test2", "Test2", "~/usercontrol/test2.ascx", "MyAssembly2", "test2.xslt", "~/views/macropartials/test2.cshtml"));
                repository.AddOrUpdate(new Macro("test3", "Tet3", "~/usercontrol/test3.ascx", "MyAssembly3", "test3.xslt", "~/views/macropartials/test3.cshtml"));
                unitOfWork.Commit();
            }

        }
    }
}