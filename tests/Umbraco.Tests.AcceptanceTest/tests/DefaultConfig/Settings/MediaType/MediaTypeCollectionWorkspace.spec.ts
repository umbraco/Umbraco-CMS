import {expect} from "@playwright/test";
import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';

const mediaTypeName = 'TestMediaType';
const mediaTypeFolderName = 'TestMediaTypeFolder';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeFolderName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeFolderName);
});

test('can create a media type using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.mediaType.clickMediaTypesMenu();

  // Act
  await umbracoUi.mediaType.clickCreateActionWithOptionName('Media Type');
  await umbracoUi.mediaType.enterMediaTypeName(mediaTypeName);
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  expect(await umbracoApi.mediaType.doesNameExist(mediaTypeName)).toBeTruthy();
  // Check if the created media type is displayed in the collection view and has correct icon
  await umbracoUi.mediaType.clickMediaTypesMenu();
  await umbracoUi.mediaType.doesCollectionTreeItemTableRowHaveName(mediaTypeName);
  await umbracoUi.mediaType.doesCollectionTreeItemTableRowHaveIcon(mediaTypeName, 'icon-picture');
});

test('can create a media type folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.mediaType.clickMediaTypesMenu();

  // Act
  await umbracoUi.mediaType.clickCreateActionWithOptionName('Folder');
  await umbracoUi.mediaType.enterFolderName(mediaTypeFolderName);
  await umbracoUi.mediaType.clickConfirmCreateFolderButton();

  // Assert
  //await umbracoUi.mediaType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  expect(await umbracoApi.mediaType.doesNameExist(mediaTypeFolderName)).toBeTruthy();
  // Check if the created media type is displayed in the collection view and has correct icon
  await umbracoUi.mediaType.clickMediaTypesMenu();
  await umbracoUi.mediaType.doesCollectionTreeItemTableRowHaveName(mediaTypeFolderName);
  await umbracoUi.mediaType.doesCollectionTreeItemTableRowHaveIcon(mediaTypeFolderName, 'icon-folder');
});

test('can create a media type in a folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.mediaType.createFolder(mediaTypeFolderName);
  await umbracoUi.mediaType.goToMediaType(mediaTypeFolderName);

  // Act
  await umbracoUi.mediaType.clickCreateActionWithOptionName('Media Type');
  await umbracoUi.mediaType.enterMediaTypeName(mediaTypeName);
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  expect(await umbracoApi.mediaType.doesNameExist(mediaTypeName)).toBeTruthy();
  // Check if the created media type is displayed in the collection view and has correct icon
  await umbracoUi.mediaType.goToMediaType(mediaTypeFolderName);
  await umbracoUi.mediaType.doesCollectionTreeItemTableRowHaveName(mediaTypeName);
  await umbracoUi.mediaType.doesCollectionTreeItemTableRowHaveIcon(mediaTypeName, 'icon-picture');
});

test('can create a media type folder in a folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'Test Child Folder';
  await umbracoApi.mediaType.ensureNameNotExists(childFolderName);
  await umbracoApi.mediaType.createFolder(mediaTypeFolderName);
  await umbracoUi.mediaType.goToMediaType(mediaTypeFolderName);

  // Act
  await umbracoUi.mediaType.clickCreateActionWithOptionName('Folder');
  await umbracoUi.mediaType.enterFolderName(childFolderName);
  await umbracoUi.mediaType.clickConfirmCreateFolderButton();

  // Assert
  //await umbracoUi.mediaType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  expect(await umbracoApi.mediaType.doesNameExist(childFolderName)).toBeTruthy();
  // Check if the created media type is displayed in the collection view and has correct icon
  await umbracoUi.mediaType.doesCollectionTreeItemTableRowHaveName(childFolderName);
  await umbracoUi.mediaType.doesCollectionTreeItemTableRowHaveIcon(childFolderName, 'icon-folder');

  // Clean
  await umbracoApi.mediaType.ensureNameNotExists(childFolderName);
});

