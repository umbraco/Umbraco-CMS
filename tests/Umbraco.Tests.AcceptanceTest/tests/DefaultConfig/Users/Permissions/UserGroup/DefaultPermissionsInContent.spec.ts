import {ConstantHelper, NotificationConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

const rootDocumentTypeName = 'RootDocumentType';
const childDocumentTypeOneName = 'ChildDocumentTypeOne';
const childDocumentTypeTwoName = 'ChildDocumentTypeTwo';
let childDocumentTypeId = null;
let rootDocumentTypeId = null;
const rootDocumentName = 'RootDocument';
const childDocumentOneName = 'ChildDocumentOne';
const childDocumentTwoName = 'SecondChildDocument';
let rootDocumentId = null;

const dataTypeName = 'Textstring';
let dataTypeId = null;
const documentText = 'This is test document text';

const testDocumentName = 'TestDocument';
const documentBlueprintName = 'TestBlueprintName';

const testUser = ConstantHelper.testUserCredentials;
let testUserCookieAndToken = {cookie: "", accessToken: "", refreshToken: ""};

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

test('can browse content node with permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithBrowseNodePermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.goToContentWithName(rootDocumentName);

  // Assert
  await umbracoUi.content.doesDocumentHaveName(rootDocumentName);
});

test('can not browse content node with permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithBrowseNodePermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.goToContentWithName(rootDocumentName);

  // Assert
  await umbracoUi.content.isErrorNotificationVisible();
  // TODO: Uncomment this when this issue is fixed https://github.com/umbraco/Umbraco-CMS/issues/18533
  //await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.noAccessToResource);
});

test('can create document blueprint with permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCreateDocumentBlueprintPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickCreateDocumentBlueprintButton();
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

test('can delete content with delete permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeleteDocumentPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickTrashButton();
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.movedToRecycleBin);
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

test('can empty recycle bin with delete permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.moveToRecycleBin(rootDocumentId);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeleteDocumentPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickRecycleBinButton();
  await umbracoUi.content.clickEmptyRecycleBinButton();
  await umbracoUi.content.clickConfirmEmptyRecycleBinButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.emptiedRecycleBin);
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

test('can create content with create permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCreateDocumentPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(rootDocumentTypeName);
  await umbracoUi.content.enterContentName(testDocumentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.content.isErrorNotificationVisible(false);
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

// TODO: Setup SMTP server to test notifications, do this when we test appsettings.json
test.skip('can create notifications with notification permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithNotificationsPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
});

test('can not create notifications with notification permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithNotificationsPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can publish content with publish permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPublishPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickPublishButton();
  await umbracoUi.content.clickConfirmToPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  expect(await umbracoApi.document.isDocumentPublished(rootDocumentId)).toBeTruthy();
});

test('can not publish content with publish permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPublishPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

// Bug, does nothing in the frontend.
test.skip('can set permissions with set permissions permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithSetPermissionsPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  // await umbracoUi.content.clickSetPermissionsButton();
  //
  // // Assert
  // await umbracoUi.content.doesDocumentPermissionsDialogExist();
});

test('can not set permissions with set permissions permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithSetPermissionsPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can unpublish content with unpublish permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.publish(rootDocumentId);
  expect(await umbracoApi.document.isDocumentPublished(rootDocumentId)).toBeTruthy();
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUnpublishPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickUnpublishButton();
  await umbracoUi.content.clickConfirmToUnpublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.unpublished);
  expect(await umbracoApi.document.isDocumentPublished(rootDocumentId)).toBeFalsy();
});

test('can not unpublish content with unpublish permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.publish(rootDocumentId);
  expect(await umbracoApi.document.isDocumentPublished(rootDocumentId)).toBeTruthy();
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUnpublishPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can update content with update permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdatePermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.goToContentWithName(rootDocumentName);
  await umbracoUi.content.isDocumentReadOnly(false);
  await umbracoUi.content.enterContentName(testDocumentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(testDocumentName)).toBeTruthy();
});

// TODO: the permission for update is not working, it is always enabled.
test.skip('can not update content with update permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdatePermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can duplicate content with duplicate permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const duplicatedContentName = rootDocumentName + ' (1)';
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDuplicatePermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  // Duplicate to root
  await umbracoUi.content.clickDuplicateToButton();
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
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDuplicatePermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can move content with move to permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const moveToDocumentName = 'SecondRootDocument';
  const moveToDocumentId = await umbracoApi.document.createDocumentWithTextContent(moveToDocumentName, rootDocumentTypeId, documentText, dataTypeName);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithMoveToPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickCaretButtonForContentName(rootDocumentName);
  await umbracoUi.content.clickActionsMenuForContent(childDocumentOneName);
  await umbracoUi.content.clickMoveToButton();
  await umbracoUi.content.moveToContentWithName([], moveToDocumentName);

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.moved);
  await umbracoUi.content.reloadContentTree();
  await umbracoUi.content.isCaretButtonVisibleForContentName(moveToDocumentName, true);
  await umbracoUi.content.clickCaretButtonForContentName(moveToDocumentName);
  await umbracoUi.content.isChildContentInTreeVisible(moveToDocumentName, childDocumentOneName, true);
  await umbracoUi.content.isCaretButtonVisibleForContentName(rootDocumentName, false);
  expect(await umbracoApi.document.getChildrenAmount(rootDocumentId)).toEqual(0);
  expect(await umbracoApi.document.getChildrenAmount(moveToDocumentId)).toEqual(1);
});

test('can not move content with move to permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const moveToDocumentName = 'SecondRootDocument';
  await umbracoApi.document.createDocumentWithTextContent(moveToDocumentName, rootDocumentTypeId, documentText, dataTypeName);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithMoveToPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can sort children with sort children permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithParent(childDocumentTwoName, childDocumentTypeId, rootDocumentId);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithSortChildrenPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickSortChildrenButton();

  // TODO: uncomment when it is not flaky
  // const childDocumentOneLocator = await umbracoUi.content.getButtonWithName(childDocumentOneName);
  // const childDocumentTwoLocator = await umbracoUi.content.getButtonWithName(childDocumentTwoName)
  // await umbracoUi.content.sortChildrenDragAndDrop(childDocumentOneLocator, childDocumentTwoLocator, 10, 0, 10);
  await umbracoUi.content.clickSortButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.itemsSorted);
  // TODO: uncomment when it is not flaky
  // await umbracoUi.content.clickCaretButtonForContentName(rootDocumentName);
  // await umbracoUi.content.doesIndexDocumentInTreeContainName(rootDocumentName, childDocumentTwoName, 0);
  // await umbracoUi.content.doesIndexDocumentInTreeContainName(rootDocumentName, childDocumentOneName, 1);
});

test('can not sort children with sort children permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithParent(childDocumentTwoName, childDocumentTypeId, rootDocumentId);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithSortChildrenPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can set culture and hostnames with culture and hostnames permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCultureAndHostnamesPermission(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickCultureAndHostnamesButton();
  await umbracoUi.content.clickAddNewDomainButton();
  await umbracoUi.content.enterDomain('/domain');
  await umbracoUi.content.clickSaveModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.culturesAndHostnamesSaved);
});

test('can not set culture and hostnames with culture and hostnames permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCultureAndHostnamesPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

// TODO: Notification is not correct 'Public acccess setting created' should be 'access'
test.skip('can set public access with public access permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPublicAccessPermission(userGroupName);
  const testMemberGroup = 'TestMemberGroup';
  await umbracoApi.memberGroup.ensureNameNotExists(testMemberGroup);
  await umbracoApi.memberGroup.create(testMemberGroup)
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickPublicAccessButton();
  await umbracoUi.content.addGroupBasedPublicAccess(testMemberGroup, rootDocumentName);

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.publicAccessSettingCreated);
});

test('can not set public access with public access permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPublicAccessPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can rollback content with rollback permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithRollbackPermission(userGroupName);
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
  await umbracoUi.content.clickLatestRollBackItem();
  await umbracoUi.content.clickRollbackContainerButton();

  // Assert
  await umbracoUi.content.clickContentTab();
  await umbracoUi.content.doesDocumentPropertyHaveValue(dataTypeName, documentText);
});

test('can not rollback content with rollback permission disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithRollbackPermission(userGroupName, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isActionsMenuForNameVisible(rootDocumentName, false);
});

test('can not see delete button in content for userGroup with delete permission disabled and create permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeletePermissionAndCreatePermission(userGroupName, false, true);
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
