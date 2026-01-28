import {ConstantHelper, NotificationConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

// Element Type
const elementTypeName = 'TestElementTypeForPermissions';
let elementTypeId = '';

// Element
const elementName = 'TestElement';
const newElementName = 'NewTestElement';
let elementId = '';

// Data Type
const dataTypeName = 'Textstring';
const elementText = 'This is test element text';

// User
const testUser = ConstantHelper.testUserCredentials;
let testUserCookieAndToken = {cookie: "", accessToken: "", refreshToken: ""};

// User Group
const userGroupName = 'TestUserGroup';
let userGroupId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.element.ensureNameNotExists(newElementName);
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', dataTypeName, dataType.id);
  elementId = await umbracoApi.element.createElementWithTextContent(elementName, elementTypeId, elementText, dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser(testUserCookieAndToken.cookie, testUserCookieAndToken.accessToken, testUserCookieAndToken.refreshToken);
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.element.ensureNameNotExists(newElementName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.element.emptyRecycleBin();
});

test('can read element with permission enabled', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithReadElementPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.goToElementWithName(elementName);

  // Assert
  await umbracoUi.library.doesElementHaveName(elementName);
});

test('can not see element in tree with read permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithReadElementPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isElementInTreeVisible(elementName, false);
});

test('access denied when accessing element by direct link with read permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithReadElementPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToElementById(elementId);

  // Assert
  await umbracoUi.library.doesElementWorkspaceHaveText('Access denied');
});

test('can create element with create permission enabled', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCreateElementPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickActionsMenuAtRoot();
  await umbracoUi.library.clickCreateActionMenuOption();
  await umbracoUi.library.clickElementButton();
  await umbracoUi.library.clickModalMenuItemWithName(elementTypeName);
  await umbracoUi.library.clickChooseModalButton();
  await umbracoUi.library.enterElementName(newElementName);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeCreated();

  // Assert
  expect(await umbracoApi.element.doesNameExist(newElementName)).toBeTruthy();
  await umbracoUi.library.isElementReadOnly(true);
});

test('can not see create action menu with create permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCreateElementPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isActionsMenuForNameVisible(elementName, false);
});

test('can delete element with delete permission enabled', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeleteElementPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickActionsMenuForElement(elementName);
  await umbracoUi.library.clickTrashActionMenuOption();
  await umbracoUi.library.clickConfirmTrashButtonAndWaitForElementToBeTrashed();

  // Assert
  await umbracoUi.library.isItemVisibleInRecycleBin(elementName);
});

test('can not delete element with delete permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeleteElementPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isActionsMenuForNameVisible(elementName, false);
});

test('can empty recycle bin with delete permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.moveToRecycleBin(elementId);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeleteElementPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickRecycleBinButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.library.clickEmptyRecycleBinButton();
  await umbracoUi.library.clickConfirmEmptyRecycleBinButtonAndWaitForRecycleBinToBeEmptied();

  // Assert
  await umbracoUi.library.isItemVisibleInRecycleBin(elementName, false, false);
});

test('can not empty recycle bin with delete permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.moveToRecycleBin(elementId);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeleteElementPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isActionsMenuForRecycleBinVisible(false);
});

test('can publish element with publish permission enabled', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPublishElementPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickActionsMenuForElement(elementName);
  await umbracoUi.library.clickPublishActionMenuOption();
  await umbracoUi.library.clickConfirmToPublishButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  expect(await umbracoApi.element.isElementPublished(elementId)).toBeTruthy();
});

test('can not publish element with publish permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPublishElementPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isActionsMenuForNameVisible(elementName, false);
});

test('can unpublish element with unpublish permission enabled', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.publish(elementId);
  expect(await umbracoApi.element.isElementPublished(elementId)).toBeTruthy();
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUnpublishElementPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickActionsMenuForElement(elementName);
  await umbracoUi.library.clickUnpublishActionMenuOption();
  await umbracoUi.library.clickConfirmToUnpublishButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.unpublished);
  expect(await umbracoApi.element.isElementPublished(elementId)).toBeFalsy();
});

test('can not unpublish element with unpublish permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.publish(elementId);
  expect(await umbracoApi.element.isElementPublished(elementId)).toBeTruthy();
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUnpublishElementPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isActionsMenuForNameVisible(elementName, false);
});

test('can update element with update permission enabled', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdateElementPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.goToElementWithName(elementName);
  await umbracoUi.library.isElementReadOnly(false);
  await umbracoUi.library.enterElementName(newElementName);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeUpdated();

  // Assert
  expect(await umbracoApi.element.doesNameExist(newElementName)).toBeTruthy();
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeFalsy();
});

test('can not update element with update permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdateElementPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);
  await umbracoUi.library.goToElementWithName(elementName);

  // Assert
  await umbracoUi.library.isActionsMenuForNameVisible(elementName, false);
  await umbracoUi.library.isElementReadOnly(true);
});

test('can duplicate element with duplicate permission enabled', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const duplicatedElementName = elementName + ' (1)';
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDuplicateElementPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickActionsMenuForElement(elementName);
  // Duplicate to root
  await umbracoUi.library.clickDuplicateToActionMenuOption();
  await umbracoUi.library.clickLabelWithName('Elements');
  await umbracoUi.library.clickDuplicateButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.duplicated);
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeTruthy();
  expect(await umbracoApi.element.doesNameExist(duplicatedElementName)).toBeTruthy();
  await umbracoUi.library.isElementInTreeVisible(elementName);
  await umbracoUi.library.isElementInTreeVisible(duplicatedElementName);
  const originalElement = await umbracoApi.element.getByName(elementName);
  const duplicatedElement = await umbracoApi.element.getByName(duplicatedElementName);
  expect(originalElement.values[0].value).toEqual(duplicatedElement.values[0].value);

  // Clean
  await umbracoApi.element.ensureNameNotExists(duplicatedElementName);
});

test('can not duplicate element with duplicate permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDuplicateElementPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isActionsMenuForNameVisible(elementName, false);
});

test('can move element with move to permission enabled', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementFolderName = 'TestElementFolder';
  const elementFolderId = await umbracoApi.element.createDefaultElementFolder(elementFolderName);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithMoveElementPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickActionsMenuForElement(elementName);
  await umbracoUi.library.clickMoveToActionMenuOption();
  await umbracoUi.library.moveToElementWithName([], elementFolderName);

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.moved);
  await umbracoUi.library.clickActionsMenuForElement(elementFolderName);
  await umbracoUi.library.clickReloadChildrenActionMenuOption();
  await umbracoUi.library.isCaretButtonVisibleForElementName(elementFolderName, true);
  await umbracoUi.library.openElementCaretButtonForName(elementFolderName);
  await umbracoUi.library.isChildElementInTreeVisible(elementFolderName, elementName, true);
  expect(await umbracoApi.element.getChildrenAmount(elementFolderId)).toEqual(1);

  // Clean
  await umbracoApi.element.ensureNameNotExists(elementFolderName);
});

test('can not move element with move to permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementFolderName = 'TestElementFolder';
  await umbracoApi.element.createDefaultElementFolder(elementFolderName);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithMoveElementPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isActionsMenuForNameVisible(elementName, false);

  // Clean
  await umbracoApi.element.ensureNameNotExists(elementFolderName);
});

test('can rollback element with rollback permission enabled', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithRollbackElementPermission(userGroupName);
  await umbracoApi.element.publish(elementId);
  const updatedTextStringText = 'This is an updated textString text';
  const element = await umbracoApi.element.get(elementId);
  element.values[0].value = updatedTextStringText;
  await umbracoApi.element.update(elementId, element);
  await umbracoApi.element.publish(elementId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.goToElementWithName(elementName);
  await umbracoUi.library.doesElementPropertyHaveValue(dataTypeName, updatedTextStringText);
  await umbracoUi.library.clickInfoTab();
  await umbracoUi.library.clickRollbackButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);// Wait for the rollback items to load
  await umbracoUi.library.clickLatestRollBackItem();
  await umbracoUi.library.clickRollbackContainerButton();

  // Assert
  await umbracoUi.library.clickContentTab();
  await umbracoUi.library.doesElementPropertyHaveValue(dataTypeName, elementText);
});

test('can not rollback element with rollback permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithRollbackElementPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isActionsMenuForNameVisible(elementName, false);
});

test('can not see delete button in element for userGroup with delete permission disabled and create permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeleteElementPermissionAndCreateElementPermission(userGroupName, false, true);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);
  await umbracoUi.library.clickActionsMenuForElement(elementName);

  // Assert
  await umbracoUi.library.isPermissionInActionsMenuVisible('Delete…', false);
  await umbracoUi.library.isPermissionInActionsMenuVisible('Create…', true);
});

test('can create and update element with permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedElementName = newElementName + ' Updated';
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCreateAndUpdateElementPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickActionsMenuAtRoot();
  await umbracoUi.library.clickCreateActionMenuOption();
  await umbracoUi.library.clickElementButton();
  await umbracoUi.library.clickModalMenuItemWithName(elementTypeName);
  await umbracoUi.library.clickChooseModalButton();
  await umbracoUi.library.enterElementName(newElementName);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeCreated();

  // Assert
  expect(await umbracoApi.element.doesNameExist(newElementName)).toBeTruthy();
  // Update the element
  await umbracoUi.library.goToElementWithName(newElementName);
  await umbracoUi.library.isElementReadOnly(false);
  await umbracoUi.library.enterElementName(updatedElementName);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeUpdated();
  expect(await umbracoApi.element.doesNameExist(updatedElementName)).toBeTruthy();
  await umbracoUi.library.doesElementHaveName(updatedElementName);

  // Cleanup
  await umbracoApi.element.ensureNameNotExists(updatedElementName);
});
