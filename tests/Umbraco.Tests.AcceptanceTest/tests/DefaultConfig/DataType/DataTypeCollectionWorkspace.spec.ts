import {test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const dataTypeName = 'TestDataType';
const dataTypeFolderName = 'TestDataTypeFolder';
const childFolderName = 'Test Child Folder';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeFolderName);
  await umbracoApi.dataType.ensureNameNotExists(childFolderName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeFolderName);
  await umbracoApi.dataType.ensureNameNotExists(childFolderName);
});

test('can create a data type from the tree actions menu', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.clickActionsMenuAtRoot();
  await umbracoUi.dataType.clickCreateActionMenuOption();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(dataTypeName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor('Text Box');
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeCreated();

  // Assert
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
  await umbracoUi.dataType.reloadDataTypeTree();
  await umbracoUi.dataType.isDataTypeTreeItemVisible(dataTypeName);
  await umbracoUi.dataType.doesTreeItemHaveTheCorrectIcon(dataTypeName, 'icon-autofill');
});

test('can create a data type folder from the tree actions menu', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.clickActionsMenuAtRoot();
  await umbracoUi.dataType.createDataTypeFolderAndWaitForDataTypeToBeCreated(dataTypeFolderName);

  // Assert
  expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
  await umbracoUi.dataType.isDataTypeTreeItemVisible(dataTypeFolderName);
  await umbracoUi.dataType.doesTreeItemHaveTheCorrectIcon(dataTypeFolderName, 'icon-folder');
});

test('can create a data type in a folder from the tree actions menu', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeFolderId = await umbracoApi.dataType.createFolder(dataTypeFolderName);
  await umbracoUi.dataType.reloadDataTypeTree();

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.clickActionsMenuForDataType(dataTypeFolderName);
  await umbracoUi.dataType.clickCreateActionMenuOption();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(dataTypeName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor('Text Box');
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeCreated();

  // Assert
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
  await expect.poll(async () => (await umbracoApi.dataType.getChildren(dataTypeFolderId))[0]?.name).toBe(dataTypeName);
  await umbracoUi.dataType.goToDataType(dataTypeFolderName);
  await umbracoUi.dataType.doesTreeItemHaveTheCorrectIcon(dataTypeName, 'icon-autofill');
});

test('can create a data type folder in a folder from the tree actions menu', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeFolderId = await umbracoApi.dataType.createFolder(dataTypeFolderName);
  await umbracoUi.dataType.reloadDataTypeTree();

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.clickActionsMenuForDataType(dataTypeFolderName);
  await umbracoUi.dataType.createDataTypeFolderAndWaitForDataTypeToBeCreated(childFolderName);

  // Assert
  expect(await umbracoApi.dataType.doesNameExist(childFolderName)).toBeTruthy();
  await expect.poll(async () => {
    const [child] = await umbracoApi.dataType.getChildren(dataTypeFolderId);
    return {name: child?.name, isFolder: child?.isFolder};
  }).toEqual({name: childFolderName, isFolder: true});
  await umbracoUi.dataType.openCaretButtonForName(dataTypeFolderName);
  await umbracoUi.dataType.doesTreeItemHaveTheCorrectIcon(childFolderName, 'icon-folder');
});
