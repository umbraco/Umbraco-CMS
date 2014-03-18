using System.Linq;
using NUnit.Framework;
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

            var provider = new PetaPocoUnitOfWorkProvider();
            using (var unitOfWork = provider.GetUnitOfWork())
            using (var repository = new MacroRepository(unitOfWork))
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
            var result = macroService.GetById(macro.Id);
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
        }

        [Test]
        public void Can_Update()
        {
            // Arrange
            var macroService = ServiceContext.MacroService;
            IMacro macro = new Macro("test", "Test", scriptPath: "~/Views/MacroPartials/Test.cshtml", cacheDuration: 1234);
            macroService.Save(macro);

            // Act
            macro.Name = "New name";
            macro.Alias = "NewAlias";
            macroService.Save(macro);


            macro = macroService.GetById(macro.Id);

            //assert
            Assert.AreEqual("New name", macro.Name);
            Assert.AreEqual("NewAlias", macro.Alias);

        }

        [Test]
        public void Can_Update_Property()
        {
            // Arrange
            var macroService = ServiceContext.MacroService;
            IMacro macro = new Macro("test", "Test", scriptPath: "~/Views/MacroPartials/Test.cshtml", cacheDuration: 1234);
            macro.Properties.Add(new MacroProperty("blah", "Blah", 0, "blah"));
            macroService.Save(macro);

            // Act
            macro.Properties.First().Alias = "new Alias";
            macro.Properties.First().Name = "new Name";
            macro.Properties.First().SortOrder = 1;
            macro.Properties.First().EditorAlias = "new";
            macroService.Save(macro);

            macro = macroService.GetById(macro.Id);

            //assert
            Assert.AreEqual(1, macro.Properties.Count());
            Assert.AreEqual("new Alias", macro.Properties.First().Alias);
            Assert.AreEqual("new Name", macro.Properties.First().Name);
            Assert.AreEqual(1, macro.Properties.First().SortOrder);
            Assert.AreEqual("new", macro.Properties.First().EditorAlias);

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