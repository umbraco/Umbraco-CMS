using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class LanguageServiceTests : UmbracoIntegrationTest
{
    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    [SetUp]
    public async Task SetUp() => await CreateTestData();

    [Test]
    public async Task Can_Get_All_Languages()
    {
        var languages = await LanguageService.GetAllAsync();
        Assert.NotNull(languages);
        Assert.IsTrue(languages.Any());
        Assert.That(languages.Count(), Is.EqualTo(3));
    }

    [Test]
    public async Task Can_GetLanguageByIsoCode()
    {
        var danish = await LanguageService.GetAsync("da-DK");
        var english = await LanguageService.GetAsync("en-GB");
        Assert.NotNull(danish);
        Assert.NotNull(english);
    }

    [Test]
    public async Task Does_Not_Fail_When_Language_Doesnt_Exist()
    {
        var language = await LanguageService.GetAsync("sv-SE");
        Assert.Null(language);
    }

    [Test]
    public async Task Can_Delete_Language()
    {
        var languageNbNo = new LanguageBuilder()
            .WithCultureInfo("nb-NO")
            .Build();
        await LanguageService.CreateAsync(languageNbNo, Constants.Security.SuperUserKey);
        Assert.That(languageNbNo.HasIdentity, Is.True);

        var language = await LanguageService.GetAsync("nb-NO");
        Assert.NotNull(language);

        var result = await LanguageService.DeleteAsync("nb-NO", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        language = await LanguageService.GetAsync("nb-NO");
        Assert.Null(language);
    }

    [Test]
    public async Task Can_Create_Language_With_Fallback()
    {
        var languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.NotNull(languageDaDk);
        var languageNbNo = new LanguageBuilder()
            .WithCultureInfo("nb-NO")
            .WithFallbackLanguageIsoCode(languageDaDk.IsoCode)
            .Build();
        var result = await LanguageService.CreateAsync(languageNbNo, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        var language = await LanguageService.GetAsync("nb-NO");
        Assert.NotNull(language);
        Assert.AreEqual("da-DK", language.FallbackIsoCode);
    }

    [Test]
    public async Task Can_Delete_Language_Used_As_Fallback()
    {
        var languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.NotNull(languageDaDk);
        var languageNbNo = new LanguageBuilder()
            .WithCultureInfo("nb-NO")
            .WithFallbackLanguageIsoCode(languageDaDk.IsoCode)
            .Build();
        var result = await LanguageService.CreateAsync(languageNbNo, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        var language = await LanguageService.GetAsync("nb-NO");
        Assert.NotNull(language);
        Assert.AreEqual("da-DK", language.FallbackIsoCode);

        result = await LanguageService.DeleteAsync("da-DK", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        language = await LanguageService.GetAsync("da-DK");
        Assert.Null(language);

        language = await LanguageService.GetAsync("nb-NO");
        Assert.NotNull(language);
        Assert.Null(language.FallbackIsoCode);
    }

    [Test]
    public async Task Find_BaseData_Language()
    {
        // Act
        var languages = await LanguageService.GetAllAsync();

        // Assert
        Assert.That(3, Is.EqualTo(languages.Count()));
    }

    [Test]
    public async Task Save_Language_And_GetLanguageByIsoCode()
    {
        // Arrange
        var isoCode = "en-AU";
        var languageEnAu = new LanguageBuilder()
            .WithCultureInfo(isoCode)
            .Build();

        // Act
        await LanguageService.CreateAsync(languageEnAu, Constants.Security.SuperUserKey);
        var result = await LanguageService.GetAsync(isoCode);

        // Assert
        Assert.NotNull(result);
    }

    [Test]
    public async Task Set_Default_Language()
    {
        var languageEnAu = new LanguageBuilder()
            .WithCultureInfo("en-AU")
            .WithIsDefault(true)
            .Build();
        await LanguageService.CreateAsync(languageEnAu, Constants.Security.SuperUserKey);
        var result = await LanguageService.GetAsync(languageEnAu.IsoCode);

        Assert.IsTrue(result.IsDefault);

        var languageEnNz = new LanguageBuilder()
            .WithCultureInfo("en-NZ")
            .WithIsDefault(true)
            .Build();
        await LanguageService.CreateAsync(languageEnNz, Constants.Security.SuperUserKey);
        var result2 = await LanguageService.GetAsync(languageEnNz.IsoCode);

        // re-get
        result = await LanguageService.GetAsync(languageEnAu.IsoCode);

        Assert.IsTrue(result2.IsDefault);
        Assert.IsFalse(result.IsDefault);
    }

    [Test]
    public async Task Deleted_Language_Should_Not_Exist()
    {
        var isoCode = "en-AU";
        var languageEnAu = new LanguageBuilder()
            .WithCultureInfo(isoCode)
            .Build();
        await LanguageService.CreateAsync(languageEnAu, Constants.Security.SuperUserKey);

        // Act
        var result = await LanguageService.DeleteAsync(languageEnAu.IsoCode, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Assert
        Assert.IsNull(await LanguageService.GetAsync(isoCode));
    }

    [Test]
    public async Task Can_Update_Existing_Language()
    {
        ILanguage languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.NotNull(languageDaDk);
        Assert.IsFalse(languageDaDk.IsMandatory);
        languageDaDk.IsMandatory = true;
        languageDaDk.IsoCode = "da";
        languageDaDk.CultureName = "New Culture Name For da-DK";

        var result = await LanguageService.UpdateAsync(languageDaDk, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(LanguageOperationStatus.Success, result.Status);

        // re-get
        languageDaDk = await LanguageService.GetAsync(languageDaDk.IsoCode);
        Assert.NotNull(languageDaDk);
        Assert.IsTrue(languageDaDk.IsMandatory);
        Assert.AreEqual("da", languageDaDk.IsoCode);
        Assert.AreEqual("New Culture Name For da-DK", languageDaDk.CultureName);
    }

    [Test]
    public async Task Can_Change_Default_Language_By_Update()
    {
        var defaultLanguageIsoCode = await LanguageService.GetDefaultIsoCodeAsync();
        Assert.AreEqual("en-US", defaultLanguageIsoCode);

        ILanguage languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.NotNull(languageDaDk);
        Assert.IsFalse(languageDaDk.IsDefault);
        Assert.AreNotEqual(defaultLanguageIsoCode, languageDaDk.IsoCode);

        languageDaDk.IsDefault = true;
        var result = await LanguageService.UpdateAsync(languageDaDk, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // re-get
        var previousDefaultLanguage = await LanguageService.GetAsync(defaultLanguageIsoCode);
        Assert.NotNull(previousDefaultLanguage);
        Assert.IsFalse(previousDefaultLanguage.IsDefault);
        languageDaDk = await LanguageService.GetAsync(languageDaDk.IsoCode);
        Assert.NotNull(languageDaDk);
        Assert.IsTrue(languageDaDk.IsDefault);
    }

    [Test]
    public async Task Cannot_Create_Language_With_Invalid_IsoCode()
    {
        var invalidLanguage = new Language("no-such-iso-code", "Invalid ISO code");
        var result = await LanguageService.CreateAsync(invalidLanguage, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(LanguageOperationStatus.InvalidIsoCode, result.Status);
    }

    [Test]
    public async Task Cannot_Create_Duplicate_Languages()
    {
        var isoCode = "en-AU";
        var languageEnAu = new LanguageBuilder()
            .WithCultureInfo(isoCode)
            .Build();
        var result = await LanguageService.CreateAsync(languageEnAu, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        var duplicateLanguageEnAu = new LanguageBuilder()
            .WithCultureInfo(isoCode)
            .Build();
        result = await LanguageService.CreateAsync(duplicateLanguageEnAu, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(LanguageOperationStatus.DuplicateIsoCode, result.Status);
    }

    [Test]
    public async Task Cannot_Create_Language_With_Invalid_Fallback_Language()
    {
        var isoCode = "en-AU";
        var languageEnAu = new LanguageBuilder()
            .WithCultureInfo(isoCode)
            .WithFallbackLanguageIsoCode("no-such-ISO-code")
            .Build();
        var result = await LanguageService.CreateAsync(languageEnAu, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(LanguageOperationStatus.InvalidFallbackIsoCode, result.Status);
    }

    [Test]
    public async Task Cannot_Create_Language_With_NonExisting_Fallback_Language()
    {
        var isoCode = "en-AU";
        var languageEnAu = new LanguageBuilder()
            .WithCultureInfo(isoCode)
            .WithFallbackLanguageIsoCode("fr-FR")
            .Build();
        var result = await LanguageService.CreateAsync(languageEnAu, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(LanguageOperationStatus.InvalidFallback, result.Status);
    }

    [Test]
    public async Task Cannot_Update_Non_Existing_Language()
    {
        ILanguage language = new Language("da", "Danish");
        var result = await LanguageService.UpdateAsync(language, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(LanguageOperationStatus.NotFound, result.Status);
    }

    [Test]
    public async Task Cannot_Undefault_Default_Language_By_Update()
    {
        var defaultLanguageIsoCode = await LanguageService.GetDefaultIsoCodeAsync();
        Assert.IsNotNull(defaultLanguageIsoCode);
        var defaultLanguage = await LanguageService.GetAsync(defaultLanguageIsoCode);
        Assert.IsNotNull(defaultLanguage);
        Assert.IsTrue(defaultLanguage.IsDefault);

        defaultLanguage.IsDefault = false;
        var result = await LanguageService.UpdateAsync(defaultLanguage, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(LanguageOperationStatus.MissingDefault, result.Status);

        // re-get
        defaultLanguage = await LanguageService.GetAsync(defaultLanguageIsoCode);
        Assert.IsNotNull(defaultLanguage);
        Assert.IsTrue(defaultLanguage.IsDefault);
        defaultLanguageIsoCode = await LanguageService.GetDefaultIsoCodeAsync();
        Assert.AreEqual(defaultLanguage.IsoCode, defaultLanguageIsoCode);
    }

    [Test]
    public async Task Cannot_Update_Language_With_NonExisting_Fallback_Language()
    {
        ILanguage languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.NotNull(languageDaDk);
        Assert.IsNull(languageDaDk.FallbackIsoCode);

        languageDaDk.FallbackIsoCode = "fr-FR";
        var result = await LanguageService.UpdateAsync(languageDaDk, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(LanguageOperationStatus.InvalidFallback, result.Status);
    }

    [Test]
    public async Task Cannot_Update_Language_With_Invalid_Fallback_Language()
    {
        ILanguage languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.NotNull(languageDaDk);
        Assert.IsNull(languageDaDk.FallbackIsoCode);

        languageDaDk.FallbackIsoCode = "no-such-iso-code";
        var result = await LanguageService.UpdateAsync(languageDaDk, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(LanguageOperationStatus.InvalidFallbackIsoCode, result.Status);
    }

    [Test]
    public async Task Cannot_Create_Direct_Cyclic_Fallback_Language()
    {
        ILanguage languageDaDk = await LanguageService.GetAsync("da-DK");
        ILanguage languageEnGb = await LanguageService.GetAsync("en-GB");
        Assert.NotNull(languageDaDk);
        Assert.NotNull(languageEnGb);
        Assert.IsNull(languageDaDk.FallbackIsoCode);
        Assert.IsNull(languageEnGb.FallbackIsoCode);

        languageDaDk.FallbackIsoCode = languageEnGb.IsoCode;
        var result = await LanguageService.UpdateAsync(languageDaDk, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        languageEnGb.FallbackIsoCode = languageDaDk.IsoCode;
        result = await LanguageService.UpdateAsync(languageEnGb, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(LanguageOperationStatus.InvalidFallback, result.Status);
    }

    [Test]
    public async Task Cannot_Create_Implicit_Cyclic_Fallback_Language()
    {
        ILanguage languageEnUs = await LanguageService.GetAsync("en-US");
        ILanguage languageEnGb = await LanguageService.GetAsync("en-GB");
        ILanguage languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.IsNotNull(languageEnUs);
        Assert.IsNotNull(languageEnGb);
        Assert.IsNotNull(languageDaDk);
        Assert.IsNull(languageEnUs.FallbackIsoCode);
        Assert.IsNull(languageEnGb.FallbackIsoCode);
        Assert.IsNull(languageDaDk.FallbackIsoCode);

        languageEnGb.FallbackIsoCode = languageEnUs.IsoCode;
        var result = await LanguageService.UpdateAsync(languageEnGb, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        languageDaDk.FallbackIsoCode = languageEnGb.IsoCode;
        result = await LanguageService.UpdateAsync(languageDaDk, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        languageEnUs.FallbackIsoCode = languageDaDk.IsoCode;
        result = await LanguageService.UpdateAsync(languageEnUs, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(LanguageOperationStatus.InvalidFallback, result.Status);

        // re-get
        languageEnUs = await LanguageService.GetAsync("en-US");
        languageEnGb = await LanguageService.GetAsync("en-GB");
        languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.IsNotNull(languageEnUs);
        Assert.IsNotNull(languageEnGb);
        Assert.IsNotNull(languageDaDk);

        Assert.AreEqual(languageEnUs.IsoCode, languageEnGb.FallbackIsoCode);
        Assert.AreEqual(languageEnGb.IsoCode, languageDaDk.FallbackIsoCode);
        Assert.IsNull(languageEnUs.FallbackIsoCode);
    }

    [Test]
    public async Task Cannot_Delete_Default_Language()
    {
        var languageNbNo = new LanguageBuilder()
            .WithCultureInfo("nb-NO")
            .WithIsDefault(true)
            .Build();
        var result = await LanguageService.CreateAsync(languageNbNo, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        result = await LanguageService.DeleteAsync(languageNbNo.IsoCode, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(LanguageOperationStatus.MissingDefault, result.Status);

        // re-get
        languageNbNo = await LanguageService.GetAsync("nb-NO");
        Assert.NotNull(languageNbNo);
        Assert.IsTrue(languageNbNo.IsDefault);
    }

    [Test]
    public async Task Cannot_Delete_NonExisting_Language()
    {
        var languageNbNo = new LanguageBuilder()
            .WithCultureInfo("nb-NO")
            .Build();

        var result = await LanguageService.DeleteAsync(languageNbNo.IsoCode, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(LanguageOperationStatus.NotFound, result.Status);
    }

    [Test]
    public async Task Cannot_Create_Language_With_Reused_Language_Model()
    {
        var languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.NotNull(languageDaDk);
        languageDaDk.IsoCode = "nb-NO";

        var result = await LanguageService.CreateAsync(languageDaDk, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(LanguageOperationStatus.InvalidId, result.Status);
    }

    private async Task CreateTestData()
    {
        var languageDaDk = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        var languageEnGb = new LanguageBuilder()
            .WithCultureInfo("en-GB")
            .Build();

        var languageResult = await LanguageService.CreateAsync(languageDaDk, Constants.Security.SuperUserKey);
        Assert.IsTrue(languageResult.Success);
        languageResult = await LanguageService.CreateAsync(languageEnGb, Constants.Security.SuperUserKey);
        Assert.IsTrue(languageResult.Success);
    }
}
