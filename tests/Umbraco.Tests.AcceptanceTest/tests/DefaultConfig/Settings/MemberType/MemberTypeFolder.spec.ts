import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const memberTypeFolderName = 'TestMemberTypeFolder';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.memberType.ensureNameNotExists(memberTypeFolderName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.memberType.goToSection(ConstantHelper.sections.settings);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.memberType.ensureNameNotExists(memberTypeFolderName);
});

test('can create a empty member type folder', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.memberType.clickActionsMenuAtRoot();
  await umbracoUi.memberType.clickCreateActionMenuOption();
  await umbracoUi.memberType.clickFolderButton();
  await umbracoUi.memberType.enterFolderName(memberTypeFolderName);
  await umbracoUi.memberType.clickConfirmCreateFolderButtonAndWaitForMemberTypeToBeCreated();

  // Assert
  const folder = await umbracoApi.memberType.getByName(memberTypeFolderName);
  expect(folder.name).toBe(memberTypeFolderName);
  await umbracoUi.memberType.isMemberTypeTreeItemVisible(memberTypeFolderName, true);
});

test('can delete a member type folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.memberType.createFolder(memberTypeFolderName);

  // Act
  await umbracoUi.memberType.clickRootFolderCaretButton();
  await umbracoUi.memberType.clickActionsMenuForName(memberTypeFolderName);
  await umbracoUi.memberType.clickDeleteAndConfirmButtonAndWaitForMemberTypeToBeDeleted();

  // Assert
  expect(await umbracoApi.memberType.doesNameExist(memberTypeFolderName)).toBeFalsy();
  await umbracoUi.memberType.isMemberTypeTreeItemVisible(memberTypeFolderName, false);
});

test('can rename a member type folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const oldFolderName = 'OldName';
  await umbracoApi.memberType.ensureNameNotExists(oldFolderName);
  await umbracoApi.memberType.createFolder(oldFolderName);

  // Act
  await umbracoUi.memberType.clickRootFolderCaretButton();
  await umbracoUi.memberType.clickActionsMenuForName(oldFolderName);
  await umbracoUi.memberType.clickUpdateActionMenuOption();
  await umbracoUi.memberType.enterFolderName(memberTypeFolderName);
  await umbracoUi.memberType.clickConfirmRenameButtonAndWaitForMemberTypeToBeRenamed();

  // Assert
  const folderData = await umbracoApi.memberType.getByName(memberTypeFolderName);
  expect(folderData.name).toBe(memberTypeFolderName);
  await umbracoUi.memberType.isMemberTypeTreeItemVisible(oldFolderName, false);
  await umbracoUi.memberType.isMemberTypeTreeItemVisible(memberTypeFolderName, true);
});

test('can create a member type folder in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'ChildFolder';
  await umbracoApi.memberType.ensureNameNotExists(childFolderName);
  const parentFolderId = await umbracoApi.memberType.createFolder(memberTypeFolderName);

  // Act
  await umbracoUi.memberType.clickRootFolderCaretButton();
  await umbracoUi.memberType.clickActionsMenuForName(memberTypeFolderName);
  await umbracoUi.memberType.clickCreateActionMenuOption();
  await umbracoUi.memberType.clickFolderButton();
  await umbracoUi.memberType.enterFolderName(childFolderName);
  await umbracoUi.memberType.clickConfirmCreateFolderButtonAndWaitForMemberTypeToBeCreated();

  // Assert
  await umbracoUi.memberType.openCaretButtonForName(memberTypeFolderName);
  await umbracoUi.memberType.isTreeItemVisible(childFolderName, true);
  const parentFolderChildren = await umbracoApi.memberType.getChildren(parentFolderId);
  expect(parentFolderChildren[0].name).toBe(childFolderName);

  // Clean
  await umbracoApi.memberType.ensureNameNotExists(childFolderName);
});

test('can not delete a member type folder with child items', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'ChildFolder';
  await umbracoApi.memberType.ensureNameNotExists(childFolderName);
  const parentFolderId = await umbracoApi.memberType.createFolder(memberTypeFolderName);
  await umbracoApi.memberType.createFolder(childFolderName, parentFolderId);

  // Act
  await umbracoUi.memberType.clickRootFolderCaretButton();
  await umbracoUi.memberType.clickActionsMenuForName(memberTypeFolderName);
  await umbracoUi.memberType.clickDeleteAndConfirmButton();

  // Assert
  await umbracoUi.memberType.isErrorNotificationVisible();
  expect(await umbracoApi.memberType.doesNameExist(memberTypeFolderName)).toBeTruthy();
  expect(await umbracoApi.memberType.doesNameExist(childFolderName)).toBeTruthy();

  // Clean
  await umbracoApi.memberType.ensureNameNotExists(childFolderName);
});

test('can create a member type folder in a folder in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const grandparentFolderName = 'GrandparentFolder';
  const childFolderName = 'ChildFolder';
  await umbracoApi.memberType.ensureNameNotExists(childFolderName);
  await umbracoApi.memberType.ensureNameNotExists(grandparentFolderName);
  const grandParentFolderId = await umbracoApi.memberType.createFolder(grandparentFolderName);
  const parentFolderId = await umbracoApi.memberType.createFolder(memberTypeFolderName, grandParentFolderId);

  // Act
  await umbracoUi.memberType.clickRootFolderCaretButton();
  await umbracoUi.memberType.openCaretButtonForName(grandparentFolderName);
  await umbracoUi.memberType.clickActionsMenuForName(memberTypeFolderName);
  await umbracoUi.memberType.clickCreateActionMenuOption();
  await umbracoUi.memberType.clickFolderButton();
  await umbracoUi.memberType.enterFolderName(childFolderName);
  await umbracoUi.memberType.clickConfirmCreateFolderButtonAndWaitForMemberTypeToBeCreated();

  // Assert
  await umbracoUi.memberType.openCaretButtonForName(memberTypeFolderName);
  await umbracoUi.memberType.isTreeItemVisible(childFolderName, true);
  const grandParentFolderChildren = await umbracoApi.memberType.getChildren(grandParentFolderId);
  expect(grandParentFolderChildren[0].name).toBe(memberTypeFolderName);
  const parentFolderChildren = await umbracoApi.memberType.getChildren(parentFolderId);
  expect(parentFolderChildren[0].name).toBe(childFolderName);

  // Clean
  await umbracoApi.memberType.ensureNameNotExists(childFolderName);
  await umbracoApi.memberType.ensureNameNotExists(grandparentFolderName);
});
