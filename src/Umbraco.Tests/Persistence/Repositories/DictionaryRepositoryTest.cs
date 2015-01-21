using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;

using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class DictionaryRepositoryTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            CreateTestData();
        }

        private DictionaryRepository CreateRepository(IDatabaseUnitOfWork unitOfWork, out LanguageRepository languageRepository)
        {
            languageRepository = new LanguageRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);
            var dictionaryRepository = new DictionaryRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), new SqlCeSyntaxProvider(), languageRepository);
            return dictionaryRepository;
        }


        [Test]
        public void Can_Perform_Get_By_Key_On_DictionaryRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            LanguageRepository languageRepository;
            using (var repository = CreateRepository(unitOfWork, out languageRepository))
            {
                var dictionaryItem = (IDictionaryItem)new DictionaryItem("Testing1235")
                {
                    Translations = new List<IDictionaryTranslation>
                    {
                        new DictionaryTranslation(ServiceContext.LocalizationService.GetLanguageByCultureCode("en-US"), "Hello world")
                    }
                };

                repository.AddOrUpdate(dictionaryItem);
                unitOfWork.Commit();

                //re-get
                dictionaryItem = repository.Get("Testing1235");

                // Assert
                Assert.That(dictionaryItem, Is.Not.Null);
                Assert.That(dictionaryItem.ItemKey, Is.EqualTo("Testing1235"));
                Assert.That(dictionaryItem.Translations.Any(), Is.True);
                Assert.That(dictionaryItem.Translations.Any(x => x == null), Is.False);
                Assert.That(dictionaryItem.Translations.First().Value, Is.EqualTo("Hello world"));
            }

        }

        [Test]
        public void Can_Perform_Get_By_UniqueId_On_DictionaryRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            LanguageRepository languageRepository;
            using (var repository = CreateRepository(unitOfWork, out languageRepository))
            {
                var dictionaryItem = (IDictionaryItem)new DictionaryItem("Testing1235")
                {
                    Translations = new List<IDictionaryTranslation>
                    {
                        new DictionaryTranslation(ServiceContext.LocalizationService.GetLanguageByCultureCode("en-US"), "Hello world")
                    }
                };

                repository.AddOrUpdate(dictionaryItem);
                unitOfWork.Commit();

                //re-get
                dictionaryItem = repository.Get(dictionaryItem.Key);

                // Assert
                Assert.That(dictionaryItem, Is.Not.Null);
                Assert.That(dictionaryItem.ItemKey, Is.EqualTo("Testing1235"));
                Assert.That(dictionaryItem.Translations.Any(), Is.True);
                Assert.That(dictionaryItem.Translations.Any(x => x == null), Is.False);
                Assert.That(dictionaryItem.Translations.First().Value, Is.EqualTo("Hello world"));
            }

        }

        [Test]
        public void Can_Perform_Get_On_DictionaryRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            LanguageRepository languageRepository;
            using (var repository = CreateRepository(unitOfWork, out languageRepository))
            {
                var dictionaryItem = (IDictionaryItem)new DictionaryItem("Testing1235")
                {
                    Translations = new List<IDictionaryTranslation>
                    {
                        new DictionaryTranslation(ServiceContext.LocalizationService.GetLanguageByCultureCode("en-US"), "Hello world")
                    }
                };

                repository.AddOrUpdate(dictionaryItem);
                unitOfWork.Commit();

                //re-get
                dictionaryItem = repository.Get(dictionaryItem.Id);
               

                // Assert
                Assert.That(dictionaryItem, Is.Not.Null);
                Assert.That(dictionaryItem.ItemKey, Is.EqualTo("Testing1235"));
                Assert.That(dictionaryItem.Translations.Any(), Is.True);
                Assert.That(dictionaryItem.Translations.Any(x => x == null), Is.False);
                Assert.That(dictionaryItem.Translations.First().Value, Is.EqualTo("Hello world"));
            }

        }

        [Test]
        public void Can_Perform_Get_On_DictionaryRepository_When_No_Language_Assigned()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            LanguageRepository languageRepository;
            using (var repository = CreateRepository(unitOfWork, out languageRepository))
            {
                var dictionaryItem = (IDictionaryItem) new DictionaryItem("Testing1235");                

                repository.AddOrUpdate(dictionaryItem);
                unitOfWork.Commit();

                //re-get
                dictionaryItem = repository.Get(dictionaryItem.Id);


                // Assert
                Assert.That(dictionaryItem, Is.Not.Null);
                Assert.That(dictionaryItem.ItemKey, Is.EqualTo("Testing1235"));
                Assert.That(dictionaryItem.Translations.Any(), Is.False);
            }

        }

        [Test]
        public void Get_Ignores_Item_WhenLanguageMissing()
        {
            // Arrange
            var language = ServiceContext.LocalizationService.GetLanguageByCultureCode("en-US");
            var itemMissingLanguage = new DictionaryItem("I have invalid language");
            var translations = new List<IDictionaryTranslation>
                                   {
                                       new DictionaryTranslation(new Language("") { Id = 0 }, ""),
                                       new DictionaryTranslation(language, "I have language")
                                   };
            itemMissingLanguage.Translations = translations;
            ServiceContext.LocalizationService.Save(itemMissingLanguage);//Id 3?

            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            LanguageRepository languageRepository;
            using (var repository = CreateRepository(unitOfWork, out languageRepository))
            {
                // Act
                var dictionaryItem = repository.Get(3);

                // Assert
                Assert.That(dictionaryItem, Is.Not.Null);
                Assert.That(dictionaryItem.ItemKey, Is.EqualTo("I have invalid language"));
                Assert.That(dictionaryItem.Translations.Any(), Is.True);
                Assert.That(dictionaryItem.Translations.Any(x => x == null), Is.False);
                Assert.That(dictionaryItem.Translations.First().Value, Is.EqualTo("I have language"));
                Assert.That(dictionaryItem.Translations.Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_DictionaryRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            LanguageRepository languageRepository;
            using (var repository = CreateRepository(unitOfWork, out languageRepository))
            {

                // Act
                var dictionaryItem = repository.Get(1);
                var dictionaryItems = repository.GetAll();

                // Assert
                Assert.That(dictionaryItems, Is.Not.Null);
                Assert.That(dictionaryItems.Any(), Is.True);
                Assert.That(dictionaryItems.Any(x => x == null), Is.False);
                Assert.That(dictionaryItems.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_GetAll_With_Params_On_DictionaryRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            LanguageRepository languageRepository;
            using (var repository = CreateRepository(unitOfWork, out languageRepository))
            {

                // Act
                var dictionaryItems = repository.GetAll(1, 2);

                // Assert
                Assert.That(dictionaryItems, Is.Not.Null);
                Assert.That(dictionaryItems.Any(), Is.True);
                Assert.That(dictionaryItems.Any(x => x == null), Is.False);
                Assert.That(dictionaryItems.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_GetByQuery_On_DictionaryRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            LanguageRepository languageRepository;
            using (var repository = CreateRepository(unitOfWork, out languageRepository))
            {

                // Act
                var query = Query<IDictionaryItem>.Builder.Where(x => x.ItemKey == "Article");
                var result = repository.GetByQuery(query);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Any(), Is.True);
                Assert.That(result.FirstOrDefault().ItemKey, Is.EqualTo("Article"));
            }
        }

        [Test]
        public void Can_Perform_Count_On_DictionaryRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            LanguageRepository languageRepository;
            using (var repository = CreateRepository(unitOfWork, out languageRepository))
            {

                // Act
                var query = Query<IDictionaryItem>.Builder.Where(x => x.ItemKey.StartsWith("Read"));
                var result = repository.Count(query);

                // Assert
                Assert.That(result, Is.EqualTo(1));
            }
        }

        [Test]
        public void Can_Perform_Add_On_DictionaryRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            LanguageRepository languageRepository;
            using (var repository = CreateRepository(unitOfWork, out languageRepository))
            {

                var language = languageRepository.Get(1);

                var read = new DictionaryItem("Read");
                var translations = new List<IDictionaryTranslation>
                    {
                        new DictionaryTranslation(language, "Read")
                    };
                read.Translations = translations;

                // Act
                repository.AddOrUpdate(read);
                unitOfWork.Commit();

                var exists = repository.Exists(read.Id);

                // Assert
                Assert.That(read.HasIdentity, Is.True);
                Assert.That(exists, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Update_On_DictionaryRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            LanguageRepository languageRepository;
            using (var repository = CreateRepository(unitOfWork, out languageRepository))
            {

                // Act
                var item = repository.Get(1);
                var translations = item.Translations.ToList();
                translations[0].Value = "Read even more";
                item.Translations = translations;

                repository.AddOrUpdate(item);
                unitOfWork.Commit();

                var dictionaryItem = repository.Get(1);

                // Assert
                Assert.That(dictionaryItem, Is.Not.Null);
                Assert.That(dictionaryItem.Translations.Count(), Is.EqualTo(2));
                Assert.That(dictionaryItem.Translations.FirstOrDefault().Value, Is.EqualTo("Read even more"));
            }
        }

        [Test]
        public void Can_Perform_Update_WithNewTranslation_On_DictionaryRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            var languageRepository = new LanguageRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);
            var repository = new DictionaryRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), new SqlCeSyntaxProvider(), languageRepository);

            var languageNo = new Language("nb-NO") { CultureName = "nb-NO" };
            ServiceContext.LocalizationService.Save(languageNo);

            // Act
            var item = repository.Get(1);
            var translations = item.Translations.ToList();
            translations.Add(new DictionaryTranslation(languageNo, "Les mer"));
            item.Translations = translations;

            repository.AddOrUpdate(item);
            unitOfWork.Commit();

            var dictionaryItem = repository.Get(1);

            // Assert
            Assert.That(dictionaryItem, Is.Not.Null);
            Assert.That(dictionaryItem.Translations.Count(), Is.EqualTo(3));
            Assert.That(dictionaryItem.Translations.Single(t => t.Language.IsoCode == "nb-NO").Value, Is.EqualTo("Les mer"));
        }

        [Test]
        public void Can_Perform_Delete_On_DictionaryRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            LanguageRepository languageRepository;
            using (var repository = CreateRepository(unitOfWork, out languageRepository))
            {

                // Act
                var item = repository.Get(1);
                repository.Delete(item);
                unitOfWork.Commit();

                var exists = repository.Exists(1);

                // Assert
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Exists_On_DictionaryRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            LanguageRepository languageRepository;
            using (var repository = CreateRepository(unitOfWork, out languageRepository))
            {

                // Act
                var exists = repository.Exists(1);

                // Assert
                Assert.That(exists, Is.True);
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public void CreateTestData()
        {
            var language = ServiceContext.LocalizationService.GetLanguageByCultureCode("en-US");

            var languageDK = new Language("da-DK") { CultureName = "da-DK" };
            ServiceContext.LocalizationService.Save(languageDK);//Id 2

            var readMore = new DictionaryItem("Read More");
            var translations = new List<IDictionaryTranslation>
                                   {
                                       new DictionaryTranslation(language, "Read More"),
                                       new DictionaryTranslation(languageDK, "Læs mere")
                                   };
            readMore.Translations = translations;
            ServiceContext.LocalizationService.Save(readMore);//Id 1

            var article = new DictionaryItem("Article");
            var translations2 = new List<IDictionaryTranslation>
                                   {
                                       new DictionaryTranslation(language, "Article"),
                                       new DictionaryTranslation(languageDK, "Artikel")
                                   };
            article.Translations = translations2;
            ServiceContext.LocalizationService.Save(article);//Id 2
        }
    }
}