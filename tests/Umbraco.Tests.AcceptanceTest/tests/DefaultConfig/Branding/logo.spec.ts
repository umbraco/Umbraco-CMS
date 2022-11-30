import { test } from '@umbraco/playwright-testhelpers';
import { expect } from "@playwright/test";

test.describe('Umbraco Logo Information', () => {

  test.beforeEach(async ({ page, umbracoApi }, testInfo) => {
    await umbracoApi.report.report(testInfo);
    await umbracoApi.login();
  });

  test('Check Umbraco Logo Info Displays', async ({ page }) => {
    await page.locator('.umb-app-header__logo').click();
    await expect(page.locator('.umb-app-header__logo-modal').last()).toBeVisible();
  });
});
