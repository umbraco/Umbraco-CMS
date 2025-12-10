import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Tests regression issue: https://github.com/umbraco/Umbraco-CMS/issues/20866
// Block in block grid inline mode no longer render properties when document type has tab structure
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const customDataTypeName = 'Custom Block Grid Inline';
const elementTypeName = 'BlockGridElement';
const dataTypeName = 'Textstring';
const blockGridTabName = 'BlockGridTab';
const secondTabName = 'ContentTab';
const groupName = 'testGroup';
let elementTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can render properties in inline mode when document type has tab structure', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockWithInlineEditingMode(customDataTypeName, elementTypeId);
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorInTabAndAnotherTab(documentTypeName, customDataTypeName, customDataTypeId, blockGridTabName, secondTabName, dataTypeName, textStringData.id, groupName, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickTabWithName(blockGridTabName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);

  // Assert
  await umbracoUi.content.isTextstringPropertyVisible();
});

test('can save block in inline mode when document type has tab structure', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const inputText = 'Test inline editing with tabs';
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockWithInlineEditingMode(customDataTypeName, elementTypeId);
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorInTabAndAnotherTab(documentTypeName, customDataTypeName, customDataTypeId, blockGridTabName, secondTabName, dataTypeName, textStringData.id, groupName, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickTabWithName(blockGridTabName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.enterTextstring(inputText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.contentData[0].values[0].value).toEqual(inputText);
});

test('can render properties in inline mode when block grid is on second tab', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockWithInlineEditingMode(customDataTypeName, elementTypeId);
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  // Block grid is on second tab, textstring is on first tab
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorInTabAndAnotherTab(documentTypeName, dataTypeName, textStringData.id, blockGridTabName, secondTabName, customDataTypeName, customDataTypeId, groupName, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickTabWithName(secondTabName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);

  // Assert
  await umbracoUi.content.isTextstringPropertyVisible();
});

test('can render properties in inline mode when block grid is directly under tab without group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockWithInlineEditingMode(customDataTypeName, elementTypeId);
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorDirectlyInTwoTabs(documentTypeName, customDataTypeName, customDataTypeId, blockGridTabName, secondTabName, dataTypeName, textStringData.id, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickTabWithName(blockGridTabName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);

  // Assert
  await umbracoUi.content.isTextstringPropertyVisible();
});
