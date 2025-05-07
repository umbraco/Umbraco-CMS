import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Textstring';
const editorAlias = 'Umbraco.TextBox';
const editorUiAlias = 'Umb.PropertyEditorUi.TextBox';
const customDataTypeName = 'Custom Textstring';

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
  await umbracoApi.dataType.createTextstringDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterMaximumAllowedCharactersValue(maxCharsValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'maxChars', maxCharsValue)).toBeTruthy();
});

// Remove fixme when the front-end is ready. The "Input type" should be removed.
test.fixme('the default configuration is correct', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Assert
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.textstringSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.textstringSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
  const dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
  expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
  expect(dataTypeDefaultData.values).toEqual([]);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'maxChars')).toBeFalsy();
});
