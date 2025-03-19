import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'True/false';
const editorAlias = 'Umbraco.TrueFalse';
const editorUiAlias = 'Umb.PropertyEditorUi.Toggle';
const customDataTypeName = 'Custom TrueFalse';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can update preset value state', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultTrueFalseDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickPresetValueToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'default', true)).toBeTruthy();
});

test('can update show toggle labels', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultTrueFalseDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);
  
  // Act
  await umbracoUi.dataType.clickShowToggleLabelsToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'showLabels', true)).toBeTruthy();
});

test('can update label on', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const labelOnValue = 'Test Label On';
  await umbracoApi.dataType.createDefaultTrueFalseDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);
  
  // Act
  await umbracoUi.dataType.enterLabelOnValue(labelOnValue);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'labelOn', labelOnValue)).toBeTruthy();
});

test('can update label off', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const labelOffValue = 'Test Label Off';
  await umbracoApi.dataType.createDefaultTrueFalseDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);
  
  // Act
  await umbracoUi.dataType.enterLabelOffValue(labelOffValue);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'labelOff', labelOffValue)).toBeTruthy();
});

test('the default configuration is correct', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Assert
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.trueFalseSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.trueFalseSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
  const dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
  expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
  expect(dataTypeDefaultData.values).toEqual([]);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'default')).toBeFalsy();
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'showLabels')).toBeFalsy();
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'labelOn')).toBeFalsy();
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'labelOff')).toBeFalsy();
});