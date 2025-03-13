import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Textarea';
let dataTypeDefaultData = null;
const editorAlias = 'Umbraco.TextArea';
const editorUiAlias = 'Umb.PropertyEditorUi.TextArea';

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

test('can update maximum allowed characters value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maxCharsValue = 126;

  // Act
  await umbracoUi.dataType.enterMaximumAllowedCharactersValue(maxCharsValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'maxChars', maxCharsValue)).toBeTruthy();
});

test('can update number of rows value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const numberOfRowsValue = 9;

  // Act
  await umbracoUi.dataType.enterNumberOfRowsValue(numberOfRowsValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'rows', numberOfRowsValue)).toBeTruthy();
});

// Skip this test as currently this setting is removed now
test.skip('can update min height (pixels) value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minHeightValue = 150;

  // Act
  await umbracoUi.dataType.enterMinHeightValue(minHeightValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'minHeight', minHeightValue)).toBeTruthy();
});

// Skip this test as currently this setting is removed now
test.skip('can update max height (pixels) value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maxHeightValue = 300;

  // Act
  await umbracoUi.dataType.enterMaxHeightValue(maxHeightValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'maxHeight', maxHeightValue)).toBeTruthy();
});

// TODO: Remove skip when the front-end is ready. Currently you still can update the minimum greater than the maximum.
test.skip('cannot update the min height greater than the max height', async ({umbracoUi}) => {
  // Arrange
  const minHeightValue = 150;
  const maxHeightValue = 100;

  // Act
  await umbracoUi.dataType.enterMinHeightValue(minHeightValue.toString());
  await umbracoUi.dataType.enterMaxHeightValue(maxHeightValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isErrorNotificationVisible();
});

// Remove fixme when the front-end is ready. Two unneccessary settings: Min height and Max height should be removed.
test.fixme('the default configuration is correct', async ({umbracoUi}) => {
  // Assert
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.textareaSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.textareaSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
  expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
  expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
  expect(dataTypeDefaultData.values).toEqual([]);
});
