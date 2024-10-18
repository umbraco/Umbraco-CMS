import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

const testUser = {
  name: 'Test User',
  email: 'verySecureEmail@123.test',
  password: 'verySecurePassword123',
};

const userGroupName = 'TestUserGroup';

const rootDocumentTypeName = 'RootDocumentType';
const childDocumentTypeOneName = 'ChildDocumentTypeOne';
const childDocumentTypeTwoName = 'ChildDocumentTypeTwo';
let childDocumentTypeOneId = null;
let rootDocumentTypeId = null;

let testUserCookieAndToken = {cookie: "", accessToken: "", refreshToken: ""};

let rootDocumentId = null;
let childDocumentOneId = null;
const rootDocumentName = 'RootDocument';
const childDocumentOneName = 'ChildDocumentOne';
const childDocumentTwoName = 'ChildDocumentTwo';

let userGroupId = null;

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(rootDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeOneName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeTwoName);
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  childDocumentTypeOneId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeOneName);
  const childDocumentTypeTwoId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeTwoName);
  rootDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedTwoChildNodes(rootDocumentTypeName, childDocumentTypeOneId, childDocumentTypeTwoId);
  rootDocumentId = await umbracoApi.document.createDefaultDocument(rootDocumentName, rootDocumentTypeId);
  childDocumentOneId = await umbracoApi.document.createDefaultDocumentWithParent(childDocumentOneName, childDocumentTypeOneId, rootDocumentId);
  await umbracoApi.document.createDefaultDocumentWithParent(childDocumentTwoName, childDocumentTypeTwoId, rootDocumentId);
  userGroupId = await umbracoApi.userGroup.createSimpleUserGroupWithContentSection(userGroupName);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser(testUserCookieAndToken.cookie, testUserCookieAndToken.accessToken, testUserCookieAndToken.refreshToken);
  await umbracoApi.documentType.ensureNameNotExists(rootDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeOneName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeTwoName);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
});

test('can see root start node and children', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId, [rootDocumentId]);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.user.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isContentVisible(rootDocumentName);
  await umbracoUi.content.clickCaretButtonForContentName(rootDocumentName);
  await umbracoUi.content.isChildContentVisible(rootDocumentName, childDocumentOneName);
  await umbracoUi.content.isChildContentVisible(rootDocumentName, childDocumentTwoName);
});

test('can see parent of start node but not access it', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId, [childDocumentOneId]);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.user.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isContentVisible(rootDocumentName);
  await umbracoUi.content.goToContentWithName(rootDocumentName);
  await umbracoUi.content.isTextWithMessageVisible('The authenticated user do not have access to this resource');
  await umbracoUi.content.clickCaretButtonForContentName(rootDocumentName);
  await umbracoUi.content.isChildContentVisible(rootDocumentName, childDocumentOneName);
  await umbracoUi.content.isChildContentVisible(rootDocumentName, childDocumentTwoName, false);
});

test('can not see any content when no start nodes specified', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.user.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.isContentVisible(rootDocumentName, false);
});
