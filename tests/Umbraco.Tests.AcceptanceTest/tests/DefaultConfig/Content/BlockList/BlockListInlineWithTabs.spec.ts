import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Tests regression issue: https://github.com/umbraco/Umbraco-CMS/issues/20866
// Block in block list inline mode no longer render properties when document type has tab structure
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const customDataTypeName = 'Custom Block List Inline';
const elementTypeName = 'BlockListElement';
const dataTypeName = 'Textstring';
const blockListTabName = 'BlockListTab';
const secondTabName = 'ContentTab';
const groupName = 'testGroup';
let elementTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  // Create a simple element type (no tabs) for the block
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
  const customDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithInlineEditingModeAndABlock(customDataTypeName, elementTypeId);
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorInTabAndAnotherTab(documentTypeName, customDataTypeName, customDataTypeId, blockListTabName, secondTabName, dataTypeName, textStringData.id, groupName, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickTabWithName(blockListTabName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickTextButtonWithName(elementTypeName);

  // Assert
  await umbracoUi.content.clickInlineBlockCaretButtonForName(elementTypeName);
  await umbracoUi.content.isInlineBlockPropertyVisible(dataTypeName);
});

test('can save block in inline mode when document type has tab structure', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const inputText = 'Test inline editing with tabs';
  const customDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithInlineEditingModeAndABlock(customDataTypeName, elementTypeId);
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorInTabAndAnotherTab(documentTypeName, customDataTypeName, customDataTypeId, blockListTabName, secondTabName, dataTypeName, textStringData.id, groupName, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickTabWithName(blockListTabName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickTextButtonWithName(elementTypeName);
  await umbracoUi.content.clickInlineBlockCaretButtonForName(elementTypeName);
  await umbracoUi.content.enterInlineBlockPropertyValue(dataTypeName, inputText);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.contentData[0].values[0].value).toEqual(inputText);
});

test('can render properties in inline mode when block list is on second tab', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithInlineEditingModeAndABlock(customDataTypeName, elementTypeId);
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  // Block list is on second tab, textstring is on first tab
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorInTabAndAnotherTab(documentTypeName, dataTypeName, textStringData.id, blockListTabName, secondTabName, customDataTypeName, customDataTypeId, groupName, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickTabWithName(secondTabName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickTextButtonWithName(elementTypeName);

  // Assert
  await umbracoUi.content.clickInlineBlockCaretButtonForName(elementTypeName);
  await umbracoUi.content.isInlineBlockPropertyVisible(dataTypeName);
});

test('can render properties in inline mode when block list is directly under tab without group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create document type with properties directly under tabs (no groups)
  const customDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithInlineEditingModeAndABlock(customDataTypeName, elementTypeId);
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  // Note: Using createDocumentTypeWithPropertyEditorDirectlyInTwoTabs - properties are directly under tabs, not in groups
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorDirectlyInTwoTabs(documentTypeName, customDataTypeName, customDataTypeId, blockListTabName, secondTabName, dataTypeName, textStringData.id, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickTabWithName(blockListTabName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickTextButtonWithName(elementTypeName);

  // Assert - Verify that the property is visible in inline mode when block list is directly under tab
  await umbracoUi.content.clickInlineBlockCaretButtonForName(elementTypeName);
  await umbracoUi.content.isInlineBlockPropertyVisible(dataTypeName);
});
