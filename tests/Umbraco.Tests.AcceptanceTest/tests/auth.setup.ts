import {test as setup} from '@playwright/test';
import {STORAGE_STATE} from '../playwright.config';
import {ConstantHelper, UiHelpers} from "@umbraco/playwright-testhelpers";

setup('authenticate', async ({page}) => {
  const umbracoUi = new UiHelpers(page);

  await umbracoUi.goToBackOffice();
  await umbracoUi.login.enterEmail(process.env.UMBRACO_USER_LOGIN);
  await umbracoUi.login.enterPassword(process.env.UMBRACO_USER_PASSWORD);
  await umbracoUi.login.clickLoginButton();

  // Assert
  //await expect(page.locator('uui-tab-group').locator('[label="Settings"]')).toBeVisible({timeout: 10000});
  // DELETE LATER
  await umbracoUi.waitForTimeout(10000);
  await umbracoUi.login.goToSection(ConstantHelper.sections.settings);
  await page.context().storageState({path: STORAGE_STATE});
});
