import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Element Type
const elementTypeName = 'TestElementTypeForFolderPermissions';
let elementTypeId = '';
// Element
const elementName = 'TestElementForFolderPermissions';
const newElementName = 'NewTestElementForFolderPermissions';
let elementId = '';
// Folder
const folderName = 'TestElementFolderForPermissions';
const newFolderName = 'RenamedElementFolderForPermissions';
const targetFolderName = 'TargetElementFolderForPermissions';
const createdFolderName = 'CreatedElementFolderForPermissions';
let folderId = '';
let targetFolderId = '';
// Data Type
const dataTypeName = 'Textstring';
const elementText = 'This is test element text';
// User
const testUser = ConstantHelper.testUserCredentials;
// User Group
const userGroupName = 'TestUserGroup';
let userGroupId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(createdFolderName);
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', dataTypeName, dataType.id);
  folderId = await umbracoApi.element.createDefaultElementFolder(folderName);
  elementId = await umbracoApi.element.createElementWithTextContent(elementName, elementTypeId, elementText, dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser();
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.element.ensureNameNotExists(newElementName);
  await umbracoApi.element.ensureNameNotExists(folderName);
  await umbracoApi.element.ensureNameNotExists(newFolderName);
  await umbracoApi.element.ensureNameNotExists(targetFolderName);
  await umbracoApi.element.ensureNameNotExists(createdFolderName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.element.emptyRecycleBin();
});

test('can see Element Folder permissions panel with all toggles in the user group workspace', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.users);
  await umbracoUi.userGroup.clickUserGroupsButton();

  // Act
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Assert
  await umbracoUi.userGroup.doesElementFolderPermissionsSettingsHaveValue(ConstantHelper.userGroupElementFolderPermissionsSettings);
});

test('can see an element folder with read permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithReadElementFolderPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isElementInTreeVisible(folderName, true);
});

test('cannot see an element folder with read permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithReadElementFolderPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isElementInTreeVisible(folderName, false);
});

test('can create an element folder with create permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCreateElementFolderPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickActionsMenuAtRoot();
  await umbracoUi.library.clickCreateActionMenuOption();
  await umbracoUi.library.chooseElementType('Folder');
  await umbracoUi.library.enterFolderName(createdFolderName);
  await umbracoUi.library.clickCreateFolderButtonAndWaitForElementFolderToBeCreated();

  // Assert
  expect(await umbracoApi.element.doesNameExist(createdFolderName)).toBeTruthy();
});

test('cannot create an element folder with create permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCreateElementFolderPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isActionsMenuForNameVisible(folderName, false);
});

test('can rename an element folder with update permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdateElementFolderPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickActionsMenuForElement(folderName);
  await umbracoUi.library.clickRenameActionMenuOption();
  await umbracoUi.library.enterFolderName(newFolderName);
  await umbracoUi.library.clickConfirmRenameFolderButtonAndWaitForElementFolderToBeRenamed();

  // Assert
  expect(await umbracoApi.element.doesNameExist(folderName)).toBeFalsy();
  expect(await umbracoApi.element.doesNameExist(newFolderName)).toBeTruthy();
});

test('cannot rename an element folder with update permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdateElementFolderPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isActionsMenuForNameVisible(folderName, false);
});

test('can delete an element folder with delete permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeleteElementFolderPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickActionsMenuForElement(folderName);
  await umbracoUi.library.clickTrashActionMenuOption();
  await umbracoUi.library.clickConfirmTrashButtonAndWaitForElementFolderToBeTrashed();

  // Assert
  expect(await umbracoApi.element.doesNameExist(folderName)).toBeFalsy();
  await umbracoUi.library.isElementInTreeVisible(folderName, false);
});

test('cannot delete an element folder with delete permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeleteElementFolderPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isActionsMenuForNameVisible(folderName, false);
});

test('can move an element folder with move permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  targetFolderId = await umbracoApi.element.createDefaultElementFolder(targetFolderName);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithMoveElementFolderPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickActionsMenuForElement(folderName);
  await umbracoUi.library.clickMoveToActionMenuOption();
  await umbracoUi.library.moveToElementWithName([], targetFolderName);

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.moved);
  const targetFolderChildren = await umbracoApi.element.getChildren(targetFolderId);
  expect(targetFolderChildren.some((child) => child.name === folderName)).toBeTruthy();
});

test('cannot move an element folder with move permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithMoveElementFolderPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isActionsMenuForNameVisible(folderName, false);
});

test('can see recycle bin actions menu for a trashed folder when delete folder permission is enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.moveToRecycleBin(folderId, true);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeleteElementFolderPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isActionsMenuForRecycleBinVisible(true);
});

test('cannot delete a trashed folder from the recycle bin when delete folder permission is disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.moveToRecycleBin(folderId, true);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeleteElementFolderPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isActionsMenuForRecycleBinVisible(false);
});

test('cannot create an element folder without create folder permission via API', async ({umbracoApi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCreateElementFolderPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);

  // Act
  const response = await umbracoApi.element.createFolderResponse(createdFolderName);

  // Assert
  expect(response.status()).toBe(ConstantHelper.statusCodes.forbidden);
  await umbracoApi.loginToAdminUser();
  expect(await umbracoApi.element.doesNameExist(createdFolderName)).toBeFalsy();
});

test('cannot rename an element folder without update folder permission via API', async ({umbracoApi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdateElementFolderPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);

  // Act
  const response = await umbracoApi.element.renameFolder(folderId, newFolderName);

  // Assert
  expect(response.status()).toBe(ConstantHelper.statusCodes.forbidden);
});

test('cannot delete an element folder without delete folder permission via API', async ({umbracoApi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeleteElementFolderPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);

  // Act
  const response = await umbracoApi.element.deleteFolder(folderId);

  // Assert
  expect(response.status()).toBe(ConstantHelper.statusCodes.forbidden);
});

test('cannot move an element folder without move folder permission via API', async ({umbracoApi}) => {
  // Arrange
  targetFolderId = await umbracoApi.element.createDefaultElementFolder(targetFolderName);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithMoveElementFolderPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);

  // Act
  const response = await umbracoApi.element.moveFolder(folderId, targetFolderId);

  // Assert
  expect(response.status()).toBe(ConstantHelper.statusCodes.forbidden);
});
