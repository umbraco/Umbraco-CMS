import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

// Content Name
const contentName = 'NestedInlineBlockListContent';

// Document Type
const documentTypeName = 'NestedInlineBlockListDocumentType';
const documentTypeTabName = 'NestedInlineDocumentTab';
const documentTypeGroupName = 'NestedInlineDocumentGroup';

// Block Lists
const outerBlockListDataTypeName = 'NestedInlineOuterBlockList';
const innerBlockListDataTypeName = 'NestedInlineInnerBlockList';

// Element Types
// Each level carries its own tab, and the tab names differ per level so a tab of one level can never
// be mistaken for a tab of the level nesting it.
const outerElementTypeName = 'NestedInlineOuterElement';
const outerElementTabName = 'NestedInlineOuterTab';
const outerElementGroupName = 'NestedInlineOuterGroup';
const innerElementTypeName = 'NestedInlineInnerElement';
const innerElementTabName = 'NestedInlineInnerTab';
const innerElementGroupName = 'NestedInlineInnerGroup';

// Text String
const textStringDataTypeName = 'Textstring';

// A block is rendered inside the block that contains it, so a locator matching the inner element type
// name also matches the outer entry wrapping it. The inner block is the second match.
const innerBlockIndex = 1;

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(outerBlockListDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(outerElementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(innerBlockListDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(innerElementTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(outerBlockListDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(outerElementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(innerBlockListDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(innerElementTypeName);
});

test('can see the properties of a block added to a nested block list with inline editing enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const innerElementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(innerElementTypeName, innerElementTabName, innerElementGroupName, textStringDataTypeName, textStringDataType.id);
  const innerBlockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithInlineEditingModeAndABlock(innerBlockListDataTypeName, innerElementTypeId);
  const outerElementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(outerElementTypeName, outerElementTabName, outerElementGroupName, innerBlockListDataTypeName, innerBlockListDataTypeId);
  const outerBlockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithInlineEditingModeAndABlock(outerBlockListDataTypeName, outerElementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorInTab(documentTypeName, outerBlockListDataTypeName, outerBlockListDataTypeId, documentTypeTabName, documentTypeGroupName, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickAddBlockWithNameButton(innerElementTypeName);

  // Assert
  await umbracoUi.content.isInlineBlockPropertyVisibleForBlockWithName(innerElementTypeName, textStringDataTypeName, true, innerBlockIndex);
});

test('can see the properties of a block in a nested block list after collapsing and expanding it', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const innerElementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(innerElementTypeName, innerElementTabName, innerElementGroupName, textStringDataTypeName, textStringDataType.id);
  const innerBlockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithInlineEditingModeAndABlock(innerBlockListDataTypeName, innerElementTypeId);
  const outerElementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(outerElementTypeName, outerElementTabName, outerElementGroupName, innerBlockListDataTypeName, innerBlockListDataTypeId);
  const outerBlockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithInlineEditingModeAndABlock(outerBlockListDataTypeName, outerElementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorInTab(documentTypeName, outerBlockListDataTypeName, outerBlockListDataTypeId, documentTypeTabName, documentTypeGroupName, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickAddBlockWithNameButton(innerElementTypeName);

  // Act
  await umbracoUi.content.clickInlineBlockCaretButtonForName(innerElementTypeName, innerBlockIndex);
  await umbracoUi.content.isInlineBlockPropertyVisibleForBlockWithName(innerElementTypeName, textStringDataTypeName, false, innerBlockIndex);
  await umbracoUi.content.clickInlineBlockCaretButtonForName(innerElementTypeName, innerBlockIndex);

  // Assert
  await umbracoUi.content.isInlineBlockPropertyVisibleForBlockWithName(innerElementTypeName, textStringDataTypeName, true, innerBlockIndex);
});

test('can enter a value in a block added to a nested block list with inline editing enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const inputText = 'This is a nested inline block';
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const innerElementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(innerElementTypeName, innerElementTabName, innerElementGroupName, textStringDataTypeName, textStringDataType.id);
  const innerBlockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithInlineEditingModeAndABlock(innerBlockListDataTypeName, innerElementTypeId);
  const outerElementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(outerElementTypeName, outerElementTabName, outerElementGroupName, innerBlockListDataTypeName, innerBlockListDataTypeId);
  const outerBlockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithInlineEditingModeAndABlock(outerBlockListDataTypeName, outerElementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorInTab(documentTypeName, outerBlockListDataTypeName, outerBlockListDataTypeId, documentTypeTabName, documentTypeGroupName, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickAddBlockWithNameButton(innerElementTypeName);
  await umbracoUi.content.enterInlineBlockPropertyValue(textStringDataTypeName, inputText, innerBlockIndex);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  const outerBlockValue = contentData.values[0].value.contentData[0].values[0].value;
  expect(outerBlockValue.contentData[0].values[0].value).toEqual(inputText);
});
