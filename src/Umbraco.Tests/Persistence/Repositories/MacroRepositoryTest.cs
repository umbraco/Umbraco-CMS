using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    public class MacroRepositoryTest
    {
        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            var repository = new MacroRepository(unitOfWork, NullCacheProvider.Current);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Cant_Find_Macro_File()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new MacroRepository(unitOfWork, NullCacheProvider.Current);

            // Act
            bool exists = repository.Exists("commentList");

            // Assert
            Assert.That(exists, Is.False);
        }

        [Test]
        public void Can_Verify_Macro_File_Exists()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new MacroRepository(unitOfWork, NullCacheProvider.Current);

            // Act
            bool exists = repository.Exists("testMacro");

            // Assert
            Assert.That(exists, Is.True);
        }

        [Test]
        public void Can_Perform_Add_On_MacroRepository()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new MacroRepository(unitOfWork, NullCacheProvider.Current);

            // Act
            var macro = new Macro { Alias = "testMacro", CacheByPage = false, CacheByMember = false, DontRender = true, Name = "Test Macro", Xslt = "/xslt/testMacro.xslt", UseInEditor = false };
            macro.Properties = new List<IMacroProperty>();
            macro.Properties.Add(new MacroProperty { Alias = "level", Name = "Level", SortOrder = 0, PropertyType = new Umbraco.Core.Macros.PropertyTypes.Number() });

            repository.AddOrUpdate(macro);
            unitOfWork.Commit();

            // Assert
            Assert.That(macro.CreateDate, Is.GreaterThan(DateTime.MinValue));
            Assert.That(macro.UpdateDate, Is.GreaterThan(DateTime.MinValue));
        }

        [Test]
        public void Can_Perform_Update_On_MacroRepository()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new MacroRepository(unitOfWork, NullCacheProvider.Current);
            var macro = CreateMacro("updateMacro", "Update Macro");
            repository.AddOrUpdate(macro);
            unitOfWork.Commit();

            // Act
            var macro1 = repository.Get("updateMacro");
            macro1.Xslt = "/xslt/updateTestMacro.xslt";
            repository.AddOrUpdate(macro1);
            unitOfWork.Commit();

            var macro2 = repository.Get("updateMacro");

            // Assert
            Assert.That(macro2.CreateDate, Is.EqualTo(macro.CreateDate));
            //Assert.That(macro2.ModifiedDate, Is.GreaterThan(macro.ModifiedDate));
            Assert.That(macro2.Xslt, Is.EqualTo("/xslt/updateTestMacro.xslt"));
        }

        [Test]
        public void Can_Perform_Delete_On_MacroRepository()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new MacroRepository(unitOfWork, NullCacheProvider.Current);
            var macro = CreateMacro("deleteMacro", "Delete Macro");
            repository.AddOrUpdate(macro);
            unitOfWork.Commit();

            // Act
            repository.Delete(macro);
            unitOfWork.Commit();
            var exists = repository.Exists("deleteMacro");

            // Assert
            Assert.That(exists, Is.False);
        }

        [Test]
        public void Can_Perform_Get_On_MacroRepository()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new MacroRepository(unitOfWork, NullCacheProvider.Current);
            var macro = CreateMacro("getMacro", "Get Macro");
            repository.AddOrUpdate(macro);
            unitOfWork.Commit();

            // Act
            var unitOfWork2 = provider.GetUnitOfWork();
            var repository2 = new MacroRepository(unitOfWork2, NullCacheProvider.Current);
            var macro1 = repository2.Get("getMacro");

            // Assert
            Assert.That(macro1.CreateDate, Is.GreaterThan(DateTime.MinValue));
            Assert.That(macro1.UpdateDate, Is.GreaterThan(DateTime.MinValue));
            Assert.That(macro1.Name, Is.EqualTo("Get Macro"));
            Assert.That(macro1.Properties.Any(), Is.True);
            Assert.That(macro1.Properties.Count, Is.EqualTo(2));
        }

        [Test]
        public void Can_Perform_GetAll_On_MacroRepository()
        {
            // Arrange
            var provider = new FileUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new MacroRepository(unitOfWork, NullCacheProvider.Current);

            // Act
            var macros = repository.GetAll();
            var list = macros.ToList();

            // Assert
            Assert.That(list.Any(), Is.True);
            Assert.That(list.Any(x => x == null), Is.False);
            Assert.That(list.Count(), Is.GreaterThan(1));
            Assert.That(list.Any(x => x.Alias == "getMacro"), Is.True);
            Assert.That(list.Any(x => x.Alias == "testMacro"), Is.True);
        }

        private IMacro CreateMacro(string alias, string name)
        {
            var macro = new Macro
            {
                Alias = alias,
                CacheByPage = false,
                CacheByMember = false,
                DontRender = true,
                Name = name,
                Xslt = "/xslt/testMacro.xslt",
                UseInEditor = false
            };

            macro.Properties = new List<IMacroProperty>();
            macro.Properties.Add(new MacroProperty { Alias = "level", Name = "Level", SortOrder = 0, PropertyType = new Umbraco.Core.Macros.PropertyTypes.Number() });
            macro.Properties.Add(new MacroProperty { Alias = "fixedTitle", Name = "Fixed Title", SortOrder = 1, PropertyType = new Umbraco.Core.Macros.PropertyTypes.Text() });

            return macro;
        }
    }
}