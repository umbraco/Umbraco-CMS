import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Language tests', () => {
  const languageNameDanish = 'Dansk';
  const isoCodeDanish = 'da-DK';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.language.delete(isoCodeDanish);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.language.delete(isoCodeDanish);
  });

  test('can create a language', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.language.create(languageNameDanish, false, false, isoCodeDanish);

    // Assert
    await expect(await umbracoApi.language.exists(isoCodeDanish)).toBeTruthy();
  });

  test('can update a language', async ({page, umbracoApi, umbracoUi}) => {
    const wrongLanguageName = 'densk';

    await umbracoApi.language.create(wrongLanguageName, false, false, isoCodeDanish);

    const language = await umbracoApi.language.get(isoCodeDanish);

    // Updates language
    language.name = languageNameDanish;
    await umbracoApi.language.update(isoCodeDanish, language);

    // Assert
    await expect(await umbracoApi.language.exists(isoCodeDanish)).toBeTruthy();
    // Checks if the language name was updated
    const updatedLanguage = await umbracoApi.language.get(isoCodeDanish);
    await expect(updatedLanguage.name).toEqual(languageNameDanish);
  });

  test('can delete a language', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.language.create(languageNameDanish, false, false, isoCodeDanish);

    await expect(await umbracoApi.language.exists(isoCodeDanish)).toBeTruthy();

    await umbracoApi.language.delete(isoCodeDanish);

    // Assert
    await expect(await umbracoApi.language.exists(isoCodeDanish)).toBeFalsy();
  });
});
