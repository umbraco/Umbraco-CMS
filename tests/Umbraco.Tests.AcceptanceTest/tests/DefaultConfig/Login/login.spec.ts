import {expect } from '@playwright/test';
import {test} from "@umbraco/playwright-testhelpers";
test.describe('Login', () => {

  test.beforeEach(async ({ page, umbracoApi }, testInfo) => {
    await umbracoApi.report.report(testInfo);
    await page.goto(process.env.URL + '/umbraco/login');
  });

  test('Login page is shown when not logged in', async ({page}) => {
    await page.goto(process.env.URL + '/umbraco');
    await page.waitForURL(process.env.URL + '/umbraco/login');
  });

  test('Login with correct username and password', async ({page}) => {

    let error = page.locator('.text-error');
    await expect(error).toBeHidden();

    // Action
    await page.fill('#username-input input', process.env.UMBRACO_USER_LOGIN);
    await page.fill('#password-input input', process.env.UMBRACO_USER_PASSWORD);
    await page.locator('#umb-login-button').click();
    await page.waitForURL(process.env.URL + '/umbraco#/content');

    // Assert
    let authApp = await page.locator('umb-auth');
    await expect(authApp).toHaveCount(0);
  });

  test('Login with correct username but wrong password', async ({page}) => {
    const username = process.env.UMBRACO_USER_LOGIN;
    const password = 'wrong';

    // Precondition«
    let error = page.locator('.text-error');
    await expect(error).toBeHidden();

    // Action
    await page.fill('#username-input input', username);
    await page.fill('#password-input input', password);
    await page.locator('#umb-login-button').click();

    // Assert
    let usernameField = await page.locator('#username-input');
    let passwordField = await page.locator('#password-input');
    await expect(error).toBeVisible();
    await expect(usernameField).toBeVisible();
    await expect(passwordField).toBeVisible();
  });

  test('Login with wrong username and wrong password', async ({page}) => {
    const username = 'wrong-username@example.com';
    const password = 'wrong';

    // Precondition
    let error = page.locator('.text-error');
    await expect(error).toBeHidden();

    // Action
    await page.fill('#username-input input', username);
    await page.fill('#password-input input', password);
    await page.locator('#umb-login-button').click();

    // Assert
    let usernameField = await page.locator('#username-input');
    let passwordField = await page.locator('#password-input');
    await expect(error).toBeVisible();
    await expect(usernameField).toBeVisible();
    await expect(passwordField).toBeVisible();
  });
});
