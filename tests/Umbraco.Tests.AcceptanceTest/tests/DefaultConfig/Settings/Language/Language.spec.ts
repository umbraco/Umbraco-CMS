import {NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
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

test('can add language', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.language.goToLanguages();

  // Act
  await umbracoUi.language.clickCreateLink();
  await umbracoUi.language.chooseLanguageByName(languageName);
  await umbracoUi.language.clickSaveButton();

  // Assert
  //await umbracoUi.language.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.language.isErrorNotificationVisible(false);
  expect(await umbracoApi.language.doesExist(isoCode)).toBeTruthy();
  // Verify the created language displays in the list
  await umbracoUi.language.clickLanguagesMenu();
  await umbracoUi.language.isLanguageNameVisible(languageName, true);
});

test('can update default language option', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.create(languageName, false, false, isoCode);
  expect(await umbracoApi.language.doesExist(isoCode)).toBeTruthy();
  await umbracoUi.language.goToLanguages();

  // Act
  await umbracoUi.language.clickLanguageByName(languageName);
  await umbracoUi.language.switchDefaultLanguageOption();
  await umbracoUi.language.clickSaveButton();

  // Assert
  //await umbracoUi.language.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.language.isErrorNotificationVisible(false);
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
  await umbracoUi.language.goToLanguages();

  // Act
  await umbracoUi.language.clickLanguageByName(languageName);
  await umbracoUi.language.switchMandatoryLanguageOption();
  await umbracoUi.language.clickSaveButton();

  // Assert
  //await umbracoUi.language.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.language.isErrorNotificationVisible(false);
  const languageData = await umbracoApi.language.get(isoCode);
  expect(languageData.isMandatory).toBe(true);
});

test('can delete language', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.create(languageName, false, false, isoCode);
  expect(await umbracoApi.language.doesExist(isoCode)).toBeTruthy();
  await umbracoUi.language.goToLanguages();

  // Act
  await umbracoUi.language.removeLanguageByName(languageName);

  // Assert
  //await umbracoUi.language.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted);
  await umbracoUi.language.isErrorNotificationVisible(false);
  expect(await umbracoApi.language.doesExist(isoCode)).toBeFalsy();
  await umbracoUi.language.isLanguageNameVisible(languageName, false);
});

test('can remove fallback language', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.create(languageName, false, false, isoCode);
  expect(await umbracoApi.language.doesExist(isoCode)).toBeTruthy();
  await umbracoUi.language.goToLanguages();

  // Act
  await umbracoUi.language.clickLanguageByName(languageName);
  await umbracoUi.language.removeFallbackLanguageByIsoCode(defaultLanguageIsoCode);
  await umbracoUi.language.clickSaveButton();

  // Act
  //await umbracoUi.language.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.language.isErrorNotificationVisible(false);
  const languageData = await umbracoApi.language.get(isoCode);
  expect(languageData.fallbackIsoCode).toBeFalsy();
});

test('can add fallback language', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.create(languageName, false, false, isoCode, null);
  expect(await umbracoApi.language.doesExist(isoCode)).toBeTruthy();
  await umbracoUi.language.goToLanguages();

  // Act
  await umbracoUi.language.clickLanguageByName(languageName);
  await umbracoUi.language.clickChooseButton();
  await umbracoUi.language.selectFallbackLanguageByName(defaultLanguageName);
  await umbracoUi.language.clickSaveButton();

  // Act
  //await umbracoUi.language.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.language.isErrorNotificationVisible(false);
  const languageData = await umbracoApi.language.get(isoCode);
  expect(languageData.fallbackIsoCode).toBe(defaultLanguageIsoCode);
});

test('cannot add a language with duplicate ISO code', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.create(languageName, false, false, isoCode);
  expect(await umbracoApi.language.doesExist(isoCode)).toBeTruthy();
  await umbracoUi.language.goToLanguages();

  // Act
  await umbracoUi.language.clickCreateLink();
  await umbracoUi.language.chooseLanguageByName(languageName);
  await umbracoUi.language.clickSaveButton();

  // Assert
  await umbracoUi.language.doesErrorNotificationHaveText(NotificationConstantHelper.error.duplicateISOcode);
});
