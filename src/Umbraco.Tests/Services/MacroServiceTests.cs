﻿using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Services
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture, RequiresSTA]
    public class MacroServiceTests : BaseServiceTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void CreateTestData()
        {
            base.CreateTestData();

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

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void Can_Get_By_Alias()
        {
            // Arrange
            var macroService = ServiceContext.MacroService;

            // Act
            var macro = macroService.GetByAlias("test1");

            //assert
            Assert.IsNotNull(macro);
            Assert.AreEqual("Test1", macro.Name);
        }

        [Test]
        public void Can_Get_All()
        {
            // Arrange
            var macroService = ServiceContext.MacroService;

            // Act
            var result = macroService.GetAll();

            //assert
            Assert.AreEqual(3, result.Count());
        }

        [Test]
        public void Can_Create()
        {
            // Arrange
            var macroService = ServiceContext.MacroService;

            // Act
            var macro = new Macro("test", "Test", scriptPath: "~/Views/MacroPartials/Test.cshtml", cacheDuration: 1234);
            macroService.Save(macro);

            //assert
            Assert.IsTrue(macro.HasIdentity);
            Assert.Greater(macro.Id, 0);
            Assert.AreNotEqual(Guid.Empty, macro.Key);
            var result = macroService.GetById(macro.Id);
            Assert.AreEqual("test", result.Alias);
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual("~/Views/MacroPartials/Test.cshtml", result.ScriptPath);
            Assert.AreEqual(1234, result.CacheDuration);

            result = macroService.GetById(macro.Key);
            Assert.AreEqual("test", result.Alias);
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual("~/Views/MacroPartials/Test.cshtml", result.ScriptPath);
            Assert.AreEqual(1234, result.CacheDuration);
        }

        [Test]
        public void Can_Delete()
        {
            // Arrange
            var macroService = ServiceContext.MacroService;
            var macro = new Macro("test", "Test", scriptPath: "~/Views/MacroPartials/Test.cshtml", cacheDuration: 1234);
            macroService.Save(macro);

            // Act
            macroService.Delete(macro);

            //assert
            var result = macroService.GetById(macro.Id);
            Assert.IsNull(result);

            result = macroService.GetById(macro.Key);
            Assert.IsNull(result);
        }

        [Test]
        public void Can_Update()
        {
            // Arrange
            var macroService = ServiceContext.MacroService;
            IMacro macro = new Macro("test", "Test", scriptPath: "~/Views/MacroPartials/Test.cshtml", cacheDuration: 1234);
            macroService.Save(macro);

            // Act
            var currKey = macro.Key;
            macro.Name = "New name";
            macro.Alias = "NewAlias";
            macroService.Save(macro);


            macro = macroService.GetById(macro.Id);

            //assert
            Assert.AreEqual("New name", macro.Name);
            Assert.AreEqual("NewAlias", macro.Alias);
            Assert.AreEqual(currKey, macro.Key);
        }

        [Test]
        public void Can_Update_Property()
        {
            // Arrange
            var macroService = ServiceContext.MacroService;
            IMacro macro = new Macro("test", "Test", scriptPath: "~/Views/MacroPartials/Test.cshtml", cacheDuration: 1234);
            macro.Properties.Add(new MacroProperty("blah", "Blah", 0, "blah"));
            macroService.Save(macro);

            Assert.AreNotEqual(Guid.Empty, macro.Properties[0].Key);

            // Act
            var currPropKey = macro.Properties[0].Key;
            macro.Properties[0].Alias = "new Alias";
            macro.Properties[0].Name = "new Name";
            macro.Properties[0].SortOrder = 1;
            macro.Properties[0].EditorAlias = "new";
            macroService.Save(macro);

            macro = macroService.GetById(macro.Id);

            //assert
            Assert.AreEqual(1, macro.Properties.Count);
            Assert.AreEqual(currPropKey, macro.Properties[0].Key);
            Assert.AreEqual("new Alias", macro.Properties[0].Alias);
            Assert.AreEqual("new Name", macro.Properties[0].Name);
            Assert.AreEqual(1, macro.Properties[0].SortOrder);
            Assert.AreEqual("new", macro.Properties[0].EditorAlias);
            Assert.AreEqual(currPropKey, macro.Properties[0].Key);

        }

        [Test]
        public void Can_Update_Remove_Property()
        {
            // Arrange
            var macroService = ServiceContext.MacroService;
            IMacro macro = new Macro("test", "Test", scriptPath: "~/Views/MacroPartials/Test.cshtml", cacheDuration: 1234);
            macro.Properties.Add(new MacroProperty("blah1", "Blah1", 0, "blah1"));
            macro.Properties.Add(new MacroProperty("blah2", "Blah2", 1, "blah2"));
            macro.Properties.Add(new MacroProperty("blah3", "Blah3", 2, "blah3"));
            macroService.Save(macro);

            var lastKey = macro.Properties[0].Key;
            for (var i = 1; i < macro.Properties.Count; i++)
            {
                Assert.AreNotEqual(Guid.Empty, macro.Properties[i].Key);
                Assert.AreNotEqual(lastKey, macro.Properties[i].Key);
                lastKey = macro.Properties[i].Key;
            }



            // Act
            macro.Properties["blah1"].Alias = "newAlias";
            macro.Properties["blah1"].Name = "new Name";
            macro.Properties["blah1"].SortOrder = 1;
            macro.Properties["blah1"].EditorAlias = "new";
            macro.Properties.Remove("blah3");

            var allPropKeys = macro.Properties.Select(x => new { x.Alias, x.Key }).ToArray();

            macroService.Save(macro);

            macro = macroService.GetById(macro.Id);

            //assert
            Assert.AreEqual(2, macro.Properties.Count);
            Assert.AreEqual("newAlias", macro.Properties["newAlias"].Alias);
            Assert.AreEqual("new Name", macro.Properties["newAlias"].Name);
            Assert.AreEqual(1, macro.Properties["newAlias"].SortOrder);
            Assert.AreEqual("new", macro.Properties["newAlias"].EditorAlias);
            foreach (var propKey in allPropKeys)
            {
                Assert.AreEqual(propKey.Key, macro.Properties[propKey.Alias].Key);
            }
        }

        [Test]
        public void Can_Add_And_Remove_Properties()
        {
            var macroService = ServiceContext.MacroService;
            var macro = new Macro("test", "Test", scriptPath: "~/Views/MacroPartials/Test.cshtml", cacheDuration: 1234);

            //adds some properties
            macro.Properties.Add(new MacroProperty("blah1", "Blah1", 0, "blah1"));
            macro.Properties.Add(new MacroProperty("blah2", "Blah2", 0, "blah2"));
            macro.Properties.Add(new MacroProperty("blah3", "Blah3", 0, "blah3"));
            macro.Properties.Add(new MacroProperty("blah4", "Blah4", 0, "blah4"));
            macroService.Save(macro);

            var result1 = macroService.GetById(macro.Id);
            Assert.AreEqual(4, result1.Properties.Count());

            //simulate clearing the sections
            foreach (var s in result1.Properties.ToArray())
            {
                result1.Properties.Remove(s.Alias);
            }
            //now just re-add a couple
            result1.Properties.Add(new MacroProperty("blah3", "Blah3", 0, "blah3"));
            result1.Properties.Add(new MacroProperty("blah4", "Blah4", 0, "blah4"));
            macroService.Save(result1);

            //assert

            //re-get
            result1 = macroService.GetById(result1.Id);
            Assert.AreEqual(2, result1.Properties.Count());

        }

        [Test]
        public void Cannot_Save_Macro_With_Empty_Name()
        {
            // Arrange
            var macroService = ServiceContext.MacroService;
            var macro = new Macro("test", string.Empty, scriptPath: "~/Views/MacroPartials/Test.cshtml", cacheDuration: 1234);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => macroService.Save(macro));
        }

        //[Test]
        //public void Can_Get_Many_By_Alias()
        //{
        //    // Arrange
        //    var macroService = ServiceContext.MacroService;

        //    // Act
        //    var result = macroService.GetAll("test1", "test2");

        //    //assert
        //    Assert.AreEqual(2, result.Count());
        //}

    }
}