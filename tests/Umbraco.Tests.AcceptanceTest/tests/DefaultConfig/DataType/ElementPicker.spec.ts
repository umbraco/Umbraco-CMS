import {test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Element Picker';
const customDataTypeName = 'Custom Element Picker';
const editorAlias = 'Umbraco.ElementPicker';
const editorUiAlias = 'Umb.PropertyEditorUi.ElementPicker';
const elementTypeName = 'TestElementPickerElementType';
const elementFolderName = 'TestElementPickerFolder';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.element.ensureNameNotExists(elementFolderName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.element.ensureNameNotExists(elementFolderName);
});

test('can create an element picker data type', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.clickActionsMenuForName('Data Types');
  await umbracoUi.dataType.clickCreateActionMenuOption();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(customDataTypeName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor(dataTypeName);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeCreated();

  // Assert
  await umbracoUi.dataType.isDataTypeTreeItemVisible(customDataTypeName);
  expect(await umbracoApi.dataType.doesNameExist(customDataTypeName)).toBeTruthy();
  const dataTypeData = await umbracoApi.dataType.getByName(customDataTypeName);
  expect(dataTypeData.editorAlias).toBe(editorAlias);
  expect(dataTypeData.editorUiAlias).toBe(editorUiAlias);
  expect(dataTypeData.values).toEqual([]);
});

test('can set minimum amount', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minAmount = 2;
  await umbracoApi.dataType.createDefaultElementPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterMinAmount(minAmount.toString());
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesElementPickerHaveMinAndMaxAmount(customDataTypeName, minAmount)).toBeTruthy();
});

test('can set maximum amount', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maxAmount = 5;
  await umbracoApi.dataType.createDefaultElementPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterMaxAmount(maxAmount.toString());
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesElementPickerHaveMinAndMaxAmount(customDataTypeName, undefined, maxAmount)).toBeTruthy();
});

test('can set minimum and maximum amount', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minAmount = 1;
  const maxAmount = 10;
  await umbracoApi.dataType.createDefaultElementPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterAmountValue(minAmount.toString(), maxAmount.toString());
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesElementPickerHaveMinAndMaxAmount(customDataTypeName, minAmount, maxAmount)).toBeTruthy();
});

test('can set accepted types', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName, true);
  await umbracoApi.dataType.createDefaultElementPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.addElementPickerAcceptedType(elementTypeName);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'allowedContentTypes', elementTypeId)).toBeTruthy();
});

test('can set start node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementFolderId = await umbracoApi.element.createDefaultElementFolder(elementFolderName);
  await umbracoApi.dataType.createDefaultElementPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.addElementPickerStartNode(elementFolderName);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'startNodeId', [elementFolderId])).toBeTruthy();
});
