import {NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'TestDataType';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
});

test('can create a data type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.clickActionsMenuAtRoot();
  await umbracoUi.dataType.clickActionsMenuCreateButton();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(dataTypeName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor('Text Box');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
});

test('can rename a data type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongDataTypeName = 'Wrong Data Type';
  await umbracoApi.dataType.ensureNameNotExists(wrongDataTypeName);
  await umbracoApi.dataType.createTextstringDataType(wrongDataTypeName);
  expect(await umbracoApi.dataType.doesNameExist(wrongDataTypeName)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(wrongDataTypeName);
  await umbracoUi.dataType.enterDataTypeName(dataTypeName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
  expect(await umbracoApi.dataType.doesNameExist(wrongDataTypeName)).toBeFalsy();
});

test('can delete a data type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createTextstringDataType(dataTypeName);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.deleteDataType(dataTypeName);

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeFalsy();
});

test('can change property editor in a data type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedEditorName = 'Text Area';
  const updatedEditorAlias = 'Umbraco.TextArea';
  const updatedEditorUiAlias = 'Umb.PropertyEditorUi.TextArea';

  await umbracoApi.dataType.createTextstringDataType(dataTypeName);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);
  await umbracoUi.dataType.clickChangeButton();
  await umbracoUi.dataType.selectAPropertyEditor(updatedEditorName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.editorAlias).toBe(updatedEditorAlias);
  expect(dataTypeData.editorUiAlias).toBe(updatedEditorUiAlias);
});

test('cannot create a data type without selecting the property editor', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.clickActionsMenuAtRoot();
  await umbracoUi.dataType.clickActionsMenuCreateButton();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(dataTypeName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isFailedStateButtonVisible();
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeFalsy();
});

test('can change settings', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maxCharsValue = 126;
  const expectedDataTypeValues = {
    "alias": "maxChars",
    "value": maxCharsValue
  };
  await umbracoApi.dataType.createTextstringDataType(dataTypeName);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);
  await umbracoUi.dataType.enterMaximumAllowedCharactersValue(maxCharsValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});
