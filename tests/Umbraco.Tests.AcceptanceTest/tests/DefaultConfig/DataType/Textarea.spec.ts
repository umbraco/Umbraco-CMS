import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Textarea';
const editorAlias = 'Umbraco.TextArea';
const editorUiAlias = 'Umb.PropertyEditorUi.TextArea';
const customDataTypeName = 'Custom Textarea';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can update maximum allowed characters value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maxCharsValue = 126;
  await umbracoApi.dataType.createTextareaDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterMaximumAllowedCharactersValue(maxCharsValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'maxChars', maxCharsValue)).toBeTruthy();
});

test('can update number of rows value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const numberOfRowsValue = 9;
  await umbracoApi.dataType.createTextareaDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterNumberOfRowsValue(numberOfRowsValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'rows', numberOfRowsValue)).toBeTruthy();
});

// Skip this test as currently this setting is removed now
test.skip('can update min height (pixels) value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minHeightValue = 150;
  await umbracoApi.dataType.createTextareaDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterMinHeightValue(minHeightValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'minHeight', minHeightValue)).toBeTruthy();
});

// Skip this test as currently this setting is removed now
test.skip('can update max height (pixels) value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maxHeightValue = 300;
  await umbracoApi.dataType.createTextareaDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterMaxHeightValue(maxHeightValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'maxHeight', maxHeightValue)).toBeTruthy();
});

// Skip this test as currently these settings are removed now
test.skip('cannot update the min height greater than the max height', async ({umbracoUi}) => {
  // Arrange
  const minHeightValue = 150;
  const maxHeightValue = 100;
  await umbracoApi.dataType.createTextareaDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterMinHeightValue(minHeightValue.toString());
  await umbracoUi.dataType.enterMaxHeightValue(maxHeightValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isErrorNotificationVisible();
});

test('the default configuration is correct', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Assert
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.textareaSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.textareaSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
  const dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
  expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
  expect(dataTypeDefaultData.values).toEqual([]);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'maxChars')).toBeFalsy();
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'rows')).toBeFalsy();
});
