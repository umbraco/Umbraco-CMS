import {test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const propertyEditorName = 'Decimal';
const customDataTypeName = 'Custom Decimal';
const editorAlias = 'Umbraco.Decimal';
const editorUiAlias = 'Umb.PropertyEditorUi.Decimal';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can create a decimal data type', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.clickActionsMenuForName('Data Types');
  await umbracoUi.dataType.clickCreateActionMenuOption();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(customDataTypeName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor(propertyEditorName);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeCreated();

  // Assert
  await umbracoUi.dataType.isDataTypeTreeItemVisible(customDataTypeName);
  expect(await umbracoApi.dataType.doesNameExist(customDataTypeName)).toBeTruthy();
  const dataTypeData = await umbracoApi.dataType.getByName(customDataTypeName);
  expect(dataTypeData.editorAlias).toBe(editorAlias);
  expect(dataTypeData.editorUiAlias).toBe(editorUiAlias);
});

test('can update minimum value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minimumValue = 1.5;
  await umbracoApi.dataType.createDecimalDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterMinimumValue(minimumValue.toString());
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'min', minimumValue)).toBeTruthy();
});

test('can update maximum value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maximumValue = 99.9;
  await umbracoApi.dataType.createDecimalDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterMaximumValue(maximumValue.toString());
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'max', maximumValue)).toBeTruthy();
});

test('can update step size value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const stepSizeValue = 0.5;
  await umbracoApi.dataType.createDecimalDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterStepSizeValue(stepSizeValue.toString());
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'step', stepSizeValue)).toBeTruthy();
});

test('the default configuration is correct', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.clickActionsMenuForName('Data Types');
  await umbracoUi.dataType.clickCreateActionMenuOption();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(customDataTypeName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor(propertyEditorName);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeCreated();

  // Assert
  const dataTypeData = await umbracoApi.dataType.getByName(customDataTypeName);
  expect(dataTypeData.editorAlias).toBe(editorAlias);
  expect(dataTypeData.editorUiAlias).toBe(editorUiAlias);
  expect(dataTypeData.values).toEqual([{alias: 'step', value: '0.01'}]);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'min')).toBeFalsy();
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'max')).toBeFalsy();
});
