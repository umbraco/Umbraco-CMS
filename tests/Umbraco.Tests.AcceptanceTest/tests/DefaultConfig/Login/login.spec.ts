import { test, expect } from '@playwright/test';
test.describe('Login', () => {

    test.beforeEach(async ({ page }) => {
      // TODO: REMOVE THIS WHEN SQLITE IS FIXED
      // Wait so we don't bombard the API
      await page.waitForTimeout(1000);
      await page.goto(process.env.URL + '/umbraco');
    });
    test('Login with correct username and password', async ({page}) => {

      let error = page.locator('.text-error');
      await expect(error).toBeHidden();

      // Action
      await page.fill('#umb-username', process.env.UMBRACO_USER_LOGIN);
      await page.fill('#umb-passwordTwo', process.env.UMBRACO_USER_PASSWORD);
      await page.locator('[label-key="general_login"]').click();
      await page.waitForNavigation();

      // Assert
      await expect(page).toHaveURL(process.env.URL + '/umbraco#/content');
      let usernameField = await page.locator('#umb-username');
      let passwordField = await page.locator('#umb-passwordTwo');
      await expect(usernameField).toHaveCount(0);
      await expect(passwordField).toHaveCount(0);
    });

    test('Login with correct username but wrong password', async({page}) => {
      const username = process.env.UMBRACO_USER_LOGIN;
      const password = 'wrong';

      // Precondition
      let error = page.locator('.text-error');
      await expect(error).toBeHidden();

      // Action
      await page.fill('#umb-username', username);
      await page.fill('#umb-passwordTwo', password);
      await page.locator('[label-key="general_login"]').click();

      // Assert
      let usernameField = await page.locator('#umb-username');
      let passwordField = await page.locator('#umb-passwordTwo');
      await expect(error).toBeVisible();
      await expect(usernameField).toBeVisible();
      await expect(passwordField).toBeVisible();
    });

    test('Login with wrong username and wrong password', async({page}) => {
      const username = 'wrong-username';
      const password = 'wrong';

      // Precondition
      let error = page.locator('.text-error');
      await expect(error).toBeHidden();

      // Action
      await page.fill('#umb-username', username);
      await page.fill('#umb-passwordTwo', password);
      await page.locator('[label-key="general_login"]').click();

      // Assert
      let usernameField = await page.locator('#umb-username');
      let passwordField = await page.locator('#umb-passwordTwo');
      await expect(error).toBeVisible();
      await expect(usernameField).toBeVisible();
      await expect(passwordField).toBeVisible();
    });
});