import {ConstantHelper, NotificationConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

// Document Type
const documentTypeName = 'DocumentType';
const childDocumentTypeName = 'ChildDocumentType';
let documentTypeId = null;
let childDocumentTypeId = null;

// Document
const firstDocumentName = 'FirstDocumentName';
const secondDocumentName = 'SecondDocumentName';
const childDocumentName = 'ChildDocumentName';
let firstDocumentId = null;
let secondDocumentId = null;

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
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintName);
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeId = dataType.id;
  childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndDataType(documentTypeName, childDocumentTypeId, dataTypeName, dataTypeId);
  firstDocumentId = await umbracoApi.document.createDocumentWithTextContent(firstDocumentName, documentTypeId, documentText, dataTypeName);
  secondDocumentId = await umbracoApi.document.createDocumentWithTextContent(secondDocumentName, documentTypeId, documentText, dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser(testUserCookieAndToken.cookie, testUserCookieAndToken.accessToken, testUserCookieAndToken.refreshToken);
  await umbracoApi.document.ensureNameNotExists(firstDocumentName);
  await umbracoApi.document.ensureNameNotExists(secondDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintName);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
});

test('can read a specific document with read permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithReadPermissionForSpecificDocument(userGroupName, firstDocumentId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.goToContentWithName(firstDocumentName);

  // Assert
  await umbracoUi.content.doesDocumentHaveName(firstDocumentName);
  await umbracoUi.content.goToContentWithName(secondDocumentName);
  await umbracoUi.content.doesDocumentWorkspaceHaveText('Not found');
});

test('can create document blueprint for a specific document with create document blueprint permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCreateDocumentBlueprintPermissionForSpecificDocument(userGroupName, firstDocumentId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(firstDocumentName);
  await umbracoUi.content.clickCreateBlueprintActionMenuOption();
  await umbracoUi.content.enterDocumentBlueprintName(documentBlueprintName);
  await umbracoUi.content.clickSaveDocumentBlueprintButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.documentBlueprintCreated);
  await umbracoUi.content.isActionsMenuForNameVisible(secondDocumentName, false);
});

test('can delete a specific content with delete permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDeletePermissionForSpecificDocument(userGroupName, firstDocumentId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(firstDocumentName);
  await umbracoUi.content.clickTrashActionMenuOption();
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.waitForContentToBeDeleted();
  await umbracoUi.content.isActionsMenuForNameVisible(secondDocumentName, false);
});

test('can create content from a specific content with create permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCreatePermissionForSpecificDocument(userGroupName, firstDocumentId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(firstDocumentName);

  // Assert
  await umbracoUi.content.isPermissionInActionsMenuVisible('Create…');
  await umbracoUi.content.isActionsMenuForNameVisible(secondDocumentName, false);
});

test('can publish a specific content with publish permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPublishPermissionForSpecificDocument(userGroupName, firstDocumentId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(firstDocumentName);
  await umbracoUi.content.clickPublishActionMenuOption();
  await umbracoUi.content.clickConfirmToPublishButton();

  // Assert
  await umbracoUi.content.waitForContentToBePublished();
  expect(await umbracoApi.document.isDocumentPublished(firstDocumentId)).toBeTruthy();
  await umbracoUi.content.isActionsMenuForNameVisible(secondDocumentName, false);
});

test('can unpublish a specific content with unpublish permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.publish(firstDocumentId);
  await umbracoApi.document.publish(secondDocumentId);
  expect(await umbracoApi.document.isDocumentPublished(firstDocumentId)).toBeTruthy();
  expect(await umbracoApi.document.isDocumentPublished(secondDocumentId)).toBeTruthy();
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUnpublishPermissionForSpecificDocument(userGroupName, firstDocumentId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(firstDocumentName);
  await umbracoUi.content.clickUnpublishActionMenuOption();
  await umbracoUi.content.clickConfirmToUnpublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.unpublished);
  expect(await umbracoApi.document.isDocumentPublished(firstDocumentId)).toBeFalsy();
  await umbracoUi.content.isActionsMenuForNameVisible(secondDocumentName, false);
});

test('can update a specific content with update permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdatePermissionForSpecificDocument(userGroupName, firstDocumentId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.goToContentWithName(firstDocumentName);
  await umbracoUi.content.isDocumentReadOnly(false);
  await umbracoUi.content.enterContentName(testDocumentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveButton();
  expect(await umbracoApi.document.doesNameExist(testDocumentName)).toBeTruthy();
  await umbracoUi.content.goToContentWithName(secondDocumentName);
  await umbracoUi.content.isActionsMenuForNameVisible(secondDocumentName, false);
  await umbracoUi.content.isDocumentReadOnly(true);
});

test('can duplicate a specific content with duplicate permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDuplicatePermissionForSpecificDocument(userGroupName, firstDocumentId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(firstDocumentName);
  // Duplicate to root
  await umbracoUi.content.clickDuplicateToActionMenuOption();
  await umbracoUi.content.clickLabelWithName('Content');
  await umbracoUi.content.clickDuplicateButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.duplicated);
  await umbracoUi.content.clickActionsMenuForContent(secondDocumentName);
  await umbracoUi.content.isPermissionInActionsMenuVisible('Duplicate to…', false);
});

test('can move a specific content with move to permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const moveToDocumentName = 'MoveToDocument';
  await umbracoApi.document.createDefaultDocumentWithParent(childDocumentName, childDocumentTypeId, firstDocumentId);
  await umbracoApi.document.createDocumentWithTextContent(moveToDocumentName, documentTypeId, documentText, dataTypeName);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithMoveToPermissionForSpecificDocument(userGroupName, firstDocumentId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickCaretButtonForContentName(firstDocumentName);
  await umbracoUi.content.clickActionsMenuForContent(childDocumentName);
  await umbracoUi.content.clickMoveToActionMenuOption();
  await umbracoUi.content.moveToContentWithName([], moveToDocumentName);

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.moved);
  await umbracoUi.content.clickActionsMenuForContent(secondDocumentName);
  await umbracoUi.content.isPermissionInActionsMenuVisible('Move to…', false);
});

test('can sort children with sort children permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithParent(childDocumentName, childDocumentTypeId, firstDocumentId);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithSortChildrenPermissionForSpecificDocument(userGroupName, firstDocumentId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(firstDocumentName);

  // Assert
  await umbracoUi.content.isPermissionInActionsMenuVisible('Sort children…');
  await umbracoUi.content.isActionsMenuForNameVisible(secondDocumentName, false);
});

test('can set culture and hostnames for a specific content with culture and hostnames permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const domainName = '/domain';
  userGroupId = await umbracoApi.userGroup.createUserGroupWithCultureAndHostnamesPermissionForSpecificDocument(userGroupName, firstDocumentId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(firstDocumentName);
  await umbracoUi.content.clickCultureAndHostnamesActionMenuOption();
  await umbracoUi.content.clickAddNewDomainButton();
  await umbracoUi.content.enterDomain(domainName);
  await umbracoUi.content.clickSaveModalButton();

  // Assert
  await umbracoUi.content.waitForDomainToBeCreated();
  const document = await umbracoApi.document.getByName(firstDocumentName);
  const domains = await umbracoApi.document.getDomains(document.id);
  expect(domains.domains[0].domainName).toEqual(domainName);
  expect(domains.domains[0].isoCode).toEqual('en-US');
  await umbracoUi.content.isActionsMenuForNameVisible(secondDocumentName, false);
});

test('can set public access for a specific content with public access permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPublicAccessPermissionForSpecificDocument(userGroupName, firstDocumentId);
  const testMemberGroup = 'TestMemberGroup';
  await umbracoApi.memberGroup.ensureNameNotExists(testMemberGroup);
  await umbracoApi.memberGroup.create(testMemberGroup);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(firstDocumentName);
  await umbracoUi.content.clickPublicAccessActionMenuOption();
  await umbracoUi.content.addGroupBasedPublicAccess(testMemberGroup, firstDocumentName);

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.publicAccessSettingCreated);
  await umbracoUi.content.isActionsMenuForNameVisible(secondDocumentName, false);

  // Clean
  await umbracoApi.memberGroup.ensureNameNotExists(testMemberGroup);
});

test('can rollback a specific content with rollback permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithRollbackPermissionForSpecificDocument(userGroupName, firstDocumentId);
  await umbracoApi.document.publish(firstDocumentId);
  const updatedTextStringText = 'This is an updated textString text';
  const contentData = await umbracoApi.document.get(firstDocumentId);
  contentData.values[0].value = updatedTextStringText;
  await umbracoApi.document.update(firstDocumentId, contentData);
  await umbracoApi.document.publish(firstDocumentId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(firstDocumentName);
  await umbracoUi.content.clickRollbackActionMenuOption();
  await umbracoUi.content.clickLatestRollBackItem();
  await umbracoUi.content.clickRollbackContainerButton();

  // Assert
  await umbracoUi.content.goToContentWithName(firstDocumentName);
  await umbracoUi.content.doesDocumentPropertyHaveValue(dataTypeName, documentText);
  await umbracoUi.content.isActionsMenuForNameVisible(secondDocumentName, false);
});
