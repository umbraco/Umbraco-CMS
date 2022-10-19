// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class LanguageRepositoryTest : UmbracoIntegrationTest
{
    [SetUp]
    public void SetUp() => CreateTestData();

    [Test]
    public void Can_Perform_Get_On_LanguageRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
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
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var au = CultureInfo.GetCultureInfo("en-AU");
            ILanguage language = new Language(au.Name, au.EnglishName) { FallbackLanguageId = 1 };
            repository.Save(language);

            // re-get
            language = repository.GetByIsoCode(au.Name);

            // Assert
            Assert.That(language, Is.Not.Null);
            Assert.That(language.HasIdentity, Is.True);
            Assert.That(language.CultureName, Is.EqualTo(au.EnglishName));
            Assert.That(language.IsoCode, Is.EqualTo(au.Name));
            Assert.That(language.FallbackLanguageId, Is.EqualTo(1));
        }
    }

    [Test]
    public void Get_When_Id_Doesnt_Exist_Returns_Null()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
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
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var languages = repository.GetMany().ToArray();

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
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var languages = repository.GetMany(1, 2).ToArray();

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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var query = provider.CreateQuery<ILanguage>().Where(x => x.IsoCode == "da-DK");
            var result = repository.Get(query).ToArray();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Any(), Is.True);
            Assert.That(result.FirstOrDefault().IsoCode, Is.EqualTo("da-DK"));
        }
    }

    [Test]
    public void Can_Perform_Count_On_LanguageRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var query = provider.CreateQuery<ILanguage>().Where(x => x.IsoCode.StartsWith("D"));
            var count = repository.Count(query);

            // Assert
            Assert.That(count, Is.EqualTo(2));
        }
    }

    [Test]
    public void Can_Perform_Add_On_LanguageRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var languageBR = new Language("pt-BR", "Portuguese (Brazil)");
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
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var languageBR = new Language("pt-BR", "Portuguese (Brazil)") { IsDefault = true, IsMandatory = true };
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
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var languageBR = new Language("pt-BR", "Portuguese (Brazil)") { FallbackLanguageId = 1 };
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
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            ILanguage languageBR = new Language("pt-BR", "Portuguese (Brazil)") { IsDefault = true, IsMandatory = true };
            repository.Save(languageBR);
            var languageEN = new Language("en-AU", "English (Australia)");
            repository.Save(languageEN);

            Assert.IsTrue(languageBR.IsDefault);
            Assert.IsTrue(languageBR.IsMandatory);

            // Act
            var languageNZ = new Language("en-NZ", "English (New Zealand)") { IsDefault = true, IsMandatory = true };
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
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var language = repository.Get(5);
            language.IsoCode = "pt-BR";
            language.CultureName = "Portuguese (Brazil)";
            language.FallbackLanguageId = 1;

            repository.Save(language);

            var languageUpdated = repository.Get(5);

            // Assert
            Assert.That(languageUpdated, Is.Not.Null);
            Assert.That(languageUpdated.IsoCode, Is.EqualTo("pt-BR"));
            Assert.That(languageUpdated.CultureName, Is.EqualTo("Portuguese (Brazil)"));
            Assert.That(languageUpdated.FallbackLanguageId, Is.EqualTo(1));
        }
    }

    [Test]
    public void Perform_Update_With_Existing_Culture()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var language = repository.Get(5);
            language.IsoCode = "da-DK";
            language.CultureName = "Danish (Denmark)";

            Assert.Throws<InvalidOperationException>(() => repository.Save(language));
        }
    }

    [Test]
    public void Can_Perform_Delete_On_LanguageRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
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
        var provider = ScopeProvider;
        using (provider.CreateScope())
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
        var provider = ScopeProvider;
        using (provider.CreateScope())
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

    private LanguageRepository CreateRepository(IScopeProvider provider) => new((IScopeAccessor)provider, AppCaches.Disabled, LoggerFactory.CreateLogger<LanguageRepository>());

    private void CreateTestData()
    {
        var localizationService = GetRequiredService<ILocalizationService>();

        //Id 1 is en-US - when Umbraco is installed

        var languageDK = new Language("da-DK", "Danish (Denmark)");
        localizationService.Save(languageDK); //Id 2

        var languageSE = new Language("sv-SE", "Swedish (Sweden)");
        localizationService.Save(languageSE); //Id 3

        var languageDE = new Language("de-DE", "German (Germany)");
        localizationService.Save(languageDE); //Id 4

        var languagePT = new Language("pt-PT", "Portuguese (Portugal)");
        localizationService.Save(languagePT); //Id 5
    }
}
