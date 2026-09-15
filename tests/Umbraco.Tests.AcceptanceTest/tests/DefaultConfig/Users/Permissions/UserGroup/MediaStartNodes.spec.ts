import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

const testUser = ConstantHelper.testUserCredentials;

const userGroupName = 'TestUserGroup';
let userGroupId = null;

let rootFolderId = null;
let childFolderOneId = null;
const rootFolderName = 'RootFolder';
const childFolderOneName = 'ChildFolderOne';
const childFolderTwoName = 'ChildFolderTwo';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.media.ensureNameNotExists(rootFolderName);
  await umbracoApi.media.ensureNameNotExists(childFolderOneName);
  await umbracoApi.media.ensureNameNotExists(childFolderTwoName);
  rootFolderId = await umbracoApi.media.createDefaultMediaFolder(rootFolderName);
  childFolderOneId = await umbracoApi.media.createDefaultMediaFolderAndParentId(childFolderOneName, rootFolderId);
  await umbracoApi.media.createDefaultMediaFolderAndParentId(childFolderTwoName, rootFolderId);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser();
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.media.ensureNameNotExists(rootFolderName);
  await umbracoApi.media.ensureNameNotExists(childFolderOneName);
  await umbracoApi.media.ensureNameNotExists(childFolderTwoName);
});

test('can see root media start node and children', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithMediaStartNode(userGroupName, rootFolderId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.user.goToSection(ConstantHelper.sections.media, false);

  // Assert
  await umbracoUi.media.isMediaTreeItemVisible(rootFolderName);
  await umbracoUi.media.openMediaCaretButtonForName(rootFolderName);
  await umbracoUi.media.isChildMediaVisible(rootFolderName, childFolderOneName);
  await umbracoUi.media.isChildMediaVisible(rootFolderName, childFolderTwoName);
});

test('can see parent of start node but not access it', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithMediaStartNode(userGroupName, childFolderOneId);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.user.goToSection(ConstantHelper.sections.media, false);

  // Assert
  await umbracoUi.media.isMediaTreeItemVisible(rootFolderName);
  // A folder the user cannot access is rendered disabled and without an href, so clicking it must not navigate.
  await umbracoUi.page.waitForURL('**/section/media/collection');
  const initialUrl = umbracoUi.page.url();
  await umbracoUi.media.goToMediaWithName(rootFolderName);
  expect(umbracoUi.page.url()).toBe(initialUrl);
  await umbracoUi.media.openMediaCaretButtonForName(rootFolderName);
  await umbracoUi.media.isChildMediaVisible(rootFolderName, childFolderOneName);
  await umbracoUi.media.isChildMediaVisible(rootFolderName, childFolderTwoName, false);
});

test('can not see any media when no media start nodes specified', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createSimpleUserGroupWithMediaSection(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.user.goToSection(ConstantHelper.sections.media, false);

  // Assert
  await umbracoUi.media.isMediaTreeItemVisible(rootFolderName, false);
});
