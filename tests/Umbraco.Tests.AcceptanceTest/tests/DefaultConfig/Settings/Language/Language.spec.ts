import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const languageName = 'Arabic';
const isoCode = 'ar';
const defaultLanguageName = 'English (United States)';
const defaultLanguageIsoCode = 'en-US';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoApi.language.ensureNameNotExists(languageName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.language.ensureNameNotExists(languageName);
});

test.skip('can add language', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.language.goToSettingsTreeItem('Language');

  // Act
  await umbracoUi.language.clickCreateLink();
  await umbracoUi.language.chooseLanguageByName(languageName);
  await umbracoUi.language.clickSaveButton();

  // Assert
  expect(await umbracoApi.language.doesExist(isoCode)).toBeTruthy();
  // Verify the created language displays in the list
  await umbracoUi.language.clickLanguageRoot();
  await umbracoUi.language.isLanguageNameVisible(languageName, true);
});

test('can update default language option', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.create(languageName, false, false, isoCode);
  expect(await umbracoApi.language.doesExist(isoCode)).toBeTruthy();
  await umbracoUi.language.goToSettingsTreeItem('Language');

  // Act
  await umbracoUi.language.clickLanguageByName(languageName);
  await umbracoUi.language.switchDefaultLanguageOption();
  await umbracoUi.language.clickSaveButton();

  // Assert
  const languageData = await umbracoApi.language.get(isoCode);
  expect(languageData.isDefault).toBe(true);

  // Clean
  // To delete this language, you need to change en-US to be default language
  const defaultLanguageData = await umbracoApi.language.get(defaultLanguageIsoCode);
  defaultLanguageData.isDefault = true;
  await umbracoApi.language.update(defaultLanguageIsoCode, defaultLanguageData);
});

test('can update mandatory language option', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.create(languageName, false, false, isoCode);
  expect(await umbracoApi.language.doesExist(isoCode)).toBeTruthy();
  await umbracoUi.language.goToSettingsTreeItem('Language');

  // Act
  await umbracoUi.language.clickLanguageByName(languageName);
  await umbracoUi.language.switchMandatoryLanguageOption();
  await umbracoUi.language.clickSaveButton();

  // Assert
  const languageData = await umbracoApi.language.get(isoCode);
  expect(languageData.isMandatory).toBe(true);
});

test('can delete language', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.create(languageName, false, false, isoCode);
  expect(await umbracoApi.language.doesExist(isoCode)).toBeTruthy();
  await umbracoUi.language.goToSettingsTreeItem('Language');

  // Act
  await umbracoUi.language.removeLanguageByName(languageName);

  // Assert
  await umbracoUi.language.isSuccessNotificationVisible();
  expect(await umbracoApi.language.doesExist(isoCode)).toBeFalsy();
  // TODO: uncomment this when the front-end is ready. Currently the deleted language is not disappeared after deleting.
  //await umbracoUi.language.isLanguageNameVisible(languageName, false);
});

// TODO: Remove skip when the add fallback language function works
test.skip('can remove fallback language', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.create(languageName, false, false, isoCode);
  expect(await umbracoApi.language.doesExist(isoCode)).toBeTruthy();
  await umbracoUi.language.goToSettingsTreeItem('Language');

  // Act
  await umbracoUi.language.clickLanguageByName(languageName);
  await umbracoUi.language.removeFallbackLanguageByName(defaultLanguageName);
  await umbracoUi.language.clickSaveButton();

  // Act
  const languageData = await umbracoApi.language.get(isoCode);
  expect(languageData.fallbackIsoCode).toBeFalsy();
});

// TODO: Remove skip when the add fallback language function works
test.skip('can add fallback language', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.create(languageName, false, false, isoCode, null);
  expect(await umbracoApi.language.doesExist(isoCode)).toBeTruthy();
  await umbracoUi.language.goToSettingsTreeItem('Language');

  // Act
  await umbracoUi.language.clickLanguageByName(languageName);
  await umbracoUi.language.clickAddFallbackLanguageButton();
  await umbracoUi.language.selectFallbackLanguageByName(defaultLanguageName);
  await umbracoUi.language.clickSaveButton();

  // Act
  const languageData = await umbracoApi.language.get(isoCode);
  expect(languageData.fallbackIsoCode).toBe(defaultLanguageIsoCode);
});
