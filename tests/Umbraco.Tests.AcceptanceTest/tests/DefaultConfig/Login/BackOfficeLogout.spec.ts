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
  await expect.poll(() => umbracoApi.user.getCurrentUserStatus()).toBe(401);
  // Let the logout navigation settle before re-navigating, or the in-flight cookie-clear is
  // interrupted and the still-valid session silently re-authorises back into the backoffice.
  await umbracoUi.page.waitForURL(/\/umbraco\/logout/, {timeout: ConstantHelper.timeout.navigation});
  // Deterministic settle: retry re-navigating until the login page renders, rather than a SPA-flaky
  // networkidle wait (background polling can hold the network open indefinitely).
  await expect(async () => {
    await umbracoUi.goToBackOffice();
    await umbracoUi.login.isLoginPageVisible();
  }).toPass();
});
