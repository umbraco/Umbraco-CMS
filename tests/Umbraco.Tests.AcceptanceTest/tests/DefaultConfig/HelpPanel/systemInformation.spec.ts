import {ConstantHelper, test, UiHelpers} from '@umbraco/playwright-testhelpers';
import {expect, Page} from "@playwright/test";

test.describe('System Information', () => {
  const enCulture = "en-US";
  const dkCulture = "da-DK";

  test.beforeEach(async ({page, umbracoApi}) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
    await umbracoApi.users.setCurrentLanguage(enCulture);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.users.setCurrentLanguage(enCulture);
  });

  async function openSystemInformation(page: Page, umbracoUi : UiHelpers) {
    //We have to wait for page to load, if the site is slow
    await umbracoUi.clickElement(umbracoUi.getGlobalHelp());
    await expect(page.locator('.umb-help-list-item').last()).toBeVisible();
    await umbracoUi.clickElement(page.locator('.umb-help-list-item').last());
    await page.locator('.umb-drawer-content').scrollIntoViewIfNeeded();
  }

  test('Check System Info Displays', async ({page, umbracoApi, umbracoUi}) => {
    await openSystemInformation(page, umbracoUi);
    await expect(page.locator('.table').locator('tr')).toHaveCount(14);
    await expect(await page.locator("tr", {hasText: "Current Culture"})).toContainText(enCulture);
    await expect(await page.locator("tr", {hasText: "Current UI Culture"})).toContainText(enCulture);
  });

  test('Checks language displays correctly after switching', async ({page, umbracoApi, umbracoUi}) => {
    //Navigate to edit user and change language
    await umbracoUi.clickElement(umbracoUi.getGlobalUser());
    await page.locator('[alias="editUser"]').click();
    await page.locator('[name="culture"]').selectOption({value: "string:da-DK"});
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save), {force: true});
    await umbracoUi.isSuccessNotificationVisible();

    await openSystemInformation(page, umbracoUi);
    
    //Assert
    await expect(await page.locator("tr", {hasText: "Current Culture"})).toContainText(dkCulture);
    await expect(await page.locator("tr", {hasText: "Current UI Culture"})).toContainText(dkCulture);
    
    // Close the help panel
    await page.locator('.umb-button__content').last().click();
  });
  });
