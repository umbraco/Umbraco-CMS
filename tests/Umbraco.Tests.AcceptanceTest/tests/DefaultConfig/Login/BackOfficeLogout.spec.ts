import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

test('can log out via the user menu', {tag: '@smoke'}, async ({umbracoUi, umbracoApi}) => {
  // Arrange
  await umbracoUi.goToBackOffice();
  await umbracoUi.login.isBackOfficeMainVisible();

  // Act
  await umbracoUi.login.clickCurrentUserAvatarButton();
  await umbracoUi.login.clickLogoutButtonAndWaitForUserLogout();

  // Assert
  expect(await umbracoApi.user.getCurrentUserStatus()).toBe(401);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium); // Wait for the logout process to complete and the login page to be visible
  await umbracoUi.goToBackOffice();
  await umbracoUi.login.isLoginPageVisible();
  await umbracoUi.login.isBackOfficeMainVisible(false);
});