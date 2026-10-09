import {ConstantHelper, test, AliasHelper} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const compositionDocumentTypeName = 'CompositionDocumentType';
const dataTypeName = 'Textstring';
const groupName = 'TestGroup';
let compositionDocumentTypeId = null;

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(compositionDocumentTypeName);
  await umbracoUi.goToBackOffice();
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  compositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(compositionDocumentTypeName, dataTypeName, dataTypeData.id, groupName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(compositionDocumentTypeName);
});

test('can create content with a document type that has a composition', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  await umbracoApi.documentType.createDocumentTypeWithACompositionAndAllowAsRoot(documentTypeName, compositionDocumentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
});

test('can edit property value from composition in content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const text = 'This is a property value';
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithACompositionAndAllowAsRoot(documentTypeName, compositionDocumentTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterTextstring(text);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value).toEqual(text);
});

test('can publish content with a document type that has a composition', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const text = 'Published composition value';
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithACompositionAndAllowAsRoot(documentTypeName, compositionDocumentTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterTextstring(text);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Published');
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value).toEqual(text);
});
