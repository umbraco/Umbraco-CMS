import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

const testUser = {
  name: 'Test User',
  email: 'verySecureEmail@123.test',
  password: 'verySecurePassword123',
}

const userGroupName = 'TestUserGroup';
let userGroupId = null;

let testUserCookieAndToken = {cookie: "", accessToken: "", refreshToken: ""}

let rootFolderId = null
let childFolderOneId = null;
const rootFolderName = 'RootFolder';
const childFolderOneName = 'ChildFolderOne';
const childFolderTwoName = 'ChildFolderTwo';

test.beforeEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.refreshAccessToken(process.env.UMBRACO_USER_LOGIN, process.env.UMBRACO_USER_PASSWORD);
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.media.ensureNameNotExists(rootFolderName);
  await umbracoApi.media.ensureNameNotExists(childFolderOneName);
  await umbracoApi.media.ensureNameNotExists(childFolderTwoName);

  // The media name is redundant in the method, as well as default
  rootFolderId = await umbracoApi.media.createDefaultMediaFolder(rootFolderName);
  childFolderOneId = await umbracoApi.media.createDefaultMediaFolderAndParentId(childFolderOneName, rootFolderId);
  await umbracoApi.media.createDefaultMediaFolderAndParentId(childFolderTwoName, rootFolderId);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithMediaSection(userGroupName);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser(testUserCookieAndToken.cookie, testUserCookieAndToken.accessToken, testUserCookieAndToken.refreshToken);
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.media.ensureNameNotExists(rootFolderName);
  await umbracoApi.media.ensureNameNotExists(childFolderOneName);
  await umbracoApi.media.ensureNameNotExists(childFolderTwoName);
});

test('can see root media start node and children', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId, [], false, [rootFolderId]);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.user.goToSection(ConstantHelper.sections.media, true);

  // Assert
  await umbracoUi.media.isMediaVisible(rootFolderName);
  await umbracoUi.media.clickCaretButtonForMediaName(rootFolderName);
  await umbracoUi.media.isChildMediaVisible(rootFolderName, childFolderOneName);
  await umbracoUi.media.isChildMediaVisible(rootFolderName, childFolderTwoName);
});

test('can see parent of start node but not access it', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId, [], false, [childFolderOneId]);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.user.goToSection(ConstantHelper.sections.media, true);

  // Assert
  await umbracoUi.media.isMediaVisible(rootFolderName);
  await umbracoUi.media.goToMediaWithName(rootFolderName);
  await umbracoUi.media.isTextWithMessageVisible('The authenticated user do not have access to this resource');
  await umbracoUi.media.clickCaretButtonForMediaName(rootFolderName);
  await umbracoUi.media.isChildMediaVisible(rootFolderName, childFolderOneName);
  await umbracoUi.media.isChildMediaVisible(rootFolderName, childFolderTwoName, false);
});

test('can not see any media when no media start nodes specified', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.user.goToSection(ConstantHelper.sections.media, true);

  // Assert
  await umbracoUi.media.isMediaVisible(rootFolderName, false);
});
