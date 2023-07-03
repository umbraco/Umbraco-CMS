import {test as setup, expect, Page} from '@playwright/test';
import {STORAGE_STATE} from '../playwright.config'

// Update this setup with test ids in the future. Maybe also find better locators in general
setup('authenticate', async ({page}) => {
  await page.goto(process.env.URL + '/umbraco');

  await page.getByRole('textbox', { name: 'Email' }).fill(process.env.UMBRACO_USER_LOGIN);
  await page.getByRole('textbox', { name: 'Password' }).fill(process.env.UMBRACO_USER_PASSWORD);
  await page.getByRole('button', {name: 'Login'}).click();

  await page.waitForURL(process.env.URL + '/umbraco');

  // Assert
  await expect(page.getByRole('tab', { name: 'Settings' })).toBeVisible();

  await page.context().storageState({path: STORAGE_STATE});
});
