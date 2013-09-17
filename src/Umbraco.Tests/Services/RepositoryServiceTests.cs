using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Tests.Services
{
    [TestFixture, RequiresSTA]
    public class RepositoryServiceTests : BaseServiceTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            CreateTestData();
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
        public void Can_Get_Many_By_Alias()
        {
            // Arrange
            var macroService = ServiceContext.MacroService;

            // Act
            var result = macroService.GetAll("test1", "test2");

            //assert
            Assert.AreEqual(2, result.Count());
        }

    }
}