import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const mediaTypeFolderName = 'TestMediaTypeFolder';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeFolderName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeFolderName);
});

test('can create a empty media type folder', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.mediaType.clickActionsMenuForName('Media Types');
  await umbracoUi.mediaType.clickActionsMenuCreateButton();
  await umbracoUi.mediaType.clickFolderButton();
  await umbracoUi.mediaType.enterFolderName(mediaTypeFolderName);
  await umbracoUi.mediaType.clickConfirmCreateFolderButton();

  // Assert
  await umbracoUi.mediaType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  const folder = await umbracoApi.mediaType.getByName(mediaTypeFolderName);
  expect(folder.name).toBe(mediaTypeFolderName);
  // Checks if the folder is in the root
  await umbracoUi.mediaType.clickRootFolderCaretButton();
  await umbracoUi.mediaType.isTreeItemVisible(mediaTypeFolderName, true);
});

test('can delete a media type folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.mediaType.createFolder(mediaTypeFolderName);

  // Act
  await umbracoUi.mediaType.clickRootFolderCaretButton();
  await umbracoUi.mediaType.clickActionsMenuForName(mediaTypeFolderName);
  await umbracoUi.mediaType.deleteFolder();

  // Assert
  await umbracoUi.mediaType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted);
  expect(await umbracoApi.mediaType.doesNameExist(mediaTypeFolderName)).toBeFalsy();
});

test('can rename a media type folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const oldFolderName = 'OldName';
  await umbracoApi.mediaType.ensureNameNotExists(oldFolderName);
  await umbracoApi.mediaType.createFolder(oldFolderName);

  // Act
  await umbracoUi.mediaType.clickRootFolderCaretButton();
  await umbracoUi.mediaType.clickActionsMenuForName(oldFolderName);
  await umbracoUi.mediaType.clickRenameFolderButton();
  await umbracoUi.mediaType.enterFolderName(mediaTypeFolderName);
  await umbracoUi.mediaType.clickConfirmRenameFolderButton();

  // Assert
  await umbracoUi.mediaType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  const folder = await umbracoApi.mediaType.getByName(mediaTypeFolderName);
  expect(folder.name).toBe(mediaTypeFolderName);
});

test('can create a media type folder in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'ChildFolder';
  await umbracoApi.mediaType.ensureNameNotExists(childFolderName);
  const parentFolderId = await umbracoApi.mediaType.createFolder(mediaTypeFolderName);

  // Act
  await umbracoUi.mediaType.clickRootFolderCaretButton();
  await umbracoUi.mediaType.clickActionsMenuForName(mediaTypeFolderName);
  await umbracoUi.mediaType.clickActionsMenuCreateButton();
  await umbracoUi.mediaType.clickFolderButton();
  await umbracoUi.mediaType.enterFolderName(childFolderName);
  await umbracoUi.mediaType.clickConfirmCreateFolderButton();

  // Assert
  await umbracoUi.mediaType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.mediaType.clickCaretButtonForName(mediaTypeFolderName);
  await umbracoUi.mediaType.isTreeItemVisible(childFolderName, true);
  const parentFolderChildren = await umbracoApi.mediaType.getChildren(parentFolderId);
  expect(parentFolderChildren[0].name).toBe(childFolderName);

  // Clean
  await umbracoApi.mediaType.ensureNameNotExists(childFolderName);
});

test('can create a media type folder in a folder in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const grandparentFolderName = 'GrandparentFolder';
  const childFolderName = 'ChildFolder';
  await umbracoApi.mediaType.ensureNameNotExists(childFolderName);
  await umbracoApi.mediaType.ensureNameNotExists(grandparentFolderName);
  const grandParentFolderId = await umbracoApi.mediaType.createFolder(grandparentFolderName);
  const parentFolderId = await umbracoApi.mediaType.createFolder(mediaTypeFolderName, grandParentFolderId);

  // Act
  await umbracoUi.mediaType.clickRootFolderCaretButton();
  await umbracoUi.mediaType.clickCaretButtonForName(grandparentFolderName);
  await umbracoUi.mediaType.clickActionsMenuForName(mediaTypeFolderName);
  await umbracoUi.mediaType.clickActionsMenuCreateButton();
  await umbracoUi.mediaType.clickFolderButton();
  await umbracoUi.mediaType.enterFolderName(childFolderName);
  await umbracoUi.mediaType.clickConfirmCreateFolderButton();

  // Assert
  await umbracoUi.mediaType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.mediaType.clickCaretButtonForName(mediaTypeFolderName);
  await umbracoUi.mediaType.isTreeItemVisible(childFolderName, true);
  const grandParentFolderChildren = await umbracoApi.mediaType.getChildren(grandParentFolderId);
  expect(grandParentFolderChildren[0].name).toBe(mediaTypeFolderName);
  const parentFolderChildren = await umbracoApi.mediaType.getChildren(parentFolderId);
  expect(parentFolderChildren[0].name).toBe(childFolderName);

  // Clean
  await umbracoApi.mediaType.ensureNameNotExists(childFolderName);
  await umbracoApi.mediaType.ensureNameNotExists(grandparentFolderName);
});
