import {NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
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
  // Arrange
  await umbracoUi.dataType.clickDataTypesMenu();

  // Act
  await umbracoUi.dataType.clickCreateActionWithOptionName('Data Type');
  await umbracoUi.dataType.enterDataTypeName(dataTypeName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor('Text Box');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
  // Check if the created data type is displayed in the collection view and has correct icon
  await umbracoUi.dataType.clickDataTypesMenu();
  await umbracoUi.dataType.doesCollectionTreeItemTableRowHaveName(dataTypeName);
  await umbracoUi.dataType.doesCollectionTreeItemTableRowHaveIcon(dataTypeName, 'icon-autofill');
});

test('can create a data type folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.dataType.clickDataTypesMenu();

  // Act
  await umbracoUi.dataType.clickCreateActionWithOptionName('Folder');
  await umbracoUi.dataType.enterFolderName(dataTypeFolderName);
  await umbracoUi.dataType.clickConfirmCreateFolderButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
  // Check if the created data type is displayed in the collection view and has correct icon
  await umbracoUi.dataType.clickDataTypesMenu();
  await umbracoUi.dataType.doesCollectionTreeItemTableRowHaveName(dataTypeFolderName);
  await umbracoUi.dataType.doesCollectionTreeItemTableRowHaveIcon(dataTypeFolderName, 'icon-folder');
});

test('can create a data type in a folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createFolder(dataTypeFolderName);
  await umbracoUi.dataType.goToDataType(dataTypeFolderName);

  // Act
  await umbracoUi.dataType.clickCreateActionWithOptionName('Data Type');
  await umbracoUi.dataType.enterDataTypeName(dataTypeName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor('Text Box');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
  // Check if the created data type is displayed in the collection view and has correct icon
  await umbracoUi.dataType.goToDataType(dataTypeFolderName);
  await umbracoUi.dataType.doesCollectionTreeItemTableRowHaveName(dataTypeName);
  await umbracoUi.dataType.doesCollectionTreeItemTableRowHaveIcon(dataTypeName, 'icon-autofill');
});

test('can create a data type folder in a folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'Test Child Folder';
  await umbracoApi.dataType.ensureNameNotExists(childFolderName);
  await umbracoApi.dataType.createFolder(dataTypeFolderName);
  await umbracoUi.dataType.goToDataType(dataTypeFolderName);

  // Act
  await umbracoUi.dataType.clickCreateActionWithOptionName('Folder');
  await umbracoUi.dataType.enterFolderName(childFolderName);
  await umbracoUi.dataType.clickConfirmCreateFolderButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.dataType.doesNameExist(childFolderName)).toBeTruthy();
  // Check if the created data type is displayed in the collection view and has correct icon
  await umbracoUi.dataType.doesCollectionTreeItemTableRowHaveName(childFolderName);
  await umbracoUi.dataType.doesCollectionTreeItemTableRowHaveIcon(childFolderName, 'icon-folder');
});