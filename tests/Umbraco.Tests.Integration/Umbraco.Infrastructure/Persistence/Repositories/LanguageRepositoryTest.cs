// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class LanguageRepositoryTest : UmbracoIntegrationTest
{
    [SetUp]
    public async Task SetUp() => await CreateTestData();

    [Test]
    public async Task Can_Perform_Get_On_LanguageRepository()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var language = await repository.GetAsync(1, CancellationToken.None);

            // Assert
            Assert.That(language, Is.Not.Null);
            Assert.That(language.HasIdentity, Is.True);
            Assert.That(language.CultureName, Is.EqualTo("English (United States)"));
            Assert.That(language.IsoCode, Is.EqualTo("en-US"));
            Assert.That(language.FallbackIsoCode, Is.Null);
            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Get_By_Iso_Code_On_LanguageRepository()
    {
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            var au = CultureInfo.GetCultureInfo("en-AU");
            ILanguage language = new Language(au.Name, au.EnglishName) { FallbackIsoCode = "en-US" };
            await repository.SaveAsync(language, CancellationToken.None);

            // re-get
            language = await repository.GetByIsoCodeAsync(au.Name);

            // Assert
            Assert.That(language, Is.Not.Null);
            Assert.That(language.HasIdentity, Is.True);
            Assert.That(language.CultureName, Is.EqualTo(au.EnglishName));
            Assert.That(language.IsoCode, Is.EqualTo(au.Name));
            Assert.That(language.FallbackIsoCode, Is.EqualTo("en-US"));
            scope.Complete();
        }
    }

    [Test]
    public async Task Get_When_Id_Doesnt_Exist_Returns_Null()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var language = await repository.GetAsync(0,  CancellationToken.None);

            // Assert
            Assert.That(language, Is.Null);
            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_GetAll_On_LanguageRepository()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var allLanguage = await repository.GetManyAsync(null, CancellationToken.None);
            var languages = allLanguage.ToArray();

            // Assert
            Assert.That(languages, Is.Not.Null);
            Assert.That(languages.Any(), Is.True);
            Assert.That(languages.Any(x => x == null), Is.False);
            Assert.That(languages.Count(), Is.EqualTo(5));

            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_GetAll_With_Params_On_LanguageRepository()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var selectedLanguages = await repository.GetManyAsync([1, 2], CancellationToken.None);
            var languages = selectedLanguages.ToArray();

            // Assert
            Assert.That(languages, Is.Not.Null);
            Assert.That(languages.Any(), Is.True);
            Assert.That(languages.Any(x => x == null), Is.False);
            Assert.That(languages.Count(), Is.EqualTo(2));

            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Add_On_LanguageRepository()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var languageBR = new Language("pt-BR", "Portuguese (Brazil)");
            await repository.SaveAsync(languageBR, CancellationToken.None);

            // Assert
            Assert.That(languageBR.HasIdentity, Is.True);
            Assert.That(languageBR.Id, Is.EqualTo(6)); // With 5 existing entries the Id should be 6
            Assert.IsFalse(languageBR.IsDefault);
            Assert.IsFalse(languageBR.IsMandatory);
            Assert.IsNull(languageBR.FallbackIsoCode);
            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Add_On_LanguageRepository_With_Boolean_Properties()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var languageBR = new Language("pt-BR", "Portuguese (Brazil)") { IsDefault = true, IsMandatory = true };
            await repository.SaveAsync(languageBR, CancellationToken.None);

            // Assert
            Assert.That(languageBR.HasIdentity, Is.True);
            Assert.That(languageBR.Id, Is.EqualTo(6)); // With 5 existing entries the Id should be 6
            Assert.IsTrue(languageBR.IsDefault);
            Assert.IsTrue(languageBR.IsMandatory);
            Assert.IsNull(languageBR.FallbackIsoCode);
            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Add_On_LanguageRepository_With_Fallback_Language()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var languageBR = new Language("pt-BR", "Portuguese (Brazil)") { FallbackIsoCode = "en-US" };
            await repository.SaveAsync(languageBR, CancellationToken.None);

            // Assert
            Assert.That(languageBR.HasIdentity, Is.True);
            Assert.That(languageBR.Id, Is.EqualTo(6)); // With 5 existing entries the Id should be 6
            Assert.That(languageBR.FallbackIsoCode, Is.EqualTo("en-US"));
            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Add_On_LanguageRepository_With_New_Default()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            ILanguage languageBR = new Language("pt-BR", "Portuguese (Brazil)") { IsDefault = true, IsMandatory = true };
            await repository.SaveAsync(languageBR, CancellationToken.None);
            var languageEN = new Language("en-AU", "English (Australia)");
            await repository.SaveAsync(languageEN, CancellationToken.None);

            Assert.IsTrue(languageBR.IsDefault);
            Assert.IsTrue(languageBR.IsMandatory);

            // Act
            var languageNZ = new Language("en-NZ", "English (New Zealand)") { IsDefault = true, IsMandatory = true };
            await repository.SaveAsync(languageNZ, CancellationToken.None);
            languageBR = await repository.GetAsync(languageBR.Id, CancellationToken.None);

            // Assert
            Assert.IsFalse(languageBR.IsDefault);
            Assert.IsTrue(languageNZ.IsDefault);
            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Update_On_LanguageRepository()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var language = await repository.GetAsync(5, CancellationToken.None);
            language.IsoCode = "pt-BR";
            language.CultureName = "Portuguese (Brazil)";
            language.FallbackIsoCode = "en-US";

            await repository.SaveAsync(language, CancellationToken.None);

            var languageUpdated = await repository.GetAsync(5, CancellationToken.None);

            // Assert
            Assert.That(languageUpdated, Is.Not.Null);
            Assert.That(languageUpdated.IsoCode, Is.EqualTo("pt-BR"));
            Assert.That(languageUpdated.CultureName, Is.EqualTo("Portuguese (Brazil)"));
            Assert.That(languageUpdated.FallbackIsoCode, Is.EqualTo("en-US"));
            scope.Complete();
        }
    }

    [Test]
    public async Task Perform_Update_With_Existing_Culture()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var language = await repository.GetAsync(5, CancellationToken.None);
            language.IsoCode = "da-DK";
            language.CultureName = "Danish (Denmark)";

            Assert.ThrowsAsync<InvalidOperationException>(async () => await repository.SaveAsync(language, CancellationToken.None));
            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Delete_On_LanguageRepository()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var language = await repository.GetAsync(3, CancellationToken.None);
            await repository.DeleteAsync(language, CancellationToken.None);

            var exists = await repository.ExistsAsync(3, CancellationToken.None);

            // Assert
            Assert.That(exists, Is.False);
            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Delete_On_LanguageRepository_With_Language_Used_As_Fallback()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            // Add language to delete as a fall-back language to another one
            var repository = CreateRepository();
            var languageToFallbackFrom = await repository.GetAsync(5, CancellationToken.None);
            languageToFallbackFrom.FallbackIsoCode = "da-DK"; // fall back to "da-DK" (something we can delete)
            await repository.SaveAsync(languageToFallbackFrom, CancellationToken.None);

            // delete #2
            var languageToDelete = await repository.GetAsync(2, CancellationToken.None);
            await repository.DeleteAsync(languageToDelete, CancellationToken.None);

            var exists = await repository.ExistsAsync(2, CancellationToken.None);

            // has been deleted
            Assert.That(exists, Is.False);
            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Exists_On_LanguageRepository()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var exists = await repository.ExistsAsync(3, CancellationToken.None);
            var doesntExist = await repository.ExistsAsync(10, CancellationToken.None);

            // Assert
            Assert.That(exists, Is.True);
            Assert.That(doesntExist, Is.False);
            scope.Complete();
        }
    }

    private LanguageRepository CreateRepository() => new(GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>(), AppCaches.Disabled, LoggerFactory.CreateLogger<LanguageRepository>(), Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>(), GetRequiredService<IEFCoreScopeProvider<UmbracoDbContext>>());

    private async Task CreateTestData()
    {
        var languageService = GetRequiredService<ILanguageService>();

        //Id 1 is en-US - when Umbraco is installed

        var languageDK = new Language("da-DK", "Danish (Denmark)");
        await languageService.CreateAsync(languageDK, Constants.Security.SuperUserKey); //Id 2

        var languageSE = new Language("sv-SE", "Swedish (Sweden)");
        await languageService.CreateAsync(languageSE, Constants.Security.SuperUserKey); //Id 3

        var languageDE = new Language("de-DE", "German (Germany)");
        await languageService.CreateAsync(languageDE, Constants.Security.SuperUserKey); //Id 4

        var languagePT = new Language("pt-PT", "Portuguese (Portugal)");
        await languageService.CreateAsync(languagePT, Constants.Security.SuperUserKey); //Id 5
    }
}
