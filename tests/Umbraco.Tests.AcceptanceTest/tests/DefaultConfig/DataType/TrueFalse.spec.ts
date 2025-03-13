import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'True/false';
let dataTypeDefaultData = null;
const editorAlias = 'Umbraco.TrueFalse';
const editorUiAlias = 'Umb.PropertyEditorUi.Toggle';

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

test('can update preset value state', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.clickPresetValueToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'default', true)).toBeTruthy();
});

test('can update show toggle labels', async ({umbracoApi, umbracoUi}) => {
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

  // Act
  await umbracoUi.dataType.enterLabelOffValue(labelOffValue);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'labelOff', labelOffValue)).toBeTruthy();
});

test('the default configuration is correct', async ({umbracoUi}) => {
  // Assert
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.trueFalseSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.trueFalseSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
  expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
  expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
  expect(dataTypeDefaultData.values).toEqual([]);
});