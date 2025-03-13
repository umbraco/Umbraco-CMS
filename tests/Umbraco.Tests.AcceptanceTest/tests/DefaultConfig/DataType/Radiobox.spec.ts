import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Radiobox';
let dataTypeDefaultData = null;
const editorAlias = 'Umbraco.RadioButtonList';
const editorUiAlias = 'Umb.PropertyEditorUi.RadioButtonList';

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

test('can add option', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const optionName = 'Test option';
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionName(optionName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'items', [optionName])).toBeTruthy();
});

test('can remove option', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const removedOptionName = 'Removed Option';
  const customDataType = 'Custom Radiobox';
  await umbracoApi.dataType.createRadioDataTypeWithOptions(customDataType, [removedOptionName]);
  await umbracoUi.dataType.goToDataType(customDataType);

  // Act
  await umbracoUi.dataType.removeOptionByName(removedOptionName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataType, 'items', [removedOptionName])).toBeFalsy();

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(customDataType);
});

test('the default configuration is correct', async ({umbracoUi}) => {
  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Assert
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.radioboxSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.radioboxSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
  expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
  expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
  expect(dataTypeDefaultData.values).toEqual([]);
});