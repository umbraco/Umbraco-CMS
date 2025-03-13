import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Numeric';
let dataTypeDefaultData = null;
const editorAlias = 'Umbraco.Integer';
const editorUiAlias = 'Umb.PropertyEditorUi.Integer';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoUi.dataType.goToDataType(dataTypeName);
  dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  if (dataTypeDefaultData !== null) {
    await umbracoApi.dataType.update(dataTypeDefaultData.id, dataTypeDefaultData);
  }
});

test('can update minimum value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minimumValue = -5;

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

  // Act
  await umbracoUi.dataType.enterStepSizeValue(stepSizeValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'step', stepSizeValue)).toBeTruthy();
});

// Skip this test as currently this setting is removed.
test.skip('can allow decimals', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.clickAllowDecimalsToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'allowDecimals', true)).toBeTruthy();
});

// TODO: Remove skip when the front-end is ready. Currently you still can update the minimum greater than the maximum.
test.skip('cannot update the minimum greater than the maximum', async ({umbracoUi}) => {
  // Arrange
  const minimumValue = 5;
  const maximumValue = 2;

  // Act
  await umbracoUi.dataType.enterMinimumValue(minimumValue.toString());
  await umbracoUi.dataType.enterMaximumValue(maximumValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isErrorNotificationVisible();
});

test('the default configuration is correct', async ({umbracoUi}) => {
  // Assert
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.numericSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.numericSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
  expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
  expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
  expect(dataTypeDefaultData.values).toEqual([]);
});
