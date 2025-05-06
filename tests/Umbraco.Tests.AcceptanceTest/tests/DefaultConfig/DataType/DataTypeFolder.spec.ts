import {NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'TestDataType';
const dataTypeFolderName = 'TestDataTypeFolder';
const editorAlias = 'Umbraco.ColorPicker';
const editorUiAlias = 'Umb.PropertyEditorUi.ColorPicker';
const propertyEditorName = 'Color Picker';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.dataType.ensureNameNotExists(dataTypeFolderName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(dataTypeFolderName);
});

test('can create a data type folder', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.clickActionsMenuAtRoot();
  await umbracoUi.dataType.createDataTypeFolder(dataTypeFolderName);

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
});

test('can rename a data type folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongDataTypeFolderName = 'Wrong Folder';
  await umbracoApi.dataType.ensureNameNotExists(wrongDataTypeFolderName);
  await umbracoApi.dataType.createFolder(wrongDataTypeFolderName);
  expect(await umbracoApi.dataType.doesNameExist(wrongDataTypeFolderName)).toBeTruthy();

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.clickActionsMenuForDataType(wrongDataTypeFolderName);
  await umbracoUi.dataType.clickRenameFolderButton();
  await umbracoUi.dataType.enterFolderName(dataTypeFolderName);
  await umbracoUi.dataType.clickConfirmRenameButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
  expect(await umbracoApi.dataType.doesNameExist(wrongDataTypeFolderName)).toBeFalsy();
});

test('can delete a data type folder', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createFolder(dataTypeFolderName);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.deleteDataTypeFolder(dataTypeFolderName);

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesFolderExist(dataTypeFolderName)).toBeFalsy();
});

test('can create a data type in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  let dataTypeFolderId = await umbracoApi.dataType.createFolder(dataTypeFolderName);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.clickActionsMenuForDataType(dataTypeFolderName);
  await umbracoUi.dataType.clickActionsMenuCreateButton();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(dataTypeName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor(propertyEditorName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
  const dataTypeChildren = await umbracoApi.dataType.getChildren(dataTypeFolderId);
  expect(dataTypeChildren[0].name).toBe(dataTypeName);
  expect(dataTypeChildren[0].isFolder).toBeFalsy();
});

test('can create a folder in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'Child Folder';
  const dataTypeFolderId = await umbracoApi.dataType.createFolder(dataTypeFolderName);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
  await umbracoApi.dataType.ensureNameNotExists(childFolderName);

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.clickActionsMenuForDataType(dataTypeFolderName);
  await umbracoUi.dataType.createDataTypeFolder(childFolderName);

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesNameExist(childFolderName)).toBeTruthy();
  const dataTypeChildren = await umbracoApi.dataType.getChildren(dataTypeFolderId);
  expect(dataTypeChildren[0].name).toBe(childFolderName);
  expect(dataTypeChildren[0].isFolder).toBeTruthy();
});

test('can create a folder in a folder in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'ChildFolderName';
  const childOfChildFolderName = 'ChildOfChildFolderName';
  const dataTypeFolderId = await umbracoApi.dataType.createFolder(dataTypeFolderName);
  const childFolderId = await umbracoApi.dataType.createFolder(childFolderName, dataTypeFolderId);

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.clickCaretButtonForName(dataTypeFolderName);
  await umbracoUi.dataType.clickActionsMenuForDataType(childFolderName);
  await umbracoUi.dataType.createDataTypeFolder(childOfChildFolderName);

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesNameExist(childOfChildFolderName)).toBeTruthy();
  const childrenFolderData = await umbracoApi.dataType.getChildren(childFolderId);
  expect(childrenFolderData[0].name).toBe(childOfChildFolderName);
  expect(childrenFolderData[0].isFolder).toBeTruthy();
});

test('cannot delete a non-empty data type folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  let dataTypeFolderId = await umbracoApi.dataType.createFolder(dataTypeFolderName);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  await umbracoApi.dataType.create(dataTypeName, editorAlias, editorUiAlias, [], dataTypeFolderId);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
  await umbracoUi.reloadPage();

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.deleteDataTypeFolder(dataTypeFolderName);

  // Assert
  await umbracoUi.dataType.doesErrorNotificationHaveText(NotificationConstantHelper.error.notEmptyFolder);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
  expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
  const dataTypeChildren = await umbracoApi.dataType.getChildren(dataTypeFolderId);
  expect(dataTypeChildren[0].name).toBe(dataTypeName);
  expect(dataTypeChildren[0].isFolder).toBeFalsy();

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
});

test('can move a data type to a data type folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  const dataTypeId = await umbracoApi.dataType.create(dataTypeName, editorAlias, editorUiAlias,[]);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
  await umbracoApi.dataType.ensureNameNotExists(dataTypeFolderName);
  const dataTypeFolderId = await umbracoApi.dataType.createFolder(dataTypeFolderName);
  expect(await umbracoApi.dataType.doesFolderExist(dataTypeFolderId)).toBeTruthy();

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.clickActionsMenuForDataType(dataTypeName);
  await umbracoUi.dataType.moveDataTypeToFolder(dataTypeFolderName);

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.moved);
  const dataTypeInFolder = await umbracoApi.dataType.getChildren(dataTypeFolderId);
  expect(dataTypeInFolder[0].id).toEqual(dataTypeId);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
});

test('can duplicate a data type to a data type folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  await umbracoApi.dataType.create(dataTypeName, editorAlias, editorUiAlias, []);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
  await umbracoApi.dataType.ensureNameNotExists(dataTypeFolderName);
  const dataTypeFolderId = await umbracoApi.dataType.createFolder(dataTypeFolderName);
  expect(await umbracoApi.dataType.doesFolderExist(dataTypeFolderId)).toBeTruthy();

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.clickActionsMenuForDataType(dataTypeName);
  await umbracoUi.dataType.duplicateDataTypeToFolder(dataTypeFolderName);

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.duplicated);
  const dataTypeInFolder = await umbracoApi.dataType.getChildren(dataTypeFolderId);
  expect(dataTypeInFolder[0].name).toEqual(dataTypeName + ' (copy)');
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
});
