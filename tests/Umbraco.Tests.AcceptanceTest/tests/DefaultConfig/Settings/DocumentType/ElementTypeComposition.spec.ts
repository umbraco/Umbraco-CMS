import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const elementTypeName = 'TestElementType';
const compositionElementTypeName = 'CompositionElementType';
const blockGridDataTypeName = 'TestBlockGridDataType';
const blockGridDocumentTypeName = 'BlockGridDocumentType';
const contentName = 'TestContent';
const dataTypeName = 'Textstring';
const groupName = 'TestGroup';
let dataTypeData = null;

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(blockGridDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(compositionElementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
  await umbracoUi.goToBackOffice();
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(blockGridDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(compositionElementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
});

test('can add a composition to an element type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const compositionElementTypeId = await umbracoApi.documentType.createDefaultElementType(compositionElementTypeName, groupName, dataTypeName, dataTypeData.id);
  await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(elementTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.documentType.clickCompositionsButton();
  await umbracoUi.documentType.clickModalMenuItemWithName(compositionElementTypeName);
  await umbracoUi.documentType.clickSubmitButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  const elementTypeData = await umbracoApi.documentType.getByName(elementTypeName);
  expect(elementTypeData.compositions.length).toBe(1);
  expect(elementTypeData.compositions[0].documentType.id).toBe(compositionElementTypeId);
});

test('can remove a composition from an element type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const compositionElementTypeId = await umbracoApi.documentType.createDefaultElementType(compositionElementTypeName, groupName, dataTypeName, dataTypeData.id);
  await umbracoApi.documentType.createElementTypeWithAComposition(elementTypeName, compositionElementTypeId);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(elementTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short);
  await umbracoUi.documentType.clickCompositionsButton();
  await umbracoUi.documentType.clickModalMenuItemWithName(compositionElementTypeName);
  await umbracoUi.documentType.clickSubmitButton();
  await umbracoUi.documentType.clickConfirmToSubmitButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  const elementTypeData = await umbracoApi.documentType.getByName(elementTypeName);
  expect(elementTypeData.compositions).toEqual([]);
});

test('can use an element type with composition in a block grid', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const propertyInBlock = 'Textstring';
  const inputText = 'Block content from composition';
  const compositionElementTypeId = await umbracoApi.documentType.createDefaultElementType(compositionElementTypeName, groupName, propertyInBlock, dataTypeData.id);
  const elementTypeId = await umbracoApi.documentType.createElementTypeWithAComposition(elementTypeName, compositionElementTypeId);
  const blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithABlock(blockGridDataTypeName, elementTypeId);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(blockGridDocumentTypeName, blockGridDataTypeName, blockGridDataTypeId);
  // Create content
  const blockGridDocumentTypeData = await umbracoApi.documentType.getByName(blockGridDocumentTypeName);
  await umbracoApi.document.createDefaultDocument(contentName, blockGridDocumentTypeData.id);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.enterTextstring(inputText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values.length).toBeGreaterThan(0);
  const blockGridValue = contentData.values[0].value;
  expect(blockGridValue.contentData[0].values[0].value).toEqual(inputText);
});
