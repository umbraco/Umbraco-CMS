import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Numeric';
const editorAlias = 'Umbraco.Integer';
const editorUiAlias = 'Umb.PropertyEditorUi.Integer';
const customDataTypeName = 'Custom Numeric';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can update minimum value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minimumValue = -5;
  await umbracoApi.dataType.createDefaultNumericDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterMinimumValue(minimumValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'min', minimumValue)).toBeTruthy();
});

test('can update Maximum value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maximumValue = 1000000;
  await umbracoApi.dataType.createDefaultNumericDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterMaximumValue(maximumValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'max', maximumValue)).toBeTruthy();
});

test('can update step size value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const stepSizeValue = 5;
  await umbracoApi.dataType.createDefaultNumericDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterStepSizeValue(stepSizeValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'step', stepSizeValue)).toBeTruthy();
});

// Skip this test as currently this setting is removed.
test.skip('can allow decimals', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultNumericDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAllowDecimalsToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'allowDecimals', true)).toBeTruthy();
});

// TODO: Remove skip when the front-end is ready. Currently you still can update the minimum greater than the maximum.
test.skip('cannot update the minimum greater than the maximum', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minimumValue = 5;
  const maximumValue = 2;
  await umbracoApi.dataType.createDefaultNumericDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterMinimumValue(minimumValue.toString());
  await umbracoUi.dataType.enterMaximumValue(maximumValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isErrorNotificationVisible();
});

test('the default configuration is correct', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Assert
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.numericSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.numericSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
  const dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
  expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
  expect(dataTypeDefaultData.values).toEqual([]);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'min')).toBeFalsy();
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'max')).toBeFalsy();
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'step')).toBeFalsy();
});
