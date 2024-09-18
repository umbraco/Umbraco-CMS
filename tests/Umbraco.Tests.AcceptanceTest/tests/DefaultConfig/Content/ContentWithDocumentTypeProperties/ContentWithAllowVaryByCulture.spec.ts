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

test('can create a content with allow vary by culture is enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDocumentTypeWithAllowVaryByCulture(documentTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButton();
  await umbracoUi.content.clickSaveAndCloseButton();
  
  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
});

test('can create a language version with a different name of an empty content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const danishContentName = 'Test indhold';
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowVaryByCulture(documentTypeName);
  await umbracoApi.document.createDefaultEnglishDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickVariantSelectorButton();
  await umbracoUi.content.clickVariantAddModeButton();
  await umbracoUi.content.enterContentName(danishContentName);
  await umbracoUi.content.clickSaveButton();
  await umbracoUi.content.clickSaveAndCloseButton();
  
  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(danishContentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(danishContentName);
  expect(contentData.variants.length).toBe(2);
  expect(contentData.variants[0].name).toBe(contentName);
  expect(contentData.variants[1].name).toBe(danishContentName);
});

test('can create a language version with a different name and same content of an non-empty content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const danishContentName = 'Test indhold';
  const textContent = 'This is a test text';
  const danishTextContent = 'Dette er testtekst';
  const dataTypeName = 'Textstring';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'Test Group', false);
  await umbracoApi.document.createEnglishDocumentWithTextContent(contentName, documentTypeId, textContent, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickVariantSelectorButton();
  await umbracoUi.content.clickVariantAddModeButton();
  await umbracoUi.content.enterContentName(danishContentName);
  await umbracoUi.content.enterTextstring(danishTextContent);
  await umbracoUi.content.clickSaveButton();
  await umbracoUi.content.clickSaveAndCloseButton();
  
  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(danishContentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(danishContentName);
  expect(contentData.variants.length).toBe(2);
  expect(contentData.variants[0].name).toBe(contentName);
  expect(contentData.variants[1].name).toBe(danishContentName);
  expect(contentData.values.length).toBe(1);
  expect(contentData.values[0].value).toBe(danishTextContent);
});

test('can create a language version with a different name and different content of an non-empty content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const danishContentName = 'Test indhold';
  const textContent = 'This is a test text';
  const danishTextContent = 'Dette er testtekst';
  const dataTypeName = 'Textstring';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'Test Group', true);
  await umbracoApi.document.createEnglishDocumentWithTextContent(contentName, documentTypeId, textContent, dataTypeName, true);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickVariantSelectorButton();
  await umbracoUi.content.clickVariantAddModeButton();
  await umbracoUi.content.enterContentName(danishContentName);
  await umbracoUi.content.enterTextstring(danishTextContent);
  await umbracoUi.content.clickSaveButton();
  await umbracoUi.content.clickSaveAndCloseButton();
  
  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(danishContentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(danishContentName);
  expect(contentData.variants.length).toBe(2);
  expect(contentData.variants[0].name).toBe(contentName);
  expect(contentData.variants[1].name).toBe(danishContentName);
  expect(contentData.values.length).toBe(2);
  expect(contentData.values[0].value).toBe(textContent);
  expect(contentData.values[1].value).toBe(danishTextContent);
});