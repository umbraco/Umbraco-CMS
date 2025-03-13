import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Approved Color';
let dataTypeDefaultData = null;
let dataTypeData = null;
const colorValue = 'ffffff';
const colorLabel = '';
const editorAlias = 'Umbraco.ColorPicker';
const editorUiAlias = 'Umb.PropertyEditorUi.ColorPicker';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  if (dataTypeDefaultData !== null) {
    await umbracoApi.dataType.update(dataTypeDefaultData.id, dataTypeDefaultData);
  }
});

test('can include label', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.clickIncludeLabelsToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'useLabel', true)).toBeTruthy();
});

test('can add color', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.addColor(colorValue);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesApprovedColorHaveColor(dataTypeName, colorValue)).toBeTruthy();
});

test('can remove color', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeName = 'Custom Approved Color';
  await umbracoApi.dataType.createApprovedColorDataTypeWithOneItem(customDataTypeName, colorLabel, colorValue);

  // Act
  await umbracoUi.dataType.removeColorByValue(colorValue);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  const customDataTypeData = await umbracoApi.dataType.getByName(customDataTypeName);
  expect(customDataTypeData.values).toEqual([]);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('the default configuration is correct', async ({umbracoUi}) => {
  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Assert
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.approvedColorSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.approvedColorSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
  expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
  expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
  expect(dataTypeDefaultData.values).toEqual([]);
});