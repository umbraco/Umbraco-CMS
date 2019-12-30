﻿using System;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class MacroServiceTests : TestWithSomeContentBase
    {
        public override void CreateTestData()
        {
            base.CreateTestData();

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new MacroRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>(), ShortStringHelper);

                repository.Save(new Macro(ShortStringHelper, "test1", "Test1", "~/views/macropartials/test1.cshtml", MacroTypes.PartialView));
                repository.Save(new Macro(ShortStringHelper, "test2", "Test2", "~/views/macropartials/test2.cshtml", MacroTypes.PartialView));
                repository.Save(new Macro(ShortStringHelper, "test3", "Tet3", "~/views/macropartials/test3.cshtml", MacroTypes.PartialView));
                scope.Complete();
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

            // Act
            var macro = MacroService.GetByAlias("test1");

            //assert
            Assert.IsNotNull(macro);
            Assert.AreEqual("Test1", macro.Name);
        }

        [Test]
        public void Can_Get_All()
        {
            // Arrange

            // Act
            var result = MacroService.GetAll();

            //assert
            Assert.AreEqual(3, result.Count());
        }

        [Test]
        public void Can_Create()
        {
            // Arrange

            // Act
            var macro = new Macro(ShortStringHelper, "test", "Test", "~/Views/MacroPartials/Test.cshtml", MacroTypes.PartialView, cacheDuration: 1234);
            MacroService.Save(macro);

            //assert
            Assert.IsTrue(macro.HasIdentity);
            Assert.Greater(macro.Id, 0);
            Assert.AreNotEqual(Guid.Empty, macro.Key);
            var result = MacroService.GetById(macro.Id);
            Assert.AreEqual("test", result.Alias);
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual("~/Views/MacroPartials/Test.cshtml", result.MacroSource);
            Assert.AreEqual(1234, result.CacheDuration);

            result = MacroService.GetById(macro.Key);
            Assert.AreEqual("test", result.Alias);
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual("~/Views/MacroPartials/Test.cshtml", result.MacroSource);
            Assert.AreEqual(1234, result.CacheDuration);
        }

        [Test]
        public void Can_Delete()
        {
            // Arrange
            var macro = new Macro(ShortStringHelper, "test", "Test", "~/Views/MacroPartials/Test.cshtml", MacroTypes.PartialView, cacheDuration: 1234);
            MacroService.Save(macro);

            // Act
            MacroService.Delete(macro);

            //assert
            var result = MacroService.GetById(macro.Id);
            Assert.IsNull(result);

            result = MacroService.GetById(macro.Key);
            Assert.IsNull(result);
        }

        [Test]
        public void Can_Update()
        {
            // Arrange
            IMacro macro = new Macro(ShortStringHelper, "test", "Test", "~/Views/MacroPartials/Test.cshtml", MacroTypes.PartialView, cacheDuration: 1234);
            MacroService.Save(macro);

            // Act
            var currKey = macro.Key;
            macro.Name = "New name";
            macro.Alias = "NewAlias";
            MacroService.Save(macro);


            macro = MacroService.GetById(macro.Id);

            //assert
            Assert.AreEqual("New name", macro.Name);
            Assert.AreEqual("NewAlias", macro.Alias);
            Assert.AreEqual(currKey, macro.Key);

        }

        [Test]
        public void Can_Update_Property()
        {
            // Arrange
            IMacro macro = new Macro(ShortStringHelper, "test", "Test", "~/Views/MacroPartials/Test.cshtml", MacroTypes.PartialView, cacheDuration: 1234);
            macro.Properties.Add(new MacroProperty("blah", "Blah", 0, "blah"));
            MacroService.Save(macro);

            Assert.AreNotEqual(Guid.Empty, macro.Properties[0].Key);

            // Act
            var currPropKey = macro.Properties[0].Key;
            macro.Properties[0].Alias = "new Alias";
            macro.Properties[0].Name = "new Name";
            macro.Properties[0].SortOrder = 1;
            macro.Properties[0].EditorAlias = "new";
            MacroService.Save(macro);

            macro = MacroService.GetById(macro.Id);

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
            IMacro macro = new Macro(ShortStringHelper, "test", "Test", "~/Views/MacroPartials/Test.cshtml", MacroTypes.PartialView, cacheDuration: 1234);
            macro.Properties.Add(new MacroProperty("blah1", "Blah1", 0, "blah1"));
            macro.Properties.Add(new MacroProperty("blah2", "Blah2", 1, "blah2"));
            macro.Properties.Add(new MacroProperty("blah3", "Blah3", 2, "blah3"));
            MacroService.Save(macro);

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

            var allPropKeys = macro.Properties.Values.Select(x => new { x.Alias, x.Key }).ToArray();

            MacroService.Save(macro);

            macro = MacroService.GetById(macro.Id);

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
            var macro = new Macro(ShortStringHelper, "test", "Test", "~/Views/MacroPartials/Test.cshtml", MacroTypes.PartialView, cacheDuration: 1234);

            //adds some properties
            macro.Properties.Add(new MacroProperty("blah1", "Blah1", 0, "blah1"));
            macro.Properties.Add(new MacroProperty("blah2", "Blah2", 0, "blah2"));
            macro.Properties.Add(new MacroProperty("blah3", "Blah3", 0, "blah3"));
            macro.Properties.Add(new MacroProperty("blah4", "Blah4", 0, "blah4"));
            MacroService.Save(macro);

            var result1 = MacroService.GetById(macro.Id);
            Assert.AreEqual(4, result1.Properties.Values.Count());

            //simulate clearing the sections
            foreach (var s in result1.Properties.Values.ToArray())
            {
                result1.Properties.Remove(s.Alias);
            }
            //now just re-add a couple
            result1.Properties.Add(new MacroProperty("blah3", "Blah3", 0, "blah3"));
            result1.Properties.Add(new MacroProperty("blah4", "Blah4", 0, "blah4"));
            MacroService.Save(result1);

            //assert

            //re-get
            result1 = MacroService.GetById(result1.Id);
            Assert.AreEqual(2, result1.Properties.Values.Count());

        }

        [Test]
        public void Cannot_Save_Macro_With_Empty_Name()
        {
            // Arrange

            var macro = new Macro(ShortStringHelper, "test", string.Empty, "~/Views/MacroPartials/Test.cshtml", MacroTypes.PartialView, cacheDuration: 1234);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => MacroService.Save(macro));
        }

        //[Test]
        //public void Can_Get_Many_By_Alias()
        //{
        //    // Arrange

        //    // Act
        //    var result = MacroService.GetAll("test1", "test2");

        //    //assert
        //    Assert.AreEqual(2, result.Count());
        //}

    }
}
