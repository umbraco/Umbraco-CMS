import {expect, test as setup} from '@playwright/test';
import {STORAGE_STATE} from '../playwright.config';
import {ConstantHelper, UiHelpers} from "@umbraco/playwright-testhelpers";

setup('authenticate', async ({page}) => {
  const umbracoUi = new UiHelpers(page);

  await umbracoUi.goToBackOffice();
  // This wait is necessary to avoid the flaky test in Window
  //await umbracoUi.waitForTimeout(1000);
  //await umbracoUi.login.enterEmail(process.env.UMBRACO_USER_LOGIN);
  //await umbracoUi.login.enterPassword(process.env.UMBRACO_USER_PASSWORD);
  await expect(umbracoUi.page.locator('name="username"')).toBeVisible({timeout: 20000});
  await umbracoUi.page.locator('name="username"').fill(process.env.UMBRACO_USER_LOGIN);
  await umbracoUi.page.locator('name="password"').fill(process.env.UMBRACO_USER_LOGIN);
  await umbracoUi.login.clickLoginButton();
  await umbracoUi.login.goToSection(ConstantHelper.sections.settings);
  await page.context().storageState({path: STORAGE_STATE});
});
