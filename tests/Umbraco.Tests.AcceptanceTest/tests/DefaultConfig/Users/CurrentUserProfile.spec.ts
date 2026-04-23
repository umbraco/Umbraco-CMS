import {expect} from '@playwright/test';
import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const userPassword = '0123456789';
const nonAdminUserGroupName = 'Editors';
const avatarFilePath = './fixtures/mediaLibrary/Umbraco.png';
let userName: string;
let userEmail: string;

test.beforeEach(async ({umbracoApi}) => {
  userName = 'TestUser' + crypto.randomUUID();
  userEmail = userName + '@test.com';
  await umbracoApi.user.ensureNameNotExists(userName);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser();
  await umbracoApi.user.ensureNameNotExists(userName);
});

const userGroups = ['Administrators', 'Editors', 'Sensitive data', 'Translators', 'Writers'];
for (const userGroup of userGroups) {
  test(`${userGroup} user can access the user profile and change the password`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const newPassword = 'TestNewPassword';
    const userGroupData = await umbracoApi.userGroup.getByName(userGroup);
    const userId = await umbracoApi.user.createDefaultUser(userName, userEmail, [userGroupData.id]);
    await umbracoApi.user.updatePassword(userId, userPassword);
    await umbracoApi.user.loginToUser(userName, userEmail, userPassword);
    await umbracoUi.goToBackOffice();
    await umbracoUi.currentUserProfile.isBackOfficeMainVisible();
    await umbracoUi.waitForTimeout(ConstantHelper.wait.medium); // Wait to ensure the UI is fully loaded

    // Act
    await umbracoUi.currentUserProfile.clickCurrentUserAvatarButton();
    await umbracoUi.currentUserProfile.isErrorNotificationVisible(false);
    await umbracoUi.currentUserProfile.clickChangePasswordButton();
    await umbracoUi.currentUserProfile.changePasswordAndWaitForSuccess(userPassword, newPassword);

    // Assert
    await umbracoUi.currentUserProfile.doesSuccessNotificationHaveText(NotificationConstantHelper.success.passwordChanged);
  });
}

test('non-admin user clicking Edit opens the workspace modal', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupData = await umbracoApi.userGroup.getByName(nonAdminUserGroupName);
  const userId = await umbracoApi.user.createDefaultUser(userName, userEmail, [userGroupData.id]);
  await umbracoApi.user.updatePassword(userId, userPassword);
  await umbracoApi.user.loginToUser(userName, userEmail, userPassword);
  await umbracoUi.goToBackOffice();
  await umbracoUi.currentUserProfile.isBackOfficeMainVisible();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

  // Act
  await umbracoUi.currentUserProfile.clickCurrentUserAvatarButton();
  await umbracoUi.currentUserProfile.clickEditButton();

  // Assert
  await umbracoUi.currentUserProfile.isWorkspaceModalVisible(true);
});

test('admin user clicking Edit navigates to user editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const currentUser = await umbracoApi.user.getCurrentUser();
  await umbracoUi.goToBackOffice();
  await umbracoUi.currentUserProfile.isBackOfficeMainVisible();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

  // Act
  await umbracoUi.currentUserProfile.clickCurrentUserAvatarButton();
  await umbracoUi.currentUserProfile.clickEditButton();

  // Assert
  await umbracoUi.currentUserProfile.isWorkspaceModalVisible(false);
  await expect(umbracoUi.page).toHaveURL(new RegExp('/user/edit/' + currentUser.id));
});

test('non-admin user can change UI language via the workspace modal', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const danishIsoCode = 'da';
  const userGroupData = await umbracoApi.userGroup.getByName(nonAdminUserGroupName);
  const userId = await umbracoApi.user.createDefaultUser(userName, userEmail, [userGroupData.id]);
  await umbracoApi.user.updatePassword(userId, userPassword);
  await umbracoApi.user.loginToUser(userName, userEmail, userPassword);
  await umbracoUi.goToBackOffice();
  await umbracoUi.currentUserProfile.isBackOfficeMainVisible();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

  // Act
  await umbracoUi.currentUserProfile.clickCurrentUserAvatarButton();
  await umbracoUi.currentUserProfile.clickEditButton();
  await umbracoUi.currentUserProfile.isWorkspaceModalVisible(true);
  await umbracoUi.currentUserProfile.selectUserLanguage(danishIsoCode);
  await umbracoUi.currentUserProfile.clickSaveWorkspaceButtonAndWaitForProfileUpdate();

  // Assert
  const currentUser = await umbracoApi.user.getCurrentUser();
  expect(currentUser.languageIsoCode).toBe(danishIsoCode);
});

test('non-admin user can upload avatar via the workspace modal', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupData = await umbracoApi.userGroup.getByName(nonAdminUserGroupName);
  const userId = await umbracoApi.user.createDefaultUser(userName, userEmail, [userGroupData.id]);
  await umbracoApi.user.updatePassword(userId, userPassword);
  await umbracoApi.user.loginToUser(userName, userEmail, userPassword);
  await umbracoUi.goToBackOffice();
  await umbracoUi.currentUserProfile.isBackOfficeMainVisible();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

  // Act
  await umbracoUi.currentUserProfile.clickCurrentUserAvatarButton();
  await umbracoUi.currentUserProfile.clickEditButton();
  await umbracoUi.currentUserProfile.isWorkspaceModalVisible(true);
  await umbracoUi.currentUserProfile.changePhotoWithFileChooser(avatarFilePath);
  await umbracoUi.currentUserProfile.clickSaveWorkspaceButton();

  // Assert
  await umbracoUi.currentUserProfile.isSuccessNotificationVisible();
  const currentUser = await umbracoApi.user.getCurrentUser();
  expect(currentUser.avatarUrls.length).toBeGreaterThan(0);
});

test('non-admin user can remove avatar via the workspace modal', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupData = await umbracoApi.userGroup.getByName(nonAdminUserGroupName);
  const userId = await umbracoApi.user.createDefaultUser(userName, userEmail, [userGroupData.id]);
  await umbracoApi.user.addDefaultAvatarImageToUser(userId);
  await umbracoApi.user.updatePassword(userId, userPassword);
  await umbracoApi.user.loginToUser(userName, userEmail, userPassword);
  await umbracoUi.goToBackOffice();
  await umbracoUi.currentUserProfile.isBackOfficeMainVisible();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

  // Act
  await umbracoUi.currentUserProfile.clickCurrentUserAvatarButton();
  await umbracoUi.currentUserProfile.clickEditButton();
  await umbracoUi.currentUserProfile.isWorkspaceModalVisible(true);
  await umbracoUi.currentUserProfile.clickRemovePhotoButton();
  await umbracoUi.currentUserProfile.clickSaveWorkspaceButtonAndWaitForAvatarDelete();

  // Assert
  const currentUser = await umbracoApi.user.getCurrentUser();
  expect(currentUser.avatarUrls).toHaveLength(0);
});

test('non-admin user closing the modal without saving does not persist changes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const danishIsoCode = 'da';
  const userGroupData = await umbracoApi.userGroup.getByName(nonAdminUserGroupName);
  const userId = await umbracoApi.user.createDefaultUser(userName, userEmail, [userGroupData.id]);
  await umbracoApi.user.updatePassword(userId, userPassword);
  await umbracoApi.user.loginToUser(userName, userEmail, userPassword);
  await umbracoUi.goToBackOffice();
  await umbracoUi.currentUserProfile.isBackOfficeMainVisible();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  const originalUser = await umbracoApi.user.getCurrentUser();

  // Act
  await umbracoUi.currentUserProfile.clickCurrentUserAvatarButton();
  await umbracoUi.currentUserProfile.clickEditButton();
  await umbracoUi.currentUserProfile.isWorkspaceModalVisible(true);
  await umbracoUi.currentUserProfile.selectUserLanguage(danishIsoCode);
  await umbracoUi.currentUserProfile.clickCloseWorkspaceButton();

  // Assert
  const currentUser = await umbracoApi.user.getCurrentUser();
  expect(currentUser.languageIsoCode).toBe(originalUser.languageIsoCode);
});
