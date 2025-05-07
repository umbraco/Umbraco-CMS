import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Multi URL Picker';
const editorAlias = 'Umbraco.MultiUrlPicker';
const editorUiAlias = 'Umb.PropertyEditorUi.MultiUrlPicker';
const customDataTypeName = 'Custom Multi URL Picker';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can update minimum number of items value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minimumValue = 2;
  await umbracoApi.dataType.createDefaultMultiUrlPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterMinimumNumberOfItemsValue(minimumValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'minNumber', minimumValue)).toBeTruthy();
});

test('can update maximum number of items value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maximumValue = 2;
  await umbracoApi.dataType.createDefaultMultiUrlPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterMaximumNumberOfItemsValue(maximumValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'maxNumber', maximumValue)).toBeTruthy();
});

test('can enable ignore user start nodes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultMultiUrlPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickIgnoreUserStartNodesToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'ignoreUserStartNodes', true)).toBeTruthy();
});

test('can update overlay size', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const overlaySizeValue = 'large';
  await umbracoApi.dataType.createDefaultMultiUrlPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.chooseOverlaySizeByValue(overlaySizeValue);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'overlaySize', overlaySizeValue)).toBeTruthy();
});

test('can update hide anchor/query string input', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultMultiUrlPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickHideAnchorQueryStringInputToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'hideAnchor', true)).toBeTruthy();
});

// TODO: Remove skip when the front-end is ready. Currently you still can update the minimum greater than the maximum.
test.skip('cannot update the minimum number of items greater than the maximum', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minimumValue = 5;
  const maximumValue = 2;
  await umbracoApi.dataType.createDefaultMultiUrlPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterMinimumNumberOfItemsValue(minimumValue.toString());
  await umbracoUi.dataType.enterMaximumNumberOfItemsValue(maximumValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isErrorNotificationVisible();
});

test('the default configuration is correct', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Assert
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.multiURLPickerSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.multiURLPickerSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
  const dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
  expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
  expect(dataTypeDefaultData.values).toEqual([]);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'minNumber')).toBeFalsy();
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'maxNumber')).toBeFalsy();
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'ignoreUserStartNodes')).toBeFalsy();
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'overlaySize')).toBeFalsy();
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'hideAnchor')).toBeFalsy();
});