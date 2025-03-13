import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Tags';
let dataTypeDefaultData = null;
const editorAlias = 'Umbraco.Tags';
const editorUiAlias = 'Umb.PropertyEditorUi.Tags';

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

test('can update define a tag group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const tagGroup = 'testTagGroup';

  // Act
  await umbracoUi.dataType.enterDefineTagGroupValue(tagGroup);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'group', tagGroup)).toBeTruthy();
});

test('can select storage type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const storageType = 'Csv';

  // Act
  await umbracoUi.dataType.selectStorageTypeOption(storageType);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'storageType', storageType)).toBeTruthy();
});

// Remove fixme when the front-end ready. Currently the settings has no description.
test.fixme('the default configuration is correct', async ({umbracoApi, umbracoUi}) => {
  // Assert
  //await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.tagsSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.tagsSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
  expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
  expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'group', 'default', dataTypeDefaultData)).toBeTruthy();
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'storageType', 'Json', dataTypeDefaultData)).toBeTruthy();
});
