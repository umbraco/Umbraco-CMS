using System.Linq;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering all methods in the LocalizationService class.
    /// This is more of an integration test as it involves multiple layers
    /// as well as configuration.
    /// </summary>
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture, RequiresSTA]
    public class LocalizationServiceTests : BaseServiceTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void Find_BaseData_Language()
        {
            // Arrange
            var localizationService = ServiceContext.LocalizationService;
            
            // Act
            var languages = localizationService.GetAllLanguages();

            // Assert 
            Assert.That(1, Is.EqualTo(languages.Count()));
        }

        [Test]
        public void Save_Language_And_GetLanguageByIsoCode()
        {
            // Arrange
            var localizationService = ServiceContext.LocalizationService;
            var isoCode = "en-GB";
            var language = new Core.Models.Language(isoCode);

            // Act
            localizationService.Save(language);
            var result = localizationService.GetLanguageByIsoCode(isoCode);

            // Assert
            Assert.NotNull(result);
        }

        [Test]
        public void Save_Language_And_GetLanguageById()
        {
            var localizationService = ServiceContext.LocalizationService;
            var isoCode = "en-GB";
            var language = new Core.Models.Language(isoCode);

            // Act
            localizationService.Save(language);
            var result = localizationService.GetLanguageById(language.Id);

            // Assert
            Assert.NotNull(result);
        }

        [Test]
        public void Deleted_Language_Should_Not_Exist()
        {
            var localizationService = ServiceContext.LocalizationService;
            var isoCode = "en-GB";
            var language = new Core.Models.Language(isoCode);
            localizationService.Save(language);

            // Act
            localizationService.Delete(language);
            var result = localizationService.GetLanguageByIsoCode(isoCode);

            // Assert
            Assert.Null(result);
        }
    }
}