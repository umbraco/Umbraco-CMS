import {test as setup, expect} from '@playwright/test';
import {STORAGE_STATE} from '../playwright.config'

// Update this setup with test ids in the future. Maybe also find better locators in general
setup('authenticate', async ({page}) => {
  await page.goto(process.env.URL + '/umbraco');

  await page.getByRole('textbox', {name: 'Email'}).fill(process.env.UMBRACO_USER_LOGIN);
  await page.getByRole('textbox', {name: 'Password'}).fill(process.env.UMBRACO_USER_PASSWORD);
  await page.getByRole('button', {name: 'Login'}).click();

  // Assert
  // CREATE A UI TEST HELPER FOR DATA ELEMENTS
  await expect(page.locator('[data-element="section-settings"]')).toBeVisible();
  await page.locator('[data-element="section-settings"]').click();

  await page.context().storageState({path: STORAGE_STATE});
});
