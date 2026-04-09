import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const documentTypeName = 'TestDocumentType';
const compositionDocumentTypeName = 'CompositionDocumentType';
const secondCompositionDocumentTypeName = 'SecondCompositionDocumentType';
const parentDocumentTypeName = 'ParentDocumentType';
const childDocumentTypeName = 'ChildDocumentType';
const dataTypeName = 'Textstring';
const secondDataTypeName = 'Numeric';
const groupName = 'TestGroup';
const secondGroupName = 'SecondGroup';
let dataTypeData = null;

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(compositionDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondCompositionDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(parentDocumentTypeName);
  await umbracoUi.goToBackOffice();
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(compositionDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondCompositionDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(parentDocumentTypeName);
});

test('can add a composition to a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const compositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(compositionDocumentTypeName, dataTypeName, dataTypeData.id, groupName);
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.documentType.clickCompositionsButton();
  await umbracoUi.documentType.clickModalMenuItemWithName(compositionDocumentTypeName);
  await umbracoUi.documentType.clickSubmitButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  await umbracoUi.documentType.isInheritedGroupVisible(groupName, compositionDocumentTypeName);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.compositions.length).toBe(1);
  expect(documentTypeData.compositions[0].documentType.id).toBe(compositionDocumentTypeId);
  expect(documentTypeData.compositions[0].compositionType).toBe('Composition');
});

test('can add multiple compositions to a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondDataTypeData = await umbracoApi.dataType.getByName(secondDataTypeName);
  const compositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(compositionDocumentTypeName, dataTypeName, dataTypeData.id, groupName);
  const secondCompositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(secondCompositionDocumentTypeName, secondDataTypeName, secondDataTypeData.id, secondGroupName);
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.documentType.clickCompositionsButton();
  await umbracoUi.documentType.clickModalMenuItemWithName(compositionDocumentTypeName);
  await umbracoUi.documentType.clickModalMenuItemWithName(secondCompositionDocumentTypeName);
  await umbracoUi.documentType.clickSubmitButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.compositions.length).toBe(2);
  const compositionIds = documentTypeData.compositions.map(c => c.documentType.id);
  expect(compositionIds).toContain(compositionDocumentTypeId);
  expect(compositionIds).toContain(secondCompositionDocumentTypeId);
});

test('can remove a composition from a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const compositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(compositionDocumentTypeName, dataTypeName, dataTypeData.id, groupName);
  await umbracoApi.documentType.createDocumentTypeWithAComposition(documentTypeName, compositionDocumentTypeId);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short);
  await umbracoUi.documentType.clickCompositionsButton();
  await umbracoUi.documentType.clickModalMenuItemWithName(compositionDocumentTypeName);
  await umbracoUi.documentType.clickSubmitButton();
  await umbracoUi.documentType.clickConfirmToSubmitButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  await umbracoUi.documentType.isGroupVisible(groupName, false);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.compositions).toEqual([]);
});

test('can add a composition with properties in a tab to a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const tabName = 'CompositionTab';
  const compositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorInTab(compositionDocumentTypeName, dataTypeName, dataTypeData.id, tabName, groupName);
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.documentType.clickCompositionsButton();
  await umbracoUi.documentType.clickModalMenuItemWithName(compositionDocumentTypeName);
  await umbracoUi.documentType.clickSubmitButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.compositions.length).toBe(1);
  expect(documentTypeData.compositions[0].documentType.id).toBe(compositionDocumentTypeId);
});

test('can add a composition to a document type that already has properties', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondDataTypeData = await umbracoApi.dataType.getByName(secondDataTypeName);
  const compositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(compositionDocumentTypeName, dataTypeName, dataTypeData.id, groupName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, secondDataTypeName, secondDataTypeData.id, secondGroupName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.documentType.clickCompositionsButton();
  await umbracoUi.documentType.clickModalMenuItemWithName(compositionDocumentTypeName);
  await umbracoUi.documentType.clickSubmitButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.compositions.length).toBe(1);
  expect(documentTypeData.compositions[0].documentType.id).toBe(compositionDocumentTypeId);
  // Verify existing property is still there
  expect(documentTypeData.properties.length).toBe(1);
  expect(documentTypeData.properties[0].dataType.id).toBe(secondDataTypeData.id);
});

test('cannot add a composition with conflicting property aliases to a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Both document types have a property with the same alias (derived from dataTypeName = 'Textstring')
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(compositionDocumentTypeName, dataTypeName, dataTypeData.id, groupName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, secondGroupName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.documentType.clickCompositionsButton();

  // Assert
  await umbracoUi.documentType.isModalMenuItemWithNameDisabled(compositionDocumentTypeName);
});

test('can remove one composition when multiple exist in a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondDataTypeData = await umbracoApi.dataType.getByName(secondDataTypeName);
  const compositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(compositionDocumentTypeName, dataTypeName, dataTypeData.id, groupName);
  const secondCompositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(secondCompositionDocumentTypeName, secondDataTypeName, secondDataTypeData.id, secondGroupName);
  // Create document type with both compositions via API
  await umbracoApi.documentType.createDocumentTypeWithTwoCompositions(documentTypeName, compositionDocumentTypeId, secondCompositionDocumentTypeId);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short);
  await umbracoUi.documentType.clickCompositionsButton();
  await umbracoUi.documentType.clickModalMenuItemWithName(compositionDocumentTypeName);
  await umbracoUi.documentType.clickSubmitButton();
  await umbracoUi.documentType.clickConfirmToSubmitButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.compositions.length).toBe(1);
  expect(documentTypeData.compositions[0].documentType.id).toBe(secondCompositionDocumentTypeId);
});

// Inheritance
test('can create a document type with inheritance', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(parentDocumentTypeName, dataTypeName, dataTypeData.id, groupName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.clickRootFolderCaretButton();
  await umbracoUi.documentType.clickActionsMenuForDocumentType(parentDocumentTypeName);
  await umbracoUi.documentType.clickCreateActionMenuOption();
  await umbracoUi.documentType.clickCreateDocumentTypeButton();
  await umbracoUi.documentType.enterDocumentTypeName(childDocumentTypeName);
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeCreated();

  // Assert
  const childDocumentTypeData = await umbracoApi.documentType.getByName(childDocumentTypeName);
  expect(childDocumentTypeData.compositions.length).toBe(1);
  expect(childDocumentTypeData.compositions[0].compositionType).toBe('Inheritance');
});

test('can see inherited properties from parent document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(parentDocumentTypeName, dataTypeName, dataTypeData.id, groupName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.clickRootFolderCaretButton();
  await umbracoUi.documentType.clickActionsMenuForDocumentType(parentDocumentTypeName);
  await umbracoUi.documentType.clickCreateActionMenuOption();
  await umbracoUi.documentType.clickCreateDocumentTypeButton();
  await umbracoUi.documentType.enterDocumentTypeName(childDocumentTypeName);
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeCreated();

  // Assert
  await umbracoUi.documentType.isInheritedGroupVisible(groupName, parentDocumentTypeName);
});

test('composed properties are visible and read-only in the document type editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const compositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(compositionDocumentTypeName, dataTypeName, dataTypeData.id, groupName);
  await umbracoApi.documentType.createDocumentTypeWithAComposition(documentTypeName, compositionDocumentTypeId);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

  // Assert
  await umbracoUi.documentType.isInheritedGroupVisible(groupName, compositionDocumentTypeName);
});

test('child document type inherits properties from both parent and composition', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondDataTypeData = await umbracoApi.dataType.getByName(secondDataTypeName);
  const compositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(compositionDocumentTypeName, dataTypeName, dataTypeData.id, groupName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditorAndComposition(parentDocumentTypeName, secondDataTypeName, secondDataTypeData.id, secondGroupName, compositionDocumentTypeId);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.clickRootFolderCaretButton();
  await umbracoUi.documentType.clickActionsMenuForDocumentType(parentDocumentTypeName);
  await umbracoUi.documentType.clickCreateActionMenuOption();
  await umbracoUi.documentType.clickCreateDocumentTypeButton();
  await umbracoUi.documentType.enterDocumentTypeName(childDocumentTypeName);
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeCreated();

  // Assert
  const childData = await umbracoApi.documentType.getByName(childDocumentTypeName);
  expect(childData.compositions.length).toBe(1);
  expect(childData.compositions[0].compositionType).toBe('Inheritance');
  await umbracoUi.documentType.isInheritedGroupVisible(secondGroupName, parentDocumentTypeName);
  await umbracoUi.documentType.isInheritedGroupVisible(groupName, compositionDocumentTypeName);
});

test('cannot use a document type with compositions as a composition', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const compositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(compositionDocumentTypeName, dataTypeName, dataTypeData.id, groupName);
  await umbracoApi.documentType.createDocumentTypeWithAComposition(secondCompositionDocumentTypeName, compositionDocumentTypeId);
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.documentType.clickCompositionsButton();

  // Assert
  await umbracoUi.documentType.isModalMenuItemWithNameVisible(secondCompositionDocumentTypeName, false);
  // The original composition (which has no compositions itself) should be available
  await umbracoUi.documentType.isModalMenuItemWithNameVisible(compositionDocumentTypeName);
});
