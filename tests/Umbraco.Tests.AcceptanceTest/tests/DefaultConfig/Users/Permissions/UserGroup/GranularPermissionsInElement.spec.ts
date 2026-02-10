import {ConstantHelper, NotificationConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

// Element Type
const elementTypeName = 'TestElementType3';
let elementTypeId = '';

// Elements
const firstElementName = 'FirstElement';
const secondElementName = 'SecondElement';
let firstElementId = '';
let secondElementId = '';

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
  await umbracoApi.element.ensureNameNotExists(firstElementName);
  await umbracoApi.element.ensureNameNotExists(secondElementName);
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', dataTypeName, dataType.id);
  firstElementId = await umbracoApi.element.createElementWithTextContent(firstElementName, elementTypeId, elementText, dataTypeName);
  secondElementId = await umbracoApi.element.createElementWithTextContent(secondElementName, elementTypeId, elementText, dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  //Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser(testUserCookieAndToken.cookie, testUserCookieAndToken.accessToken, testUserCookieAndToken.refreshToken);
  await umbracoApi.element.ensureNameNotExists(firstElementName);
  await umbracoApi.element.ensureNameNotExists(secondElementName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.element.emptyRecycleBin();
});

test('can read a specific element with read permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithReadPermissionForSpecificElement(userGroupName, firstElementId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.goToElementWithName(firstElementName);

  // Assert
  await umbracoUi.library.doesElementHaveName(firstElementName);
  // TODO: Uncomment this when front-end is ready, currently the element with read permission disabled is not hidden in the tree
  await umbracoUi.library.isElementInTreeVisible(secondElementName, false);
});

test('can trash a specific element with delete permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeletePermissionForSpecificElement(userGroupName, firstElementId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickEntityActionOnElementWithName(firstElementName);
  await umbracoUi.library.clickConfirmTrashButtonAndWaitForElementToBeTrashed();

  // Assert
  await umbracoUi.library.isItemVisibleInRecycleBin(firstElementName);
  
});

test('can publish a specific element with publish permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPublishPermissionForSpecificElement(userGroupName, firstElementId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickEntityActionOnElementWithName(firstElementName);
  await umbracoUi.library.clickConfirmToPublishButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  expect(await umbracoApi.element.isElementPublished(firstElementId)).toBeTruthy();
  await umbracoUi.library.isEntityActionForElementWithNameHidden(secondElementName);
});

test('can unpublish a specific element with unpublish permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.publish(firstElementId);
  await umbracoApi.element.publish(secondElementId);
  expect(await umbracoApi.element.isElementPublished(firstElementId)).toBeTruthy();
  expect(await umbracoApi.element.isElementPublished(secondElementId)).toBeTruthy();
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUnpublishPermissionForSpecificElement(userGroupName, firstElementId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickEntityActionOnElementWithName(firstElementName);
  await umbracoUi.library.clickConfirmToUnpublishButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.unpublished);
  expect(await umbracoApi.element.isElementPublished(firstElementId)).toBeFalsy();
  await umbracoUi.library.isEntityActionForElementWithNameHidden(secondElementName);
});

// When the update permission for an element is disabled, the element is still editable; only the “Save” button is hidden.
test.fixme('can update a specific element with update permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const newElementName = 'UpdatedElement';
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdatePermissionForSpecificElement(userGroupName, firstElementId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.goToElementWithName(firstElementName);
  await umbracoUi.library.isElementReadOnly(false);
  await umbracoUi.library.enterElementName(newElementName);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeUpdated();

  // Assert
  expect(await umbracoApi.element.doesNameExist(newElementName)).toBeTruthy();
  await umbracoUi.library.goToElementWithName(secondElementName);
  await umbracoUi.library.isActionsMenuForNameVisible(secondElementName, false);
  await umbracoUi.library.isElementReadOnly(true);

  // Clean
  await umbracoApi.element.ensureNameNotExists(newElementName);
});

test.fixme('can duplicate a specific element with duplicate permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const duplicatedElementName = firstElementName + ' (1)';
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDuplicatePermissionForSpecificElement(userGroupName, firstElementId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickEntityActionOnElementWithName(firstElementName);
  await umbracoUi.library.clickLabelWithName('Elements');
  await umbracoUi.library.clickDuplicateButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.duplicated);
  expect(await umbracoApi.element.doesNameExist(firstElementName)).toBeTruthy();
  expect(await umbracoApi.element.doesNameExist(duplicatedElementName)).toBeTruthy();
  await umbracoUi.library.isEntityActionForElementWithNameHidden(secondElementName);

  // Clean
  await umbracoApi.element.ensureNameNotExists(duplicatedElementName);
});

test('can move a specific element with move permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementFolderName = 'TestElementFolder';
  const elementFolderId = await umbracoApi.element.createDefaultElementFolder(elementFolderName);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithMovePermissionForSpecificElement(userGroupName, firstElementId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickEntityActionOnElementWithName(firstElementName);
  await umbracoUi.library.moveToElementWithName([], elementFolderName);

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.moved);
  await umbracoUi.library.clickActionsMenuForElement(elementFolderName);
  await umbracoUi.library.clickReloadChildrenActionMenuOption();
  await umbracoUi.library.isCaretButtonVisibleForElementName(elementFolderName, true);
  await umbracoUi.library.openElementCaretButtonForName(elementFolderName);
  await umbracoUi.library.isChildElementInTreeVisible(elementFolderName, firstElementName, true);
  expect(await umbracoApi.element.getChildrenAmount(elementFolderId)).toEqual(1);
  await umbracoUi.library.isEntityActionForElementWithNameHidden(secondElementName);

  // Clean
  await umbracoApi.element.ensureNameNotExists(elementFolderName);
});

// Currently the rollback functionality in elements is not working
test.fixme('can rollback a specific element with rollback permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithRollbackPermissionForSpecificElement(userGroupName, firstElementId);
  await umbracoApi.element.publish(firstElementId);
  const updatedTextStringText = 'This is an updated textString text';
  const element = await umbracoApi.element.get(firstElementId);
  element.values[0].value = updatedTextStringText;
  await umbracoApi.element.update(firstElementId, element);
  await umbracoApi.element.publish(firstElementId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.goToElementWithName(firstElementName);
  await umbracoUi.library.doesElementPropertyHaveValue(dataTypeName, updatedTextStringText);
  await umbracoUi.library.clickInfoTab();
  await umbracoUi.library.clickRollbackButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.library.clickLatestRollBackItem();
  await umbracoUi.library.clickRollbackContainerButton();

  // Assert
  await umbracoUi.library.clickContentTab();
  await umbracoUi.library.doesElementPropertyHaveValue(dataTypeName, elementText);
  await umbracoUi.library.isElementInTreeVisible(secondElementName, false);
});

// Currently user cannot choose element type to create element even with permission enabled
test.fixme('can create element from a specific element with create permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const newElementName = 'NewElement';
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCreatePermissionForSpecificElement(userGroupName, firstElementId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);

  // Act
  await umbracoUi.library.clickActionsMenuForElement(firstElementName);
  await umbracoUi.library.clickCreateActionMenuOption();
  await umbracoUi.library.clickElementButton();
  await umbracoUi.library.clickModalMenuItemWithName(elementTypeName);
  await umbracoUi.library.clickChooseModalButton();
  await umbracoUi.library.enterElementName(newElementName);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeCreated();

  // Assert
  expect(await umbracoApi.element.doesNameExist(newElementName)).toBeTruthy();
  await umbracoUi.library.isElementInTreeVisible(secondElementName, false);

  // Clean
  await umbracoApi.element.ensureNameNotExists(newElementName);
});
