using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    public class LanguageRepositoryTest : BaseDatabaseFactoryTest
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
            var repository = new LanguageRepository(unitOfWork);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Perform_Get_On_LanguageRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new LanguageRepository(unitOfWork);

            // Act
            var language = repository.Get(1);

            // Assert
            Assert.That(language, Is.Not.Null);
            Assert.That(language.HasIdentity, Is.True);
            Assert.That(language.CultureName, Is.EqualTo("en-US"));
            Assert.That(language.IsoCode, Is.EqualTo("en-US"));
        }

        [Test]
        public void Can_Perform_GetAll_On_LanguageRepository()
        { }

        [Test]
        public void Can_Perform_GetAll_With_Params_On_LanguageRepository()
        { }

        [Test]
        public void Can_Perform_GetByQuery_On_LanguageRepository()
        { }

        [Test]
        public void Can_Perform_Count_On_LanguageRepository()
        { }

        [Test]
        public void Can_Perform_Add_On_LanguageRepository()
        { }

        [Test]
        public void Can_Perform_Update_On_LanguageRepository()
        { }

        [Test]
        public void Can_Perform_Delete_On_LanguageRepository()
        { }

        [Test]
        public void Can_Perform_Exists_On_LanguageRepository()
        { }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public void CreateTestData()
        {
            var languageDK = new Language("da-DK") { CultureName = "da-DK" };
            ServiceContext.LocalizationService.Save(languageDK);//Id 2

            var languageSE = new Language("sv-SE") { CultureName = "sv-SE" };
            ServiceContext.LocalizationService.Save(languageSE);//Id 3
        }
    }
}