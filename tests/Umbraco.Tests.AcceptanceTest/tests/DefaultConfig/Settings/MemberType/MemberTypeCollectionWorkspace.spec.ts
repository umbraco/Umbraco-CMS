import {expect} from "@playwright/test";
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const memberTypeName = 'TestMemberType';
const memberTypeFolderName = 'TestMemberTypeFolder';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
  await umbracoApi.memberType.ensureNameNotExists(memberTypeFolderName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.memberType.goToSection(ConstantHelper.sections.settings);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
  await umbracoApi.memberType.ensureNameNotExists(memberTypeFolderName);
});

test('can create a member type using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.memberType.clickMemberTypesMenu();

  // Act
  await umbracoUi.memberType.clickCreateActionWithOptionName('Member Type');
  await umbracoUi.memberType.enterMemberTypeName(memberTypeName);
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeCreated();

  // Assert
  expect(await umbracoApi.memberType.doesNameExist(memberTypeName)).toBeTruthy();
  // Check if the created member type is displayed in the collection view and has correct icon
  await umbracoUi.memberType.clickMemberTypesMenu();
  await umbracoUi.memberType.doesCollectionTreeItemTableRowHaveName(memberTypeName);
  await umbracoUi.memberType.doesCollectionTreeItemTableRowHaveIcon(memberTypeName, 'icon-user');
  await umbracoUi.memberType.isMemberTypeTreeItemVisible(memberTypeName);
});

test('can create a member type folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.memberType.clickMemberTypesMenu();

  // Act
  await umbracoUi.memberType.clickCreateActionWithOptionName('Folder');
  await umbracoUi.memberType.enterFolderName(memberTypeFolderName);
  await umbracoUi.memberType.clickConfirmCreateFolderButtonAndWaitForMemberTypeToBeCreated();

  // Assert
  expect(await umbracoApi.memberType.doesNameExist(memberTypeFolderName)).toBeTruthy();
  // Check if the created folder is displayed in the collection view and has correct icon
  await umbracoUi.memberType.clickMemberTypesMenu();
  await umbracoUi.memberType.doesCollectionTreeItemTableRowHaveName(memberTypeFolderName);
  await umbracoUi.memberType.doesCollectionTreeItemTableRowHaveIcon(memberTypeFolderName, 'icon-folder');
});

test('can create a member type in a folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const parentFolderId = await umbracoApi.memberType.createFolder(memberTypeFolderName);
  await umbracoUi.memberType.goToMemberType(memberTypeFolderName);

  // Act
  await umbracoUi.memberType.clickCreateActionWithOptionName('Member Type');
  await umbracoUi.memberType.enterMemberTypeName(memberTypeName);
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeCreated();

  // Assert
  expect(await umbracoApi.memberType.doesNameExist(memberTypeName)).toBeTruthy();
  // Check if the created member type is displayed in the collection view and has correct icon
  await umbracoUi.memberType.goToMemberType(memberTypeFolderName);
  await umbracoUi.memberType.doesCollectionTreeItemTableRowHaveName(memberTypeName);
  await umbracoUi.memberType.doesCollectionTreeItemTableRowHaveIcon(memberTypeName, 'icon-user');
  // Verify the member type is inside the parent folder
  const parentFolderChildren = await umbracoApi.memberType.getChildren(parentFolderId);
  expect(parentFolderChildren[0].name).toBe(memberTypeName);
});

test('can create a member type folder in a folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'Test Child Folder';
  await umbracoApi.memberType.ensureNameNotExists(childFolderName);
  const parentFolderId = await umbracoApi.memberType.createFolder(memberTypeFolderName);
  await umbracoUi.memberType.goToMemberType(memberTypeFolderName);

  // Act
  await umbracoUi.memberType.clickCreateActionWithOptionName('Folder');
  await umbracoUi.memberType.enterFolderName(childFolderName);
  await umbracoUi.memberType.clickConfirmCreateFolderButtonAndWaitForMemberTypeToBeCreated();

  // Assert
  expect(await umbracoApi.memberType.doesNameExist(childFolderName)).toBeTruthy();
  // Check if the created folder is displayed in the collection view and has correct icon
  await umbracoUi.memberType.doesCollectionTreeItemTableRowHaveName(childFolderName);
  await umbracoUi.memberType.doesCollectionTreeItemTableRowHaveIcon(childFolderName, 'icon-folder');
  // Verify the child folder is inside the parent folder
  const parentFolderChildren = await umbracoApi.memberType.getChildren(parentFolderId);
  expect(parentFolderChildren[0].name).toBe(childFolderName);

  // Clean
  await umbracoApi.memberType.ensureNameNotExists(childFolderName);
});
