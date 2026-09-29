import {test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const dataTypeName = 'TestDataType';
const dataTypeFolderName = 'TestDataTypeFolder';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeFolderName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeFolderName);
});

test('can create a data type using create options', async ({umbracoApi, umbracoUi}) => {
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

test('can create a data type folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.clickActionsMenuAtRoot();
  await umbracoUi.dataType.createDataTypeFolderAndWaitForDataTypeToBeCreated(dataTypeFolderName);

  // Assert
  expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
  await umbracoUi.dataType.isDataTypeTreeItemVisible(dataTypeFolderName);
  await umbracoUi.dataType.doesTreeItemHaveTheCorrectIcon(dataTypeFolderName, 'icon-folder');
});

test('can create a data type in a folder using create options', async ({umbracoApi, umbracoUi}) => {
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
  const dataTypeFolderChildren = await umbracoApi.dataType.getChildren(dataTypeFolderId);
  expect(dataTypeFolderChildren[0].name).toBe(dataTypeName);
  await umbracoUi.dataType.goToDataType(dataTypeFolderName);
  await umbracoUi.dataType.doesTreeItemHaveTheCorrectIcon(dataTypeName, 'icon-autofill');
});

test('can create a data type folder in a folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'Test Child Folder';
  await umbracoApi.dataType.ensureNameNotExists(childFolderName);
  const dataTypeFolderId = await umbracoApi.dataType.createFolder(dataTypeFolderName);
  await umbracoUi.dataType.reloadDataTypeTree();

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.clickActionsMenuForDataType(dataTypeFolderName);
  await umbracoUi.dataType.createDataTypeFolderAndWaitForDataTypeToBeCreated(childFolderName);

  // Assert
  expect(await umbracoApi.dataType.doesNameExist(childFolderName)).toBeTruthy();
  const dataTypeFolderChildren = await umbracoApi.dataType.getChildren(dataTypeFolderId);
  expect(dataTypeFolderChildren[0].name).toBe(childFolderName);
  expect(dataTypeFolderChildren[0].isFolder).toBeTruthy();
  await umbracoUi.dataType.openCaretButtonForName(dataTypeFolderName);
  await umbracoUi.dataType.doesTreeItemHaveTheCorrectIcon(childFolderName, 'icon-folder');
});
