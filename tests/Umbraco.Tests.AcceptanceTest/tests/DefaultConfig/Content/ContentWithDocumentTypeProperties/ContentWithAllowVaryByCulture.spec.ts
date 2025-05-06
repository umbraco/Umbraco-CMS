import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const secondLanguageName = 'Danish';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.language.ensureNameNotExists(secondLanguageName);
  await umbracoApi.language.createDanishLanguage();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.ensureNameNotExists(secondLanguageName);
});

test('can create content with allow vary by culture enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDocumentTypeWithAllowVaryByCulture(documentTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveAndCloseButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
});

test('can create content with names that vary by culture', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const danishContentName = 'Test indhold';
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowVaryByCulture(documentTypeName);
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSelectVariantButton();
  await umbracoUi.content.clickVariantAddModeButtonForLanguageName(secondLanguageName);
  await umbracoUi.content.enterContentName(danishContentName);
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveAndCloseButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(danishContentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(danishContentName);
  expect(contentData.variants.length).toBe(2);
  expect(contentData.variants[0].name).toBe(contentName);
  expect(contentData.variants[1].name).toBe(danishContentName);
});

test('can create content with names that vary by culture and content that is invariant', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const danishContentName = 'Test indhold';
  const textContent = 'This is a test text';
  const danishTextContent = 'Dette er testtekst';
  const dataTypeName = 'Textstring';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'Test Group', true, false);
  await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, textContent, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSelectVariantButton();
  await umbracoUi.content.clickVariantAddModeButtonForLanguageName(secondLanguageName);
  await umbracoUi.content.enterContentName(danishContentName);
  await umbracoUi.content.enterTextstring(danishTextContent);
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveAndCloseButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(danishContentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(danishContentName);
  expect(contentData.variants.length).toBe(2);
  expect(contentData.variants[0].name).toBe(contentName);
  expect(contentData.variants[1].name).toBe(danishContentName);
  expect(contentData.values.length).toBe(1);
  expect(contentData.values[0].value).toBe(danishTextContent);
});

test('can create content with names and content that vary by culture', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const danishContentName = 'Test indhold';
  const textContent = 'This is a test text';
  const danishTextContent = 'Dette er testtekst';
  const dataTypeName = 'Textstring';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'Test Group', true, true);
  await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, textContent, dataTypeName, true);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSelectVariantButton();
  await umbracoUi.content.clickVariantAddModeButtonForLanguageName(secondLanguageName);
  await umbracoUi.content.enterContentName(danishContentName);
  await umbracoUi.content.enterTextstring(danishTextContent);
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveAndCloseButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(danishContentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(danishContentName);
  expect(contentData.variants.length).toBe(2);
  expect(contentData.variants[0].name).toBe(contentName);
  expect(contentData.variants[1].name).toBe(danishContentName);
  expect(contentData.values.length).toBe(2);
  expect(contentData.values[0].value).toBe(textContent);
  expect(contentData.values[1].value).toBe(danishTextContent);
});
