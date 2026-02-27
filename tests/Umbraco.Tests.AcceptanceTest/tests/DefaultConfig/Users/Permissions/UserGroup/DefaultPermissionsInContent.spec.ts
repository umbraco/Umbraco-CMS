import {ConstantHelper, NotificationConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

// Document Type
const rootDocumentTypeName = 'RootDocumentType';
const childDocumentTypeOneName = 'ChildDocumentTypeOne';
const childDocumentTypeTwoName = 'ChildDocumentTypeTwo';
let childDocumentTypeId = null;
let rootDocumentTypeId = null;

// Document
const rootDocumentName = 'RootDocument';
const childDocumentOneName = 'ChildDocumentOne';
const childDocumentTwoName = 'SecondChildDocument';
let rootDocumentId = null;

// Data Type
const dataTypeName = 'Textstring';
let dataTypeId = null;
const documentText = 'This is test document text';

// Document Blueprint
const testDocumentName = 'TestDocument';
const documentBlueprintName = 'TestBlueprintName';

// User
const testUser = ConstantHelper.testUserCredentials;
let testUserCookieAndToken = {cookie: "", accessToken: "", refreshToken: ""};

// User Group
const userGroupName = 'TestUserGroup';
let userGroupId = null;

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(rootDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeOneName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeTwoName);
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintName);
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeId = dataType.id;
  childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeOneName);
  rootDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndDataType(rootDocumentTypeName, childDocumentTypeId, dataTypeName, dataTypeId);
  rootDocumentId = await umbracoApi.document.createDocumentWithTextContent(rootDocumentName, rootDocumentTypeId, documentText, dataTypeName);
  await umbracoApi.document.createDefaultDocumentWithParent(childDocumentOneName, childDocumentTypeId, rootDocumentId);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser(testUserCookieAndToken.cookie, testUserCookieAndToken.accessToken, testUserCookieAndToken.refreshToken);
  await umbracoApi.documentType.ensureNameNotExists(rootDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeOneName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeTwoName);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintName);
});

test('can read content node with permission enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithReadDocumentPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.goToContentWithName(rootDocumentName);

  // Assert
  await umbracoUi.content.doesDocumentHaveName(rootDocumentName);
});

// Skip this test due to this issue: https://github.com/umbraco/Umbraco-CMS/issues/20505
test.skip('can not read content node with permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithReadDocumentPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.goToContentWithName(rootDocumentName);

  // Assert
  await umbracoUi.content.doesDocumentWorkspaceHaveText('Not found');
});

test('can create document blueprint with permission enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCreateDocumentBlueprintPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickCreateBlueprintActionMenuOption();
  await umbracoUi.content.enterDocumentBlueprintName(documentBlueprintName);
  await umbracoUi.content.clickSaveDocumentBlueprintButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.documentBlueprintCreated);
});

test('can not create document blueprint with permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCreateDocumentBlueprintPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.documentBlueprint.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can delete content with delete permission enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeleteDocumentPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickTrashActionMenuOption();
  await umbracoUi.content.clickConfirmTrashButtonAndWaitForContentToBeTrashed();

  // Assert
  await umbracoUi.content.isItemVisibleInRecycleBin(rootDocumentName);
});

test('can not delete content with delete permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeleteDocumentPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can empty recycle bin with delete permission enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.moveToRecycleBin(rootDocumentId);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeleteDocumentPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickRecycleBinButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.content.clickEmptyRecycleBinButton();
  await umbracoUi.content.clickConfirmEmptyRecycleBinButtonAndWaitForRecycleBinToBeEmptied();

  // Assert
  await umbracoUi.content.isItemVisibleInRecycleBin(rootDocumentName, false, false);
});

test('can not empty recycle bin with delete permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.moveToRecycleBin(rootDocumentId);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeleteDocumentPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForRecycleBinVisible(false);
});

test('can create content with create permission enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCreateDocumentPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(rootDocumentTypeName);
  await umbracoUi.content.enterContentName(testDocumentName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(testDocumentName)).toBeTruthy();
  await umbracoUi.content.isDocumentReadOnly(true);
});

test('can not create content with create permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCreateDocumentPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can set up notifications with notification permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const notificationActionIds = ['Umb.Document.Delete', 'Umb.Document.Publish'];
  userGroupId = await umbracoApi.userGroup.createUserGroupWithNotificationsDocumentPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickNotificationsActionMenuOption();
  await umbracoUi.content.clickDocumentNotificationOptionWithName(notificationActionIds[0]);
  await umbracoUi.content.clickDocumentNotificationOptionWithName(notificationActionIds[1]);
  await umbracoUi.content.clickSaveModalButtonAndWaitForNotificationToBeCreated();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNotificationExist(rootDocumentId, notificationActionIds[0])).toBeTruthy();
  expect(await umbracoApi.document.doesNotificationExist(rootDocumentId, notificationActionIds[1])).toBeTruthy();
});

test('can not set up notifications with notification permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithNotificationsDocumentPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can publish content with publish permission enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPublishDocumentPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickPublishActionMenuOption();
  await umbracoUi.content.clickConfirmToPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  expect(await umbracoApi.document.isDocumentPublished(rootDocumentId)).toBeTruthy();
});

test('can not publish content with publish permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPublishDocumentPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

// Skip this as this function is removed from the front-end.
test.skip('can set permissions with set permissions permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithSetPermissionsDocumentPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  // await umbracoUi.content.clickSetPermissionsButton();

  // Assert
  // await umbracoUi.content.doesDocumentPermissionsDialogExist();
});

// Skip this as this function is removed from the front-end.
test.skip('can not set permissions with set permissions permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithSetPermissionsDocumentPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can unpublish content with unpublish permission enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.publish(rootDocumentId);
  expect(await umbracoApi.document.isDocumentPublished(rootDocumentId)).toBeTruthy();
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUnpublishDocumentPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickUnpublishActionMenuOption();
  await umbracoUi.content.clickConfirmToUnpublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.unpublished);
  expect(await umbracoApi.document.isDocumentPublished(rootDocumentId)).toBeFalsy();
});

test('can not unpublish content with unpublish permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.publish(rootDocumentId);
  expect(await umbracoApi.document.isDocumentPublished(rootDocumentId)).toBeTruthy();
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUnpublishDocumentPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can update content with update permission enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdateDocumentPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.goToContentWithName(rootDocumentName);
  await umbracoUi.content.isDocumentReadOnly(false);
  await umbracoUi.content.enterContentName(testDocumentName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(testDocumentName)).toBeTruthy();
  expect(await umbracoApi.document.doesNameExist(rootDocumentName)).toBeFalsy();
});

test('can not update content with update permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdateDocumentPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(rootDocumentName);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
  await umbracoUi.content.isDocumentReadOnly(true);
});

test('can duplicate content with duplicate permission enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const duplicatedContentName = rootDocumentName + ' (1)';
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDuplicateDocumentPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  // Duplicate to root
  await umbracoUi.content.clickDuplicateToActionMenuOption();
  await umbracoUi.content.clickLabelWithName('Content');
  await umbracoUi.content.clickDuplicateButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.duplicated);
  expect(await umbracoApi.document.doesNameExist(rootDocumentName)).toBeTruthy();
  expect(await umbracoApi.document.doesNameExist(duplicatedContentName)).toBeTruthy();
  await umbracoUi.content.isContentInTreeVisible(rootDocumentName);
  await umbracoUi.content.isContentInTreeVisible(duplicatedContentName);
  const rootContent = await umbracoApi.document.getByName(rootDocumentName);
  const rootDuplicatedContent = await umbracoApi.document.getByName(duplicatedContentName);
  expect(rootContent.values[0].value).toEqual(rootDuplicatedContent.values[0].value);
});

test('can not duplicate content with duplicate permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDuplicateDocumentPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can move content with move to permission enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const moveToDocumentName = 'SecondRootDocument';
  const moveToDocumentId = await umbracoApi.document.createDocumentWithTextContent(moveToDocumentName, rootDocumentTypeId, documentText, dataTypeName);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithMoveToDocumentPermission(userGroupName, true);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.openContentCaretButtonForName(rootDocumentName);
  await umbracoUi.content.clickActionsMenuForContent(childDocumentOneName);
  await umbracoUi.content.clickMoveToActionMenuOption();
  await umbracoUi.content.moveToContentWithName([], moveToDocumentName);

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.moved);
  await umbracoUi.content.clickActionsMenuForContent(moveToDocumentName);
  await umbracoUi.content.clickReloadChildrenActionMenuOption();
  await umbracoUi.content.isCaretButtonVisibleForContentName(moveToDocumentName, true);
  await umbracoUi.content.openContentCaretButtonForName(moveToDocumentName);
  await umbracoUi.content.isChildContentInTreeVisible(moveToDocumentName, childDocumentOneName, true);
  await umbracoUi.content.isCaretButtonVisibleForContentName(rootDocumentName, false);
  expect(await umbracoApi.document.getChildrenAmount(rootDocumentId)).toEqual(0);
  expect(await umbracoApi.document.getChildrenAmount(moveToDocumentId)).toEqual(1);
});

test('can not move content with move to permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const moveToDocumentName = 'SecondRootDocument';
  await umbracoApi.document.createDocumentWithTextContent(moveToDocumentName, rootDocumentTypeId, documentText, dataTypeName);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithMoveToDocumentPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

// Needs a better way to assert
test('can sort children with sort children permission enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithParent(childDocumentTwoName, childDocumentTypeId, rootDocumentId);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithSortChildrenDocumentPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickSortChildrenActionMenuOption();
  const childDocumentOneLocator = umbracoUi.content.getTextLocatorWithName(childDocumentOneName);
  const childDocumentTwoLocator = umbracoUi.content.getTextLocatorWithName(childDocumentTwoName)
  await umbracoUi.content.dragAndDrop(childDocumentTwoLocator, childDocumentOneLocator);
  await umbracoUi.content.clickSortButton();

  // Assert
  await umbracoUi.content.openContentCaretButtonForName(rootDocumentName);
  await umbracoUi.content.doesIndexDocumentInTreeContainName(rootDocumentName, childDocumentTwoName, 0);
  await umbracoUi.content.doesIndexDocumentInTreeContainName(rootDocumentName, childDocumentOneName, 1);
});

test('can not sort children with sort children permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithParent(childDocumentTwoName, childDocumentTypeId, rootDocumentId);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithSortChildrenDocumentPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can set culture and hostnames with culture and hostnames permission enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const domainName = '/domain';
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCultureAndHostnamesDocumentPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickCultureAndHostnamesActionMenuOption();
  await umbracoUi.content.clickAddNewHostnameButton();
  await umbracoUi.content.enterDomain(domainName);
  await umbracoUi.content.clickSaveModalButtonAndWaitForDomainToBeCreated();

  // Assert
  const document = await umbracoApi.document.getByName(rootDocumentName);
  const domains = await umbracoApi.document.getDomains(document.id);
  expect(domains.domains[0].domainName).toEqual(domainName);
  expect(domains.domains[0].isoCode).toEqual('en-US');
});

test('can not set culture and hostnames with culture and hostnames permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCultureAndHostnamesDocumentPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can set public access with public access permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPublicAccessDocumentPermission(userGroupName);
  const testMemberGroup = 'TestMemberGroup';
  await umbracoApi.memberGroup.ensureNameNotExists(testMemberGroup);
  await umbracoApi.memberGroup.create(testMemberGroup)
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickPublicAccessActionMenuOption();
  await umbracoUi.content.addGroupBasedPublicAccess(testMemberGroup, rootDocumentName);

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.publicAccessSettingCreated);
});

test('can not set public access with public access permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPublicAccessDocumentPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can rollback content with rollback permission enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithRollbackDocumentPermission(userGroupName);
  await umbracoApi.document.publish(rootDocumentId);
  const updatedTextStringText = 'This is an updated textString text';
  const content = await umbracoApi.document.get(rootDocumentId);
  content.values[0].value = updatedTextStringText;
  await umbracoApi.document.update(rootDocumentId, content);
  await umbracoApi.document.publish(rootDocumentId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.goToContentWithName(rootDocumentName);
  await umbracoUi.content.doesDocumentPropertyHaveValue(dataTypeName, updatedTextStringText);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.clickRollbackButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);// Wait for the rollback items to load
  await umbracoUi.content.clickLatestRollBackItem();
  await umbracoUi.content.clickRollbackContainerButton();

  // Assert
  await umbracoUi.content.clickContentTab();
  await umbracoUi.content.doesDocumentPropertyHaveValue(dataTypeName, documentText);
});

test('can not rollback content with rollback permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithRollbackDocumentPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can not see delete button in content for userGroup with delete permission disabled and create permission enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeleteDocumentPermissionAndCreateDocumentPermission(userGroupName, false, true);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);

  // Assert
  await umbracoUi.content.isPermissionInActionsMenuVisible('Delete…', false);
  await umbracoUi.content.isPermissionInActionsMenuVisible('Create…', true);
});

test('can create and update content with permission enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedDocumentName = testDocumentName + ' Updated';
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCreateAndUpdateDocumentPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(rootDocumentTypeName);
  await umbracoUi.content.enterContentName(testDocumentName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(testDocumentName)).toBeTruthy();
  // Update the content
  await umbracoUi.content.goToContentWithName(testDocumentName);
  await umbracoUi.content.isDocumentReadOnly(false);
  await umbracoUi.content.enterContentName(updatedDocumentName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  expect(await umbracoApi.document.doesNameExist(updatedDocumentName)).toBeTruthy();
  await umbracoUi.content.doesDocumentHaveName(updatedDocumentName);

  // Cleanup
  await umbracoApi.document.ensureNameNotExists(updatedDocumentName);
});
