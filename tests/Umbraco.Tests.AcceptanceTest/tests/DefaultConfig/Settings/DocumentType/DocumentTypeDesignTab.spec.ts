import {ConstantHelper, NotificationConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

const documentTypeName = 'TestDocumentType';
const dataTypeName = 'Approved Color';
const groupName = 'TestGroup';
const tabName = 'TestTab';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can add a property to a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickAddGroupButton();
  await umbracoUi.documentType.addPropertyEditor(dataTypeName);
  await umbracoUi.documentType.enterGroupName(groupName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);
  // Checks if the correct property was added to the document type
  expect(documentTypeData.properties[0].dataType.id).toBe(dataType.id);
});

test('can update a property in a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const newDataTypeName = 'Image Media Picker';
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.updatePropertyEditor(newDataTypeName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  const dataType = await umbracoApi.dataType.getByName(newDataTypeName);
  // Checks if the correct property was added to the document type
  expect(documentTypeData.properties[0].dataType.id).toBe(dataType.id);
});

test('can update group name in a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const newGroupName = 'UpdatedGroupName';
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.enterGroupName(newGroupName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.containers[0].name).toBe(newGroupName);
});

test('can delete a group in a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, groupName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.deleteGroup(groupName);
  await umbracoUi.documentType.clickConfirmToDeleteButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.containers.length).toBe(0);
  expect(documentTypeData.properties.length).toBe(0);
});

test('can delete a tab in a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditorInTab(documentTypeName, dataTypeName, dataTypeData.id, tabName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickRemoveTabWithName(tabName);
  await umbracoUi.documentType.clickConfirmToDeleteButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.containers.length).toBe(0);
});

test('can delete a property editor in a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, groupName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.deletePropertyEditorWithName(dataTypeName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.properties.length).toBe(0);
});

test('can create a document type with a property in a tab', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickAddTabButton();
  await umbracoUi.documentType.enterTabName(tabName);
  await umbracoUi.documentType.clickAddGroupButton();
  await umbracoUi.documentType.enterGroupName(groupName);
  await umbracoUi.documentType.addPropertyEditor(dataTypeName, 1);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(await umbracoApi.documentType.doesTabContainCorrectPropertyEditorInGroup(documentTypeName, dataTypeName, documentTypeData.properties[0].dataType.id, tabName, groupName)).toBeTruthy();
});

test('can create a document type with multiple groups', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const secondDataTypeName = 'Image Media Picker';
  const secondGroupName = 'TesterGroup';
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, groupName);
  const secondDataType = await umbracoApi.dataType.getByName(secondDataTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickAddGroupButton();
  await umbracoUi.documentType.enterGroupName(secondGroupName, 1);
  await umbracoUi.documentType.addPropertyEditor(secondDataTypeName, 1);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  expect(await umbracoApi.documentType.doesGroupContainCorrectPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, groupName)).toBeTruthy();
  expect(await umbracoApi.documentType.doesGroupContainCorrectPropertyEditor(documentTypeName, secondDataTypeName, secondDataType.id, secondGroupName)).toBeTruthy();
});

test('can create a document type with multiple tabs', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const secondDataTypeName = 'Image Media Picker';
  const secondGroupName = 'TesterGroup';
  const secondTabName = 'SecondTab';
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditorInTab(documentTypeName, dataTypeName, dataTypeData.id, tabName, groupName);
  const secondDataType = await umbracoApi.dataType.getByName(secondDataTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickAddTabButton();
  await umbracoUi.documentType.enterTabName(secondTabName);
  await umbracoUi.documentType.clickAddGroupButton();
  await umbracoUi.documentType.enterGroupName(secondGroupName);
  await umbracoUi.documentType.addPropertyEditor(secondDataTypeName, 1);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  expect(await umbracoApi.documentType.doesTabContainCorrectPropertyEditorInGroup(documentTypeName, dataTypeName, dataTypeData.id, tabName, groupName)).toBeTruthy();
  expect(await umbracoApi.documentType.doesTabContainCorrectPropertyEditorInGroup(documentTypeName, secondDataTypeName, secondDataType.id, secondTabName, secondGroupName)).toBeTruthy();
});

test('can create a document type with a composition', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const compositionDocumentTypeName = 'CompositionDocumentType';
  await umbracoApi.documentType.ensureNameNotExists(compositionDocumentTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const compositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(compositionDocumentTypeName, dataTypeName, dataTypeData.id, groupName);
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.waitForTimeout(500);
  await umbracoUi.documentType.clickCompositionsButton();
  await umbracoUi.documentType.clickButtonWithName(compositionDocumentTypeName);
  await umbracoUi.documentType.clickSubmitButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(umbracoUi.documentType.doesGroupHaveValue(groupName)).toBeTruthy();
  // Checks if the composition in the document type is correct
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.compositions[0].documentType.id).toBe(compositionDocumentTypeId);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(compositionDocumentTypeName);
});

test('can remove a composition from a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const compositionDocumentTypeName = 'CompositionDocumentType';
  await umbracoApi.documentType.ensureNameNotExists(compositionDocumentTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const compositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(compositionDocumentTypeName, dataTypeName, dataTypeData.id, groupName);
  await umbracoApi.documentType.createDocumentTypeWithAComposition(documentTypeName, compositionDocumentTypeId);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.waitForTimeout(500);
  await umbracoUi.documentType.clickCompositionsButton();
  await umbracoUi.documentType.clickButtonWithName(compositionDocumentTypeName);
  await umbracoUi.documentType.clickSubmitButton();
  await umbracoUi.documentType.clickConfirmToSubmitButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  await umbracoUi.documentType.isGroupVisible(groupName, false);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.compositions).toEqual([]);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(compositionDocumentTypeName);
});

test('can reorder groups in a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const secondGroupName = 'SecondGroup';
  await umbracoApi.documentType.createDocumentTypeWithTwoGroups(documentTypeName, dataTypeName, dataTypeData.id, groupName, secondGroupName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.documentType.goToDocumentType(documentTypeName);

  // Act
  await umbracoUi.documentType.clickReorderButton();
  const groupValues = await umbracoUi.documentType.reorderTwoGroups();
  const firstGroupValue = groupValues.firstGroupValue;
  const secondGroupValue = groupValues.secondGroupValue;
  await umbracoUi.documentType.clickIAmDoneReorderingButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  // Since we swapped sorting order, the firstGroupValue should have sortOrder 1 and the secondGroupValue should have sortOrder 0
  expect(await umbracoApi.documentType.doesDocumentTypeGroupNameContainCorrectSortOrder(documentTypeName, secondGroupValue, 0)).toBeTruthy();
  expect(await umbracoApi.documentType.doesDocumentTypeGroupNameContainCorrectSortOrder(documentTypeName, firstGroupValue, 1)).toBeTruthy();
});

// Skip this flaky tests as sometimes the properties are not dragged correctly.
test.skip('can reorder properties in a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const dataTypeNameTwo = "Second Color Picker";
  await umbracoApi.documentType.createDocumentTypeWithTwoPropertyEditors(documentTypeName, dataTypeName, dataTypeData.id, dataTypeNameTwo, dataTypeData.id);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickReorderButton();
  // Drag and Drop
  const dragFromLocator = umbracoUi.documentType.getTextLocatorWithName(dataTypeNameTwo);
  const dragToLocator = umbracoUi.documentType.getTextLocatorWithName(dataTypeName);
  await umbracoUi.documentType.dragAndDrop(dragFromLocator, dragToLocator, 0, 0, 5);
  await umbracoUi.documentType.clickIAmDoneReorderingButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.properties[0].name).toBe(dataTypeNameTwo);
  expect(documentTypeData.properties[1].name).toBe(dataTypeName);
});

// TODO: Remove skip when the frontend is ready. Currently it is impossible to reorder tab by drag and drop
test.skip('can reorder tabs in a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const secondTabName = 'SecondTab';
  await umbracoApi.documentType.createDocumentTypeWithTwoTabs(documentTypeName, dataTypeName, dataTypeData.id, tabName, secondTabName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.documentType.goToDocumentType(documentTypeName);

  // Act
  const dragToLocator = umbracoUi.documentType.getTabLocatorWithName(tabName);
  const dragFromLocator = umbracoUi.documentType.getTabLocatorWithName(secondTabName);
  await umbracoUi.documentType.clickReorderButton();
  await umbracoUi.documentType.dragAndDrop(dragFromLocator, dragToLocator, 0, 0, 10);
  await umbracoUi.documentType.clickIAmDoneReorderingButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesDocumentTypeTabNameContainCorrectSortOrder(documentTypeName, secondTabName, 0)).toBeTruthy();
  expect(await umbracoApi.documentType.doesDocumentTypeTabNameContainCorrectSortOrder(documentTypeName, tabName, 1)).toBeTruthy();
});

test('can add a description to a property in a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const descriptionText = 'This is a property';
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickEditorSettingsButton();
  await umbracoUi.documentType.enterPropertyEditorDescription(descriptionText);
  await umbracoUi.documentType.clickSubmitButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  await expect(umbracoUi.documentType.enterDescriptionTxt).toBeVisible();
  expect(umbracoUi.documentType.doesDescriptionHaveValue(descriptionText)).toBeTruthy();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.properties[0].description).toBe(descriptionText);
});

test('can set is mandatory for a property in a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickEditorSettingsButton();
  await umbracoUi.documentType.clickMandatoryToggle();
  await umbracoUi.documentType.clickSubmitButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.properties[0].validation.mandatory).toBeTruthy();
});

test('can enable validation for a property in a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const regex = '^[a-zA-Z0-9]*$';
  const regexMessage = 'Only letters and numbers are allowed';
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickEditorSettingsButton();
  await umbracoUi.documentType.selectValidationOption('');
  await umbracoUi.documentType.enterRegEx(regex);
  await umbracoUi.documentType.enterRegExMessage(regexMessage);
  await umbracoUi.documentType.clickSubmitButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.properties[0].validation.regEx).toBe(regex);
  expect(documentTypeData.properties[0].validation.regExMessage).toBe(regexMessage);
});

test('can allow vary by culture for a property in a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, groupName, true, false);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickEditorSettingsButton();
  await umbracoUi.documentType.clickSharedAcrossCulturesToggle();
  await umbracoUi.documentType.clickSubmitButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.properties[0].variesByCulture).toBeTruthy();
});

test('can set appearance to label on top for a property in a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickEditorSettingsButton();
  await umbracoUi.documentType.clickLabelAboveButton();
  await umbracoUi.documentType.clickSubmitButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.properties[0].appearance.labelOnTop).toBeTruthy();
});

test('can add a block list property with inline editing mode to a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockListDataTypeName = 'TestBlockList';
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoApi.dataType.createBlockListDataTypeWithInlineEditingMode(blockListDataTypeName, true);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickAddGroupButton();
  await umbracoUi.documentType.addPropertyEditor(blockListDataTypeName);
  await umbracoUi.documentType.enterGroupName(groupName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  const blockListDataTypeData = await umbracoApi.dataType.getByName(blockListDataTypeName);
  // Checks if the correct property was added to the document type
  expect(documentTypeData.properties[0].dataType.id).toBe(blockListDataTypeData.id);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
});
