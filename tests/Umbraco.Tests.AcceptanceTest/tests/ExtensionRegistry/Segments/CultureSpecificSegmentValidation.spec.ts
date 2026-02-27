import {test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

// Regression tests for https://github.com/umbraco/Umbraco-CMS/issues/21029

// Content
const contentName = 'TestSegmentValidationContent';
// DocumentType
const documentTypeName = 'TestSegmentValidationDocType';
let documentTypeId = '';
// DataType
const dataTypeName = 'Textstring';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.createDanishLanguage();
  await umbracoApi.language.createFrenchLanguage();
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'TestGroup', true, true, true, true, true);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.ensureNameNotExists('Danish');
  await umbracoApi.language.ensureNameNotExists('French');
});

test('can save content in culture without culture-specific segment', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, 'English default', dataTypeName, true);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSelectVariantButton();
  await umbracoUi.content.clickVariantAddModeButtonForLanguageName('Danish');
  await umbracoUi.content.enterContentName(contentName);

  // Act
  await umbracoUi.content.enterTextstring('Danish default');
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveModalButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  const daDefault = contentData.values.find(v => v.culture === 'da' && v.segment === null);
  expect(daDefault).toBeTruthy();
  expect(daDefault.value).toBe('Danish default');
});

test('can publish content in culture without culture-specific segment', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, 'English default', dataTypeName, true);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
});
