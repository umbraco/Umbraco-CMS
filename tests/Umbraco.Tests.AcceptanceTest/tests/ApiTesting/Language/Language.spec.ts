import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Language tests', () => {
  const languageNameDanish = 'Dansk';
  const isoCodeDanish = 'da-DK';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.language.ensureIsoCodeNotExists(isoCodeDanish);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.language.ensureIsoCodeNotExists(isoCodeDanish);
  });

  test('can create new language', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.language.createLanguage(languageNameDanish, false, false, isoCodeDanish);

    // Assert
    await expect(await umbracoApi.language.doesLanguageWithIsoCodeExist(isoCodeDanish)).toBeTruthy();
  });

  test('can update language', async ({page, umbracoApi, umbracoUi}) => {
    const wrongLanguageName = 'densk';

    await umbracoApi.language.createLanguage(wrongLanguageName, false, false, isoCodeDanish);

    const language = await umbracoApi.language.getLanguageByName(wrongLanguageName);

    // Updates language
    language.name = languageNameDanish;
    await umbracoApi.language.updateLanguageWithIsoCode(isoCodeDanish, language);

    // Assert
    await expect(await umbracoApi.language.doesLanguageWithIsoCodeExist(isoCodeDanish)).toBeTruthy();
    // Checks if the language name was updated
    const updatedLanguage = await umbracoApi.language.getLanguageByIsoCode(isoCodeDanish);
    await expect(updatedLanguage.name == languageNameDanish).toBeTruthy();
  });

  test('can delete language', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.language.createLanguage(languageNameDanish, false, false, isoCodeDanish);

    await expect(await umbracoApi.language.doesLanguageWithIsoCodeExist(isoCodeDanish)).toBeTruthy();

    await umbracoApi.language.deleteLanguageWithIsoCode(isoCodeDanish);

    // Assert
    await expect(await umbracoApi.language.doesLanguageWithIsoCodeExist(isoCodeDanish)).toBeFalsy();
  });
});
