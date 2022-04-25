// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class LanguageRepositoryTest : UmbracoIntegrationTest
    {
        private GlobalSettings _globalSettings;

        [SetUp]
        public void SetUp()
        {
            CreateTestData();
            _globalSettings = new GlobalSettings();
        }

        [Test]
        public void Can_Perform_Get_On_LanguageRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                LanguageRepository repository = CreateRepository(provider);

                // Act
                ILanguage language = repository.Get(1);

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
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                LanguageRepository repository = CreateRepository(provider);

                var au = CultureInfo.GetCultureInfo("en-AU");
                var language = (ILanguage)new Language(_globalSettings, au.Name)
                {
                    CultureName = au.DisplayName,
                    FallbackLanguageId = 1
                };
                repository.Save(language);

                // re-get
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
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                LanguageRepository repository = CreateRepository(provider);

                // Act
                ILanguage language = repository.Get(0);

                // Assert
                Assert.That(language, Is.Null);
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_LanguageRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                LanguageRepository repository = CreateRepository(provider);

                // Act
                IEnumerable<ILanguage> languages = repository.GetMany();

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
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                LanguageRepository repository = CreateRepository(provider);

                // Act
                IEnumerable<ILanguage> languages = repository.GetMany(1, 2);

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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                LanguageRepository repository = CreateRepository(provider);

                // Act
                IQuery<ILanguage> query = provider.CreateQuery<ILanguage>().Where(x => x.IsoCode == "da-DK");
                IEnumerable<ILanguage> result = repository.Get(query);

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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                LanguageRepository repository = CreateRepository(provider);

                // Act
                IQuery<ILanguage> query = provider.CreateQuery<ILanguage>().Where(x => x.IsoCode.StartsWith("D"));
                int count = repository.Count(query);

                // Assert
                Assert.That(count, Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_Add_On_LanguageRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                LanguageRepository repository = CreateRepository(provider);

                // Act
                var languageBR = new Language(_globalSettings, "pt-BR") { CultureName = "pt-BR" };
                repository.Save(languageBR);

                // Assert
                Assert.That(languageBR.HasIdentity, Is.True);
                Assert.That(languageBR.Id, Is.EqualTo(6)); // With 5 existing entries the Id should be 6
                Assert.IsFalse(languageBR.IsDefault);
                Assert.IsFalse(languageBR.IsMandatory);
                Assert.IsNull(languageBR.FallbackLanguageId);
            }
        }

        [Test]
        public void Can_Perform_Add_On_LanguageRepository_With_Boolean_Properties()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                LanguageRepository repository = CreateRepository(provider);

                // Act
                var languageBR = new Language(_globalSettings, "pt-BR") { CultureName = "pt-BR", IsDefault = true, IsMandatory = true };
                repository.Save(languageBR);

                // Assert
                Assert.That(languageBR.HasIdentity, Is.True);
                Assert.That(languageBR.Id, Is.EqualTo(6)); // With 5 existing entries the Id should be 6
                Assert.IsTrue(languageBR.IsDefault);
                Assert.IsTrue(languageBR.IsMandatory);
                Assert.IsNull(languageBR.FallbackLanguageId);
            }
        }

        [Test]
        public void Can_Perform_Add_On_LanguageRepository_With_Fallback_Language()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                LanguageRepository repository = CreateRepository(provider);

                // Act
                var languageBR = new Language(_globalSettings, "pt-BR")
                {
                    CultureName = "pt-BR",
                    FallbackLanguageId = 1
                };
                repository.Save(languageBR);

                // Assert
                Assert.That(languageBR.HasIdentity, Is.True);
                Assert.That(languageBR.Id, Is.EqualTo(6)); // With 5 existing entries the Id should be 6
                Assert.That(languageBR.FallbackLanguageId, Is.EqualTo(1));
            }
        }

        [Test]
        public void Can_Perform_Add_On_LanguageRepository_With_New_Default()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                LanguageRepository repository = CreateRepository(provider);

                var languageBR = (ILanguage)new Language(_globalSettings, "pt-BR") { CultureName = "pt-BR", IsDefault = true, IsMandatory = true };
                repository.Save(languageBR);
                var languageEN = new Language(_globalSettings, "en-AU") { CultureName = "en-AU" };
                repository.Save(languageEN);

                Assert.IsTrue(languageBR.IsDefault);
                Assert.IsTrue(languageBR.IsMandatory);

                // Act
                var languageNZ = new Language(_globalSettings, "en-NZ") { CultureName = "en-NZ", IsDefault = true, IsMandatory = true };
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
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                LanguageRepository repository = CreateRepository(provider);

                // Act
                ILanguage language = repository.Get(5);
                language.IsoCode = "pt-BR";
                language.CultureName = "pt-BR";
                language.FallbackLanguageId = 1;

                repository.Save(language);

                ILanguage languageUpdated = repository.Get(5);

                // Assert
                Assert.That(languageUpdated, Is.Not.Null);
                Assert.That(languageUpdated.IsoCode, Is.EqualTo("pt-BR"));
                Assert.That(languageUpdated.CultureName, Is.EqualTo("pt-BR"));
                Assert.That(languageUpdated.FallbackLanguageId, Is.EqualTo(1));
            }
        }

        [Test]
        public void Perform_Update_With_Existing_Culture()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                LanguageRepository repository = CreateRepository(provider);

                // Act
                ILanguage language = repository.Get(5);
                language.IsoCode = "da-DK";
                language.CultureName = "da-DK";

                Assert.Throws<InvalidOperationException>(() => repository.Save(language));
            }
        }

        [Test]
        public void Can_Perform_Delete_On_LanguageRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                LanguageRepository repository = CreateRepository(provider);

                // Act
                ILanguage language = repository.Get(3);
                repository.Delete(language);

                bool exists = repository.Exists(3);

                // Assert
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Delete_On_LanguageRepository_With_Language_Used_As_Fallback()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                // Add language to delete as a fall-back language to another one
                LanguageRepository repository = CreateRepository(provider);
                ILanguage languageToFallbackFrom = repository.Get(5);
                languageToFallbackFrom.FallbackLanguageId = 2; // fall back to #2 (something we can delete)
                repository.Save(languageToFallbackFrom);

                // delete #2
                ILanguage languageToDelete = repository.Get(2);
                repository.Delete(languageToDelete);

                bool exists = repository.Exists(2);

                // has been deleted
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Exists_On_LanguageRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                LanguageRepository repository = CreateRepository(provider);

                // Act
                bool exists = repository.Exists(3);
                bool doesntExist = repository.Exists(10);

                // Assert
                Assert.That(exists, Is.True);
                Assert.That(doesntExist, Is.False);
            }
        }

        private LanguageRepository CreateRepository(IScopeProvider provider) => new LanguageRepository((IScopeAccessor)provider, AppCaches.Disabled, LoggerFactory.CreateLogger<LanguageRepository>(), Microsoft.Extensions.Options.Options.Create(_globalSettings));

        private void CreateTestData()
        {
            // Id 1 is en-US - when Umbraco is installed
            ILocalizationService localizationService = GetRequiredService<ILocalizationService>();
            var languageDK = new Language(_globalSettings, "da-DK") { CultureName = "da-DK" };
            localizationService.Save(languageDK); // Id 2

            var languageSE = new Language(_globalSettings, "sv-SE") { CultureName = "sv-SE" };
            localizationService.Save(languageSE); // Id 3

            var languageDE = new Language(_globalSettings, "de-DE") { CultureName = "de-DE" };
            localizationService.Save(languageDE); // Id 4

            var languagePT = new Language(_globalSettings, "pt-PT") { CultureName = "pt-PT" };
            localizationService.Save(languagePT); // Id 5
        }
    }
}
