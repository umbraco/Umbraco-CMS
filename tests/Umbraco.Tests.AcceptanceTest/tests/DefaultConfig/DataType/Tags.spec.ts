import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Tags';
const editorAlias = 'Umbraco.Tags';
const editorUiAlias = 'Umb.PropertyEditorUi.Tags';
const customDataTypeName = 'Custom Tags';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can update define a tag group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const tagGroup = 'testTagGroup';
  await umbracoApi.dataType.createDefaultTagsDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterDefineTagGroupValue(tagGroup);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'group', tagGroup)).toBeTruthy();
});

test('can select storage type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const storageType = 'Csv';
  await umbracoApi.dataType.createDefaultTagsDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.selectStorageTypeOption(storageType);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'storageType', storageType)).toBeTruthy();
});

test('the default configuration is correct', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Assert
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.tagsSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.tagsSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
  const dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
  expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'group', 'default', dataTypeDefaultData)).toBeTruthy();
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'storageType', 'Json', dataTypeDefaultData)).toBeTruthy();
});
