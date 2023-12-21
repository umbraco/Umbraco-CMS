import {test as setup, expect} from '@playwright/test';
import {STORAGE_STATE} from '../playwright.config';
import {ConstantHelper, UiHelpers} from "@umbraco/playwright-testhelpers";

setup('authenticate', async ({page}) => {
  const umbracoUi = new UiHelpers(page);

  await page.goto(process.env.URL + '/umbraco');
  await page.getByLabel('Email').fill(process.env.UMBRACO_USER_LOGIN);
  await page.getByLabel( 'Password', {exact: true}).fill(process.env.UMBRACO_USER_PASSWORD);
  await page.getByRole('button', {name: 'Login'}).click();

  // Assert
  await expect(page.locator('uui-tab-group').locator('[label="Settings"]')).toBeVisible({timeout: 10000});
  await umbracoUi.uiBaseLocators.goToSection(ConstantHelper.sections.settings);
  await page.context().storageState({path: STORAGE_STATE});
});
