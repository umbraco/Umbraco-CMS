import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Checkbox list';
const customDataTypeName = 'Custom Checkbox List';
const editorAlias = 'Umbraco.CheckBoxList';
const editorUiAlias = 'Umb.PropertyEditorUi.CheckBoxList';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can add option', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const optionName = 'Test option';
  await umbracoApi.dataType.createCheckboxListDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionName(optionName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'items', [optionName])).toBeTruthy();
});

test('can remove option', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const removedOptionName = 'Removed Option';
  await umbracoApi.dataType.createCheckboxListDataType(customDataTypeName, [removedOptionName]);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.removeOptionByName(removedOptionName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  const customDataTypeData = await umbracoApi.dataType.getByName(customDataTypeName);
  expect(customDataTypeData.values).toEqual([]);
});

test('can update option', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const optionName = 'Test option';
  const updatedOptionName = 'Updated option';
  await umbracoApi.dataType.createCheckboxListDataType(customDataTypeName, [optionName]);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterOptionName(updatedOptionName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'items', [updatedOptionName])).toBeTruthy();
});

test('the default configuration is correct', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Assert
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.checkboxListSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.checkboxListSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
  const dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName)
  expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
  expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
  expect(dataTypeDefaultData.values).toEqual([]);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'items')).toBeFalsy();
});