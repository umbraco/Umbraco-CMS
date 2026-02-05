import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

const testUser = ConstantHelper.testUserCredentials;
let testUserCookieAndToken = {cookie: '', accessToken: '', refreshToken: ''};

const userGroupName = 'TestUserGroup';
let userGroupId = null;

let rootFolderId = null;
let childElementOneId = null;
const rootFolderName = 'RootFolder';
const childElementOneName = 'ChildElementOne';
const childElementTwoName = 'ChildElementTwo';
const elementTypeName = 'TestElementType';
let elementTypeId = null;

test.beforeEach(async ({umbracoApi}) => {
  elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  rootFolderId = await umbracoApi.element.createDefaultElementFolder(rootFolderName);
  childElementOneId = await umbracoApi.element.createDefaultElementWithParent(childElementOneName, elementTypeId, rootFolderId);
  await umbracoApi.element.createDefaultElementWithParent(childElementTwoName, elementTypeId, rootFolderId);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser(testUserCookieAndToken.cookie, testUserCookieAndToken.accessToken, testUserCookieAndToken.refreshToken);
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.element.ensureNameNotExists(childElementOneName);
  await umbracoApi.element.ensureNameNotExists(childElementTwoName);
  await umbracoApi.element.ensureNameNotExists(rootFolderName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can see root element start node and children', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithElementStartNode(userGroupName, rootFolderId);
  await umbracoApi.user.setUserPermissionsForElement(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.user.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isElementInTreeVisible(rootFolderName);
  await umbracoUi.library.openElementCaretButtonForName(rootFolderName);
  await umbracoUi.library.isChildElementInTreeVisible(rootFolderName, childElementOneName);
  await umbracoUi.library.isChildElementInTreeVisible(rootFolderName, childElementTwoName);
});

test('can see parent of start node but not access it', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithElementStartNode(userGroupName, childElementOneId);
  await umbracoApi.user.setUserPermissionsForElement(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.user.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isElementInTreeVisible(rootFolderName);
  await umbracoUi.library.goToElementWithName(rootFolderName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short); // Wait for workspace to load
  await umbracoUi.library.doesElementWorkspaceHaveText('Access denied');
  await umbracoUi.library.openElementCaretButtonForName(rootFolderName);
  await umbracoUi.library.isChildElementInTreeVisible(rootFolderName, childElementOneName);
  await umbracoUi.library.isChildElementInTreeVisible(rootFolderName, childElementTwoName, false);
});

test('can not see any element when no element start nodes specified', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createSimpleUserGroupWithLibrarySection(userGroupName);
  await umbracoApi.user.setUserPermissionsForElement(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.user.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.library.isElementInTreeVisible(rootFolderName, false);
});
