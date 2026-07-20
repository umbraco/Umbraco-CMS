import {test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const propertyEditorName = 'Slider';
const customDataTypeName = 'Custom Slider';
const editorAlias = 'Umbraco.Slider';
const editorUiAlias = 'Umb.PropertyEditorUi.Slider';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can create a slider data type', async ({umbracoApi, umbracoUi}) => {
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
  const minimumValue = 1;
  await umbracoApi.dataType.createSliderDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterSliderMinimumValue(minimumValue.toString());
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'minVal', minimumValue)).toBeTruthy();
});

test('can update maximum value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maximumValue = 50;
  await umbracoApi.dataType.createSliderDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterSliderMaximumValue(maximumValue.toString());
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'maxVal', maximumValue)).toBeTruthy();
});

test('can update step size value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const stepSizeValue = 5;
  await umbracoApi.dataType.createSliderDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterStepSizeValue(stepSizeValue.toString());
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'step', stepSizeValue)).toBeTruthy();
});
