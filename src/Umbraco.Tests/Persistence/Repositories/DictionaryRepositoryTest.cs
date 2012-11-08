using NUnit.Framework;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    public class DictionaryRepositoryTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            CreateTestData();
        }

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            var languageRepository = new LanguageRepository(unitOfWork);
            var repository = new DictionaryRepository(unitOfWork, languageRepository);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Perform_Get_On_DictionaryRepository()
        { }

        [Test]
        public void Can_Perform_GetAll_On_DictionaryRepository()
        { }

        [Test]
        public void Can_Perform_GetAll_With_Params_On_DictionaryRepository()
        { }

        [Test]
        public void Can_Perform_GetByQuery_On_DictionaryRepository()
        { }

        [Test]
        public void Can_Perform_Count_On_DictionaryRepository()
        { }

        [Test]
        public void Can_Perform_Add_On_DictionaryRepository()
        { }

        [Test]
        public void Can_Perform_Update_On_DictionaryRepository()
        { }

        [Test]
        public void Can_Perform_Delete_On_DictionaryRepository()
        { }

        [Test]
        public void Can_Perform_Exists_On_DictionaryRepository()
        { }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public void CreateTestData()
        { }
    }
}