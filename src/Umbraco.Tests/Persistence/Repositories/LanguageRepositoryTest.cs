using System.Globalization;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class LanguageRepositoryTest : TestWithDatabaseBase
    {
        public override void SetUp()
        {
            base.SetUp();

            CreateTestData();
        }

        private LanguageRepository CreateRepository(IScopeProvider provider)
        {
            return new LanguageRepository((IScopeAccessor) provider, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>());
        }

        [Test]
        public void Can_Perform_Get_On_LanguageRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                scope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                var repository = CreateRepository(provider);

                // Act
                var language = repository.Get(1);

                // Assert
                Assert.That(language, Is.Not.Null);
                Assert.That(language.HasIdentity, Is.True);
                Assert.That(language.CultureName, Is.EqualTo("English (United States)"));
                Assert.That(language.IsoCode, Is.EqualTo("en-US"));
                Assert.That(language.FallbackLanguageId, Is.Null);
            }
        }

        [Test]
        public void Can_Perform_Get_By_Iso_Code_On_LanguageRepository()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var au = CultureInfo.GetCultureInfo("en-AU");
                var language = (ILanguage)new Language(au.Name)
                {
                    CultureName = au.DisplayName,
                    FallbackLanguageId = 1
                };
                repository.Save(language);

                //re-get
                language = repository.GetByIsoCode(au.Name);

                // Assert
                Assert.That(language, Is.Not.Null);
                Assert.That(language.HasIdentity, Is.True);
                Assert.That(language.CultureName, Is.EqualTo(au.DisplayName));
                Assert.That(language.IsoCode, Is.EqualTo(au.Name));
                Assert.That(language.FallbackLanguageId, Is.EqualTo(1));
            }
        }


        [Test]
        public void Get_When_Id_Doesnt_Exist_Returns_Null()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var language = repository.Get(0);

                // Assert
                Assert.That(language, Is.Null);
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_LanguageRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var languages = repository.GetMany();

                // Assert
                Assert.That(languages, Is.Not.Null);
                Assert.That(languages.Any(), Is.True);
                Assert.That(languages.Any(x => x == null), Is.False);
                Assert.That(languages.Count(), Is.EqualTo(5));
            }
        }

        [Test]
        public void Can_Perform_GetAll_With_Params_On_LanguageRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var languages = repository.GetMany(1, 2);

                // Assert
                Assert.That(languages, Is.Not.Null);
                Assert.That(languages.Any(), Is.True);
                Assert.That(languages.Any(x => x == null), Is.False);
                Assert.That(languages.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_GetByQuery_On_LanguageRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var query = scope.SqlContext.Query<ILanguage>().Where(x => x.IsoCode == "da-DK");
                var result = repository.Get(query);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Any(), Is.True);
                Assert.That(result.FirstOrDefault().CultureName, Is.EqualTo("da-DK"));
            }
        }

        [Test]
        public void Can_Perform_Count_On_LanguageRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var query = scope.SqlContext.Query<ILanguage>().Where(x => x.IsoCode.StartsWith("D"));
                int count = repository.Count(query);

                // Assert
                Assert.That(count, Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_Add_On_LanguageRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var languageBR = new Language("pt-BR") { CultureName = "pt-BR" };
                repository.Save(languageBR);

                // Assert
                Assert.That(languageBR.HasIdentity, Is.True);
                Assert.That(languageBR.Id, Is.EqualTo(6)); //With 5 existing entries the Id should be 6
                Assert.IsFalse(languageBR.IsDefault);
                Assert.IsFalse(languageBR.IsMandatory);
                Assert.IsNull(languageBR.FallbackLanguageId);
            }
        }

        [Test]
        public void Can_Perform_Add_On_LanguageRepository_With_Boolean_Properties()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var languageBR = new Language("pt-BR") { CultureName = "pt-BR", IsDefault = true, IsMandatory = true };
                repository.Save(languageBR);

                // Assert
                Assert.That(languageBR.HasIdentity, Is.True);
                Assert.That(languageBR.Id, Is.EqualTo(6)); //With 5 existing entries the Id should be 6
                Assert.IsTrue(languageBR.IsDefault);
                Assert.IsTrue(languageBR.IsMandatory);
                Assert.IsNull(languageBR.FallbackLanguageId);
            }
        }

        [Test]
        public void Can_Perform_Add_On_LanguageRepository_With_Fallback_Language()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var languageBR = new Language("pt-BR")
                    {
                        CultureName = "pt-BR",
                        FallbackLanguageId = 1
                    };
                repository.Save(languageBR);

                // Assert
                Assert.That(languageBR.HasIdentity, Is.True);
                Assert.That(languageBR.Id, Is.EqualTo(6)); //With 5 existing entries the Id should be 6
                Assert.That(languageBR.FallbackLanguageId, Is.EqualTo(1));
            }
        }

        [Test]
        public void Can_Perform_Add_On_LanguageRepository_With_New_Default()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var languageBR = (ILanguage)new Language("pt-BR") { CultureName = "pt-BR", IsDefault = true, IsMandatory = true };
                repository.Save(languageBR);
                var languageEN = new Language("en-AU") { CultureName = "en-AU" };
                repository.Save(languageEN);

                Assert.IsTrue(languageBR.IsDefault);
                Assert.IsTrue(languageBR.IsMandatory);

                // Act
                var languageNZ = new Language("en-NZ") { CultureName = "en-NZ", IsDefault = true, IsMandatory = true };
                repository.Save(languageNZ);
                languageBR = repository.Get(languageBR.Id);

                // Assert
                Assert.IsFalse(languageBR.IsDefault);
                Assert.IsTrue(languageNZ.IsDefault);
            }
        }

        [Test]
        public void Can_Perform_Update_On_LanguageRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var language = repository.Get(5);
                language.IsoCode = "pt-BR";
                language.CultureName = "pt-BR";
                language.FallbackLanguageId = 1;

                repository.Save(language);

                var languageUpdated = repository.Get(5);

                // Assert
                Assert.That(languageUpdated, Is.Not.Null);
                Assert.That(languageUpdated.IsoCode, Is.EqualTo("pt-BR"));
                Assert.That(languageUpdated.CultureName, Is.EqualTo("pt-BR"));
                Assert.That(languageUpdated.FallbackLanguageId, Is.EqualTo(1));
            }
        }

        [Test]
        public void Can_Perform_Delete_On_LanguageRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var language = repository.Get(3);
                repository.Delete(language);

                var exists = repository.Exists(3);

                // Assert
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Delete_On_LanguageRepository_With_Language_Used_As_Fallback()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                // Add language to delete as a fall-back language to another one
                var repository = CreateRepository(provider);
                var languageToFallbackFrom = repository.Get(5);
                languageToFallbackFrom.FallbackLanguageId = 2; // fall back to #2 (something we can delete)
                repository.Save(languageToFallbackFrom);

                // delete #2
                var languageToDelete = repository.Get(2);
                repository.Delete(languageToDelete);

                var exists = repository.Exists(2);

                // has been deleted
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Exists_On_LanguageRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var exists = repository.Exists(3);
                var doesntExist = repository.Exists(10);

                // Assert
                Assert.That(exists, Is.True);
                Assert.That(doesntExist, Is.False);
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        private void CreateTestData()
        {
            //Id 1 is en-US - when Umbraco is installed

            var languageDK = new Language("da-DK") { CultureName = "da-DK" };
            ServiceContext.LocalizationService.Save(languageDK);//Id 2

            var languageSE = new Language("sv-SE") { CultureName = "sv-SE" };
            ServiceContext.LocalizationService.Save(languageSE);//Id 3

            var languageDE = new Language("de-DE") { CultureName = "de-DE" };
            ServiceContext.LocalizationService.Save(languageDE);//Id 4

            var languagePT = new Language("pt-PT") { CultureName = "pt-PT" };
            ServiceContext.LocalizationService.Save(languagePT);//Id 5
        }
    }
}
