using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
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
        Assert.That(languages, Is.Not.Null);
        Assert.That(languages.Any(), Is.True);
        Assert.That(languages.Count(), Is.EqualTo(3));
    }

    [Test]
    public async Task Can_Get_All_Language_Iso_Codes()
    {
        var isoCodes = await LanguageService.GetAllIsoCodesAsync();
        Assert.That(isoCodes.Count(), Is.EqualTo(3));
        Assert.That(string.Join(",", isoCodes.OrderBy(x => x)), Is.EqualTo("da-DK,en-GB,en-US"));
    }

    [Test]
    public async Task Can_GetLanguageByIsoCode()
    {
        var danish = await LanguageService.GetAsync("da-DK");
        var english = await LanguageService.GetAsync("en-GB");
        Assert.That(danish, Is.Not.Null);
        Assert.That(english, Is.Not.Null);
    }

    [Test]
    public async Task Does_Not_Fail_When_Language_Doesnt_Exist()
    {
        var language = await LanguageService.GetAsync("sv-SE");
        Assert.That(language, Is.Null);
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
        Assert.That(language, Is.Not.Null);

        var result = await LanguageService.DeleteAsync("nb-NO", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        language = await LanguageService.GetAsync("nb-NO");
        Assert.That(language, Is.Null);
    }

    [Test]
    public async Task Can_Create_Language_With_Fallback()
    {
        var languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.That(languageDaDk, Is.Not.Null);
        var languageNbNo = new LanguageBuilder()
            .WithCultureInfo("nb-NO")
            .WithFallbackLanguageIsoCode(languageDaDk.IsoCode)
            .Build();
        var result = await LanguageService.CreateAsync(languageNbNo, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        var language = await LanguageService.GetAsync("nb-NO");
        Assert.That(language, Is.Not.Null);
        Assert.That(language.FallbackIsoCode, Is.EqualTo("da-DK"));
    }

    [Test]
    public async Task Can_Delete_Language_Used_As_Fallback()
    {
        var languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.That(languageDaDk, Is.Not.Null);
        var languageNbNo = new LanguageBuilder()
            .WithCultureInfo("nb-NO")
            .WithFallbackLanguageIsoCode(languageDaDk.IsoCode)
            .Build();
        var result = await LanguageService.CreateAsync(languageNbNo, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        var language = await LanguageService.GetAsync("nb-NO");
        Assert.That(language, Is.Not.Null);
        Assert.That(language.FallbackIsoCode, Is.EqualTo("da-DK"));

        result = await LanguageService.DeleteAsync("da-DK", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        language = await LanguageService.GetAsync("da-DK");
        Assert.That(language, Is.Null);

        language = await LanguageService.GetAsync("nb-NO");
        Assert.That(language, Is.Not.Null);
        Assert.That(language.FallbackIsoCode, Is.Null);
    }

    [Test]
    public async Task Find_BaseData_Language()
    {
        // Act
        var languages = await LanguageService.GetAllAsync();

        // Assert
        Assert.That(languages.Count(), Is.EqualTo(3));
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
        Assert.That(result, Is.Not.Null);
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

        Assert.That(result.IsDefault, Is.True);

        var languageEnNz = new LanguageBuilder()
            .WithCultureInfo("en-NZ")
            .WithIsDefault(true)
            .Build();
        await LanguageService.CreateAsync(languageEnNz, Constants.Security.SuperUserKey);
        var result2 = await LanguageService.GetAsync(languageEnNz.IsoCode);

        // re-get
        result = await LanguageService.GetAsync(languageEnAu.IsoCode);

        Assert.That(result2.IsDefault, Is.True);
        Assert.That(result.IsDefault, Is.False);
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
        Assert.That(result.Success, Is.True);

        // Assert
        Assert.That(await LanguageService.GetAsync(isoCode), Is.Null);
    }

    [Test]
    public async Task Can_Update_Existing_Language()
    {
        ILanguage languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.That(languageDaDk, Is.Not.Null);
        Assert.That(languageDaDk.IsMandatory, Is.False);
        languageDaDk.IsMandatory = true;
        languageDaDk.IsoCode = "da";
        languageDaDk.CultureName = "New Culture Name For da-DK";

        var result = await LanguageService.UpdateAsync(languageDaDk, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(LanguageOperationStatus.Success));

        // Verify that the create and update dates can be used to distinguish between creates
        // and updates (as these fields are used in ServerEventSender to emit a "Created" or "Updated"
        // event.
        Assert.That(result.Result.UpdateDate, Is.GreaterThan(result.Result.CreateDate));

        // re-get
        languageDaDk = await LanguageService.GetAsync(languageDaDk.IsoCode);
        Assert.That(languageDaDk, Is.Not.Null);
        Assert.That(languageDaDk.IsMandatory, Is.True);
        Assert.That(languageDaDk.IsoCode, Is.EqualTo("da"));
        Assert.That(languageDaDk.CultureName, Is.EqualTo("New Culture Name For da-DK"));
    }

    [Test]
    public async Task Can_Change_Default_Language_By_Update()
    {
        var defaultLanguageIsoCode = await LanguageService.GetDefaultIsoCodeAsync();
        Assert.That(defaultLanguageIsoCode, Is.EqualTo("en-US"));

        ILanguage languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.That(languageDaDk, Is.Not.Null);
        Assert.That(languageDaDk.IsDefault, Is.False);
        Assert.That(languageDaDk.IsoCode, Is.Not.EqualTo(defaultLanguageIsoCode));

        languageDaDk.IsDefault = true;
        var result = await LanguageService.UpdateAsync(languageDaDk, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // re-get
        var previousDefaultLanguage = await LanguageService.GetAsync(defaultLanguageIsoCode);
        Assert.That(previousDefaultLanguage, Is.Not.Null);
        Assert.That(previousDefaultLanguage.IsDefault, Is.False);
        languageDaDk = await LanguageService.GetAsync(languageDaDk.IsoCode);
        Assert.That(languageDaDk, Is.Not.Null);
        Assert.That(languageDaDk.IsDefault, Is.True);
    }

    [Test]
    public async Task Cannot_Create_Language_With_Invalid_IsoCode()
    {
        var invalidLanguage = new Language("no-such-iso-code", "Invalid ISO code");
        var result = await LanguageService.CreateAsync(invalidLanguage, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LanguageOperationStatus.InvalidIsoCode));
    }

    [Test]
    public async Task Cannot_Create_Duplicate_Languages()
    {
        var isoCode = "en-AU";
        var languageEnAu = new LanguageBuilder()
            .WithCultureInfo(isoCode)
            .Build();
        var result = await LanguageService.CreateAsync(languageEnAu, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        var duplicateLanguageEnAu = new LanguageBuilder()
            .WithCultureInfo(isoCode)
            .Build();
        result = await LanguageService.CreateAsync(duplicateLanguageEnAu, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LanguageOperationStatus.DuplicateIsoCode));
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
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LanguageOperationStatus.InvalidFallbackIsoCode));
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
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LanguageOperationStatus.InvalidFallback));
    }

    [Test]
    public async Task Cannot_Update_Non_Existing_Language()
    {
        ILanguage language = new Language("da", "Danish");
        var result = await LanguageService.UpdateAsync(language, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LanguageOperationStatus.NotFound));
    }

    [Test]
    public async Task Cannot_Undefault_Default_Language_By_Update()
    {
        var defaultLanguageIsoCode = await LanguageService.GetDefaultIsoCodeAsync();
        Assert.That(defaultLanguageIsoCode, Is.Not.Null);
        var defaultLanguage = await LanguageService.GetAsync(defaultLanguageIsoCode);
        Assert.That(defaultLanguage, Is.Not.Null);
        Assert.That(defaultLanguage.IsDefault, Is.True);

        defaultLanguage.IsDefault = false;
        var result = await LanguageService.UpdateAsync(defaultLanguage, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LanguageOperationStatus.MissingDefault));

        // re-get
        defaultLanguage = await LanguageService.GetAsync(defaultLanguageIsoCode);
        Assert.That(defaultLanguage, Is.Not.Null);
        Assert.That(defaultLanguage.IsDefault, Is.True);
        defaultLanguageIsoCode = await LanguageService.GetDefaultIsoCodeAsync();
        Assert.That(defaultLanguageIsoCode, Is.EqualTo(defaultLanguage.IsoCode));
    }

    [Test]
    public async Task Cannot_Update_Language_With_NonExisting_Fallback_Language()
    {
        ILanguage languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.That(languageDaDk, Is.Not.Null);
        Assert.That(languageDaDk.FallbackIsoCode, Is.Null);

        languageDaDk.FallbackIsoCode = "fr-FR";
        var result = await LanguageService.UpdateAsync(languageDaDk, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LanguageOperationStatus.InvalidFallback));
    }

    [Test]
    public async Task Cannot_Update_Language_With_Invalid_Fallback_Language()
    {
        ILanguage languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.That(languageDaDk, Is.Not.Null);
        Assert.That(languageDaDk.FallbackIsoCode, Is.Null);

        languageDaDk.FallbackIsoCode = "no-such-iso-code";
        var result = await LanguageService.UpdateAsync(languageDaDk, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LanguageOperationStatus.InvalidFallbackIsoCode));
    }

    [Test]
    public async Task Cannot_Create_Direct_Cyclic_Fallback_Language()
    {
        ILanguage languageDaDk = await LanguageService.GetAsync("da-DK");
        ILanguage languageEnGb = await LanguageService.GetAsync("en-GB");
        Assert.That(languageDaDk, Is.Not.Null);
        Assert.That(languageEnGb, Is.Not.Null);
        Assert.That(languageDaDk.FallbackIsoCode, Is.Null);
        Assert.That(languageEnGb.FallbackIsoCode, Is.Null);

        languageDaDk.FallbackIsoCode = languageEnGb.IsoCode;
        var result = await LanguageService.UpdateAsync(languageDaDk, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        languageEnGb.FallbackIsoCode = languageDaDk.IsoCode;
        result = await LanguageService.UpdateAsync(languageEnGb, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LanguageOperationStatus.InvalidFallback));
    }

    [Test]
    public async Task Cannot_Create_Implicit_Cyclic_Fallback_Language()
    {
        ILanguage languageEnUs = await LanguageService.GetAsync("en-US");
        ILanguage languageEnGb = await LanguageService.GetAsync("en-GB");
        ILanguage languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.That(languageEnUs, Is.Not.Null);
        Assert.That(languageEnGb, Is.Not.Null);
        Assert.That(languageDaDk, Is.Not.Null);
        Assert.That(languageEnUs.FallbackIsoCode, Is.Null);
        Assert.That(languageEnGb.FallbackIsoCode, Is.Null);
        Assert.That(languageDaDk.FallbackIsoCode, Is.Null);

        languageEnGb.FallbackIsoCode = languageEnUs.IsoCode;
        var result = await LanguageService.UpdateAsync(languageEnGb, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        languageDaDk.FallbackIsoCode = languageEnGb.IsoCode;
        result = await LanguageService.UpdateAsync(languageDaDk, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        languageEnUs.FallbackIsoCode = languageDaDk.IsoCode;
        result = await LanguageService.UpdateAsync(languageEnUs, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LanguageOperationStatus.InvalidFallback));

        // re-get
        languageEnUs = await LanguageService.GetAsync("en-US");
        languageEnGb = await LanguageService.GetAsync("en-GB");
        languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.That(languageEnUs, Is.Not.Null);
        Assert.That(languageEnGb, Is.Not.Null);
        Assert.That(languageDaDk, Is.Not.Null);

        Assert.That(languageEnGb.FallbackIsoCode, Is.EqualTo(languageEnUs.IsoCode));
        Assert.That(languageDaDk.FallbackIsoCode, Is.EqualTo(languageEnGb.IsoCode));
        Assert.That(languageEnUs.FallbackIsoCode, Is.Null);
    }

    [Test]
    public async Task Cannot_Delete_Default_Language()
    {
        var languageNbNo = new LanguageBuilder()
            .WithCultureInfo("nb-NO")
            .WithIsDefault(true)
            .Build();
        var result = await LanguageService.CreateAsync(languageNbNo, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        result = await LanguageService.DeleteAsync(languageNbNo.IsoCode, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LanguageOperationStatus.MissingDefault));

        // re-get
        languageNbNo = await LanguageService.GetAsync("nb-NO");
        Assert.That(languageNbNo, Is.Not.Null);
        Assert.That(languageNbNo.IsDefault, Is.True);
    }

    [Test]
    public async Task Cannot_Delete_NonExisting_Language()
    {
        var languageNbNo = new LanguageBuilder()
            .WithCultureInfo("nb-NO")
            .Build();

        var result = await LanguageService.DeleteAsync(languageNbNo.IsoCode, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LanguageOperationStatus.NotFound));
    }

    [Test]
    public async Task Cannot_Create_Language_With_Reused_Language_Model()
    {
        var languageDaDk = await LanguageService.GetAsync("da-DK");
        Assert.That(languageDaDk, Is.Not.Null);
        languageDaDk.IsoCode = "nb-NO";

        var result = await LanguageService.CreateAsync(languageDaDk, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LanguageOperationStatus.InvalidId));
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
        Assert.That(languageResult.Success, Is.True);
        languageResult = await LanguageService.CreateAsync(languageEnGb, Constants.Security.SuperUserKey);
        Assert.That(languageResult.Success, Is.True);
    }
}
