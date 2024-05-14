import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Language tests', () => {
  const languageNameDanish = 'Dansk';
  const isoCodeDanish = 'da-DK';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.language.ensureNameNotExists(languageNameDanish);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.language.ensureNameNotExists(languageNameDanish);
  });

  test('can create a language', async ({umbracoApi}) => {
    // Act
    await umbracoApi.language.create(languageNameDanish, false, false, isoCodeDanish);

    // Assert
    expect(await umbracoApi.language.doesExist(isoCodeDanish)).toBeTruthy();
  });

  test('can update a language', async ({umbracoApi}) => {
    // Arrange
    const wrongLanguageName = 'densk';
    await umbracoApi.language.create(wrongLanguageName, false, false, isoCodeDanish);
    const language = await umbracoApi.language.get(isoCodeDanish);

    // Act
    language.name = languageNameDanish;
    await umbracoApi.language.update(isoCodeDanish, language);

    // Assert
    expect(await umbracoApi.language.doesExist(isoCodeDanish)).toBeTruthy();
    // Checks if the language name was updated
    const updatedLanguage = await umbracoApi.language.get(isoCodeDanish);
    expect(updatedLanguage.name).toEqual(languageNameDanish);
  });

  test('can delete a language', async ({umbracoApi}) => {
    // Arrange
    await umbracoApi.language.create(languageNameDanish, false, false, isoCodeDanish);
    expect(await umbracoApi.language.doesExist(isoCodeDanish)).toBeTruthy();

    //Act
    await umbracoApi.language.delete(isoCodeDanish);

    // Assert
    expect(await umbracoApi.language.doesExist(isoCodeDanish)).toBeFalsy();
  });
});
