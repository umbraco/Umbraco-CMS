using System.Data.SqlServerCe;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;

using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class MacroRepositoryTest : TestWithDatabaseBase
    {
        public override void SetUp()
        {
            base.SetUp();

            CreateTestData();
        }

        [Test]
        public void Cannot_Add_Duplicate_Macros()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

                var macro = new Macro("test1", "Test", "~/views/macropartials/test.cshtml", MacroTypes.PartialView);

                Assert.Throws<SqlCeException>(() => repository.Save(macro));
            }

        }

        [Test]
        public void Cannot_Update_To_Duplicate_Macro_Alias()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

                var macro = repository.Get(1);
                macro.Alias = "test2";

                Assert.Throws<SqlCeException>(() => repository.Save(macro));
            }

        }

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

                // Assert
                Assert.That(repository, Is.Not.Null);
            }
        }

        [Test]
        public void Can_Perform_Get_On_Repository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

                // Act
                var macro = repository.Get(1);

                // Assert
                Assert.That(macro, Is.Not.Null);
                Assert.That(macro.HasIdentity, Is.True);
                Assert.That(macro.Alias, Is.EqualTo("test1"));
                Assert.That(macro.CacheByPage, Is.EqualTo(false));
                Assert.That(macro.CacheByMember, Is.EqualTo(false));
                Assert.That(macro.DontRender, Is.EqualTo(true));
                Assert.That(macro.Name, Is.EqualTo("Test1"));
                Assert.That(macro.CacheDuration, Is.EqualTo(0));
                Assert.That(macro.MacroSource, Is.EqualTo("~/views/macropartials/test1.cshtml"));
                Assert.That(macro.UseInEditor, Is.EqualTo(false));
            }


        }

        [Test]
        public void Can_Perform_GetAll_On_Repository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

                // Act
                var macros = repository.GetMany();

                // Assert
                Assert.That(macros.Count(), Is.EqualTo(3));
            }

        }

        [Test]
        public void Can_Perform_GetByQuery_On_Repository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

                // Act
                var query = scope.SqlContext.Query<IMacro>().Where(x => x.Alias.ToUpper() == "TEST1");
                var result = repository.Get(query);

                // Assert
                Assert.AreEqual(1, result.Count());
            }
        }

        [Test]
        public void Can_Perform_Count_On_Repository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

                // Act
                var query = scope.SqlContext.Query<IMacro>().Where(x => x.Name.StartsWith("Test"));
                int count = repository.Count(query);

                // Assert
                Assert.That(count, Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_Add_On_Repository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

                // Act
                var macro = new Macro("test", "Test", "~/views/macropartials/test.cshtml", MacroTypes.PartialView);
                macro.Properties.Add(new MacroProperty("test", "Test", 0, "test"));
                repository.Save(macro);

                // Assert
                Assert.That(macro.HasIdentity, Is.True);
                Assert.That(macro.Id, Is.EqualTo(4));//With 3 existing entries the Id should be 4
                Assert.Greater(macro.Properties.Values.Single().Id, 0);
            }
        }

        [Test]
        public void Can_Perform_Update_On_Repository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

                // Act
                var macro = repository.Get(2);
                macro.Name = "Hello";
                macro.CacheDuration = 1234;
                macro.CacheByPage = true;
                macro.CacheByMember = true;
                macro.DontRender = false;
                macro.MacroSource = "~/newpath.cshtml";
                macro.UseInEditor = true;

                repository.Save(macro);

                var macroUpdated = repository.Get(2);

                // Assert
                Assert.That(macroUpdated, Is.Not.Null);
                Assert.That(macroUpdated.Name, Is.EqualTo("Hello"));
                Assert.That(macroUpdated.CacheDuration, Is.EqualTo(1234));
                Assert.That(macroUpdated.CacheByPage, Is.EqualTo(true));
                Assert.That(macroUpdated.CacheByMember, Is.EqualTo(true));
                Assert.That(macroUpdated.DontRender, Is.EqualTo(false));
                Assert.That(macroUpdated.MacroSource, Is.EqualTo("~/newpath.cshtml"));
                Assert.That(macroUpdated.UseInEditor, Is.EqualTo(true));
            }
        }

        [Test]
        public void Can_Perform_Delete_On_Repository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

                // Act
                var macro = repository.Get(3);
                Assert.IsNotNull(macro);
                repository.Delete(macro);

                var exists = repository.Exists(3);

                // Assert
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Exists_On_Repository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

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
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

                var macro = repository.Get(1);
                macro.Properties.Add(new MacroProperty("new1", "New1", 3, "test"));

                repository.Save(macro);

                // Assert
                Assert.Greater(macro.Properties.Values.First().Id, 0); //ensure id is returned
                var result = repository.Get(1);
                Assert.Greater(result.Properties.Values.First().Id, 0);
                Assert.AreEqual(1, result.Properties.Values.Count());
                Assert.AreEqual("new1", result.Properties.Values.First().Alias);
                Assert.AreEqual("New1", result.Properties.Values.First().Name);
                Assert.AreEqual(3, result.Properties.Values.First().SortOrder);

            }
        }

        [Test]
        public void Can_Add_New_Macro_With_Property()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

                var macro = new Macro("newmacro", "A new macro", "~/views/macropartials/test1.cshtml", MacroTypes.PartialView);
                macro.Properties.Add(new MacroProperty("blah1", "New1", 4, "test.editor"));

                repository.Save(macro);


                // Assert
                var result = repository.Get(macro.Id);
                Assert.AreEqual(1, result.Properties.Values.Count());
                Assert.AreEqual("blah1", result.Properties.Values.First().Alias);
                Assert.AreEqual("New1", result.Properties.Values.First().Name);
                Assert.AreEqual(4, result.Properties.Values.First().SortOrder);

            }
        }

        [Test]
        public void Can_Remove_Macro_Property()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

                var macro = new Macro("newmacro", "A new macro", "~/views/macropartials/test1.cshtml", MacroTypes.PartialView);
                macro.Properties.Add(new MacroProperty("blah1", "New1", 4, "test.editor"));
                repository.Save(macro);

                var result = repository.Get(macro.Id);
                result.Properties.Remove("blah1");
                repository.Save(result);

                // Assert
                result = repository.Get(macro.Id);
                Assert.AreEqual(0, result.Properties.Values.Count());

            }
        }

        [Test]
        public void Can_Add_Remove_Macro_Properties()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

                var macro = new Macro("newmacro", "A new macro", "~/views/macropartials/test1.cshtml", MacroTypes.PartialView);
                var prop1 = new MacroProperty("blah1", "New1", 4, "test.editor");
                var prop2 = new MacroProperty("blah2", "New2", 3, "test.editor");

                //add/remove a few to test the collection observable
                macro.Properties.Add(prop1);
                macro.Properties.Add(prop2);
                macro.Properties.Remove(prop1);
                macro.Properties.Remove("blah2");
                macro.Properties.Add(prop2);

                repository.Save(macro);

                // Assert
                var result = repository.Get(macro.Id);

                Assert.AreEqual(1, result.Properties.Values.Count());
                Assert.AreEqual("blah2", result.Properties.Values.Single().Alias);

            }
        }

        [Test]
        public void Can_Update_Property_For_Macro()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

                var macro = repository.Get(1);
                macro.Properties.Add(new MacroProperty("new1", "New1", 3, "test"));
                repository.Save(macro);

                //Act
                macro = repository.Get(1);
                macro.Properties["new1"].Name = "this is a new name";
                repository.Save(macro);


                // Assert
                var result = repository.Get(1);
                Assert.AreEqual("new1", result.Properties.Values.First().Alias);
                Assert.AreEqual("this is a new name", result.Properties.Values.First().Name);

            }
        }

        [Test]
        public void Can_Update_Macro_Property_Alias()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

                var macro = repository.Get(1);
                macro.Properties.Add(new MacroProperty("new1", "New1", 3, "test"));
                repository.Save(macro);

                //Act
                macro = repository.Get(1);
                macro.Properties.UpdateProperty("new1", newAlias: "newAlias");
                repository.Save(macro);

                // Assert
                var result = repository.Get(1);
                Assert.AreEqual("newAlias", result.Properties.Values.First().Alias);
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public void CreateTestData()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>());

                repository.Save(new Macro("test1", "Test1", "~/views/macropartials/test1.cshtml", MacroTypes.PartialView));
                repository.Save(new Macro("test2", "Test2", "~/views/macropartials/test2.cshtml", MacroTypes.PartialView));
                repository.Save(new Macro("test3", "Tet3", "~/views/macropartials/test3.cshtml", MacroTypes.PartialView));
                scope.Complete();
            }

        }
    }
}
