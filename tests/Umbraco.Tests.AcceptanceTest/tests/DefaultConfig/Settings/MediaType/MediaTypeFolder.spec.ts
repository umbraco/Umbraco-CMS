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
  await umbracoUi.mediaType.clickActionsMenuAtRoot();
  await umbracoUi.mediaType.clickCreateActionMenuOption();
  await umbracoUi.mediaType.clickFolderButton();
  await umbracoUi.mediaType.enterFolderName(mediaTypeFolderName);
  await umbracoUi.mediaType.clickConfirmCreateFolderButton();

  // Assert
  await umbracoUi.mediaType.waitForMediaTypeToBeCreated();
  const folder = await umbracoApi.mediaType.getByName(mediaTypeFolderName);
  expect(folder.name).toBe(mediaTypeFolderName);
  // Checks if the folder is in the root
  await umbracoUi.mediaType.isMediaTypeTreeItemVisible(mediaTypeFolderName, true);
});

test('can delete a media type folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.mediaType.createFolder(mediaTypeFolderName);

  // Act
  await umbracoUi.mediaType.clickRootFolderCaretButton();
  await umbracoUi.mediaType.clickActionsMenuForName(mediaTypeFolderName);
  await umbracoUi.mediaType.clickDeleteAndConfirmButton();

  // Assert
  await umbracoUi.mediaType.waitForMediaTypeToBeDeleted();
  expect(await umbracoApi.mediaType.doesNameExist(mediaTypeFolderName)).toBeFalsy();
  await umbracoUi.mediaType.isMediaTypeTreeItemVisible(mediaTypeFolderName, false);
});

test('can rename a media type folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const oldFolderName = 'OldName';
  await umbracoApi.mediaType.ensureNameNotExists(oldFolderName);
  await umbracoApi.mediaType.createFolder(oldFolderName);

  // Act
  await umbracoUi.mediaType.clickRootFolderCaretButton();
  await umbracoUi.mediaType.clickActionsMenuForName(oldFolderName);
  await umbracoUi.mediaType.clickUpdateActionMenuOption();
  await umbracoUi.waitForTimeout(500);
  await umbracoUi.mediaType.enterFolderName(mediaTypeFolderName);
  await umbracoUi.mediaType.clickConfirmRenameButton();

  // Assert
  await umbracoUi.mediaType.waitForMediaTypeToBeRenamed();
  const folder = await umbracoApi.mediaType.getByName(mediaTypeFolderName);
  expect(folder.name).toBe(mediaTypeFolderName);
  await umbracoUi.mediaType.isMediaTypeTreeItemVisible(oldFolderName, false);
  await umbracoUi.mediaType.isMediaTypeTreeItemVisible(mediaTypeFolderName, true);
});

test('can create a media type folder in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'ChildFolder';
  await umbracoApi.mediaType.ensureNameNotExists(childFolderName);
  const parentFolderId = await umbracoApi.mediaType.createFolder(mediaTypeFolderName);

  // Act
  await umbracoUi.mediaType.clickRootFolderCaretButton();
  await umbracoUi.mediaType.clickActionsMenuForName(mediaTypeFolderName);
  await umbracoUi.mediaType.clickCreateActionMenuOption();
  await umbracoUi.mediaType.clickFolderButton();
  await umbracoUi.mediaType.enterFolderName(childFolderName);
  await umbracoUi.mediaType.clickConfirmCreateFolderButton();

  // Assert
  await umbracoUi.mediaType.waitForMediaTypeToBeCreated();
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
  await umbracoUi.mediaType.clickCreateActionMenuOption();
  await umbracoUi.mediaType.clickFolderButton();
  await umbracoUi.mediaType.enterFolderName(childFolderName);
  await umbracoUi.mediaType.clickConfirmCreateFolderButton();

  // Assert
  await umbracoUi.mediaType.waitForMediaTypeToBeCreated();
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
