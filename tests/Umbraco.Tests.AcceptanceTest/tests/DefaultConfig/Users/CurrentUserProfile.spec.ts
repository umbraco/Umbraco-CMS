import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';

const userPassword = '0123456789';
let testUserCookieAndToken = {cookie: "", accessToken: "", refreshToken: ""};

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser(testUserCookieAndToken.cookie, testUserCookieAndToken.accessToken, testUserCookieAndToken.refreshToken);
});

const userGroups = ['Administrators', 'Editors', 'Sensitive data', 'Translators', 'Writers'];
for (const userGroup of userGroups) {
  test(`${userGroup} user can access the user profile and change the password`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const crypto = require('crypto');
    const userName = 'TestUser' + crypto.randomUUID();
    const userEmail = userName + '@test.com';
    const newPassword = 'TestNewPassword';
    const userGroupData = await umbracoApi.userGroup.getByName(userGroup);
    const userId = await umbracoApi.user.createDefaultUser(userName, userEmail, [userGroupData.id]);
    await umbracoApi.user.updatePassword(userId, userPassword);
    testUserCookieAndToken = await umbracoApi.user.loginToUser(userName, userEmail, userPassword);
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
