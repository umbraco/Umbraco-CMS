// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests covering all methods in the LocalizationService class.
///     This is more of an integration test as it involves multiple layers
///     as well as configuration.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class LocalizationServiceTests : UmbracoIntegrationTest
{
    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    [SetUp]
    public async Task SetUp() => await CreateTestData();

    private Guid _parentItemGuidId;
    private int _parentItemIntId;
    private Guid _childItemGuidId;
    private int _childItemIntId;
    private int _danishLangId;
    private int _englishLangId;

    private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();

    [Test]
    public void Can_Get_All_Languages()
    {
        var languages = LocalizationService.GetAllLanguages();
        Assert.NotNull(languages);
        Assert.IsTrue(languages.Any());
        Assert.That(languages.Count(), Is.EqualTo(3));
    }

    [Test]
    public void Can_Get_Dictionary_Item_By_Int_Id()
    {
        var parentItem = LocalizationService.GetDictionaryItemById(_parentItemIntId);
        Assert.NotNull(parentItem);

        var childItem = LocalizationService.GetDictionaryItemById(_childItemIntId);
        Assert.NotNull(childItem);
    }

    [Test]
    public void Can_Get_Dictionary_Item_By_Guid_Id()
    {
        var parentItem = LocalizationService.GetDictionaryItemById(_parentItemGuidId);
        Assert.NotNull(parentItem);

        var childItem = LocalizationService.GetDictionaryItemById(_childItemGuidId);
        Assert.NotNull(childItem);
    }

    [Test]
    public void Can_Get_Dictionary_Item_By_Key()
    {
        var parentItem = LocalizationService.GetDictionaryItemByKey("Parent");
        Assert.NotNull(parentItem);

        var childItem = LocalizationService.GetDictionaryItemByKey("Child");
        Assert.NotNull(childItem);
    }

    [Test]
    public void Can_Get_Dictionary_Item_Children()
    {
        var item = LocalizationService.GetDictionaryItemChildren(_parentItemGuidId);
        Assert.NotNull(item);
        Assert.That(item.Count(), Is.EqualTo(1));

        foreach (var dictionaryItem in item)
        {
            Assert.AreEqual(_parentItemGuidId, dictionaryItem.ParentId);
            Assert.IsFalse(string.IsNullOrEmpty(dictionaryItem.ItemKey));
        }
    }

    [Test]
    public void Can_GetLanguageById()
    {
        var danish = LocalizationService.GetLanguageById(_danishLangId);
        var english = LocalizationService.GetLanguageById(_englishLangId);
        Assert.NotNull(danish);
        Assert.NotNull(english);
    }

    [Test]
    public void Can_GetLanguageByIsoCode()
    {
        var danish = LocalizationService.GetLanguageByIsoCode("da-DK");
        var english = LocalizationService.GetLanguageByIsoCode("en-GB");
        Assert.NotNull(danish);
        Assert.NotNull(english);
    }

    [Test]
    public void Does_Not_Fail_When_Language_Doesnt_Exist()
    {
        var language = LocalizationService.GetLanguageByIsoCode("sv-SE");
        Assert.Null(language);
    }

    [Test]
    public void Does_Not_Fail_When_DictionaryItem_Doesnt_Exist()
    {
        var item = LocalizationService.GetDictionaryItemByKey("RandomKey");
        Assert.Null(item);
    }

    [Test]
    public async Task Can_Delete_Language()
    {
        var languageNbNo = new LanguageBuilder()
            .WithCultureInfo("nb-NO")
            .Build();
        await LanguageService.CreateAsync(languageNbNo, Constants.Security.SuperUserKey);
        Assert.That(languageNbNo.HasIdentity, Is.True);

        await LanguageService.DeleteAsync(languageNbNo.IsoCode, Constants.Security.SuperUserKey);

        var language = await LanguageService.GetAsync(languageNbNo.IsoCode);
        Assert.Null(language);
    }

    [Test]
    public async Task Can_Delete_Language_Used_As_Fallback()
    {
        var languageDaDk = await LanguageService.GetAsync("da-DK");
        var languageNbNo = new LanguageBuilder()
            .WithCultureInfo("nb-NO")
            .WithFallbackLanguageIsoCode(languageDaDk.IsoCode)
            .Build();
        await LanguageService.CreateAsync(languageNbNo, Constants.Security.SuperUserKey);

        await LanguageService.DeleteAsync(languageDaDk.IsoCode, Constants.Security.SuperUserKey);

        var language = await LanguageService.GetAsync(languageDaDk.IsoCode);
        Assert.Null(language);
    }

    [Test]
    public void Can_Create_DictionaryItem_At_Root()
    {
        var english = LocalizationService.GetLanguageByIsoCode("en-US");

        var item = (IDictionaryItem)new DictionaryItem("Testing123")
        {
            Translations = new List<IDictionaryTranslation> { new DictionaryTranslation(english, "Hello world") }
        };
        LocalizationService.Save(item);

        // re-get
        item = LocalizationService.GetDictionaryItemById(item.Id);

        Assert.Greater(item.Id, 0);
        Assert.IsTrue(item.HasIdentity);
        Assert.IsFalse(item.ParentId.HasValue);
        Assert.AreEqual("Testing123", item.ItemKey);
        Assert.AreEqual(1, item.Translations.Count());
    }

    [Test]
    public void Can_Add_Translation_To_Existing_Dictionary_Item()
    {
        var english = LocalizationService.GetLanguageByIsoCode("en-US");

        var item = (IDictionaryItem)new DictionaryItem("Testing123");
        LocalizationService.Save(item);

        // re-get
        item = LocalizationService.GetDictionaryItemById(item.Id);

        item.Translations = new List<IDictionaryTranslation> { new DictionaryTranslation(english, "Hello world") };

        LocalizationService.Save(item);

        Assert.AreEqual(1, item.Translations.Count());
        foreach (var translation in item.Translations)
        {
            Assert.AreEqual("Hello world", translation.Value);
        }

        item.Translations = new List<IDictionaryTranslation>(item.Translations)
        {
            new DictionaryTranslation(
                LocalizationService.GetLanguageByIsoCode("en-GB"),
                "My new value")
        };

        LocalizationService.Save(item);

        // re-get
        item = LocalizationService.GetDictionaryItemById(item.Id);

        Assert.AreEqual(2, item.Translations.Count());
        Assert.AreEqual("Hello world", item.Translations.First().Value);
        Assert.AreEqual("My new value", item.Translations.Last().Value);
    }

    [Test]
    public void Can_Update_Existing_DictionaryItem()
    {
        var item = LocalizationService.GetDictionaryItemByKey("Child");
        foreach (var translation in item.Translations)
        {
            translation.Value += "UPDATED";
        }

        LocalizationService.Save(item);

        var updatedItem = LocalizationService.GetDictionaryItemByKey("Child");
        Assert.NotNull(updatedItem);

        foreach (var translation in updatedItem.Translations)
        {
            Assert.That(translation.Value.EndsWith("UPDATED"), Is.True);
        }
    }

    [Test]
    public void Find_BaseData_Language()
    {
        // Act
        var languages = LocalizationService.GetAllLanguages();

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
    public async Task Save_Language_And_GetLanguageById()
    {
        // Arrange
        var languageEnAu = new LanguageBuilder()
            .WithCultureInfo("en-AU")
            .Build();

        // Act
        await LanguageService.CreateAsync(languageEnAu, Constants.Security.SuperUserKey);
        var result = await LanguageService.GetAsync(languageEnAu.IsoCode);

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
        await LanguageService.DeleteAsync(languageEnAu.IsoCode, Constants.Security.SuperUserKey);
        var result = await LanguageService.GetAsync(isoCode);

        // Assert
        Assert.Null(result);
    }

    public async Task CreateTestData()
    {
        var languageDaDk = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        var languageEnGb = new LanguageBuilder()
            .WithCultureInfo("en-GB")
            .Build();

        await LanguageService.CreateAsync(languageDaDk, Constants.Security.SuperUserKey);
        await LanguageService.CreateAsync(languageEnGb, Constants.Security.SuperUserKey);
        _danishLangId = languageDaDk.Id;
        _englishLangId = languageEnGb.Id;

        var parentItem = new DictionaryItem("Parent")
        {
            Translations = new List<IDictionaryTranslation>
            {
                new DictionaryTranslation(languageEnGb, "ParentValue"),
                new DictionaryTranslation(languageDaDk, "ForældreVærdi")
            }
        };
        LocalizationService.Save(parentItem);
        _parentItemGuidId = parentItem.Key;
        _parentItemIntId = parentItem.Id;

        var childItem = new DictionaryItem(parentItem.Key, "Child")
        {
            Translations = new List<IDictionaryTranslation>
            {
                new DictionaryTranslation(languageEnGb, "ChildValue"),
                new DictionaryTranslation(languageDaDk, "BørnVærdi")
            }
        };
        LocalizationService.Save(childItem);
        _childItemGuidId = childItem.Key;
        _childItemIntId = childItem.Id;
    }
}
