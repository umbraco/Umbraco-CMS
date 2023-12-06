import { test } from '@umbraco/playwright-testhelpers';
import { expect } from '@playwright/test';

test.describe("Help panel links", () => {
    const enCulture = "en-US";
  
    test.beforeEach(async ({ umbracoApi }, testInfo) => {
      await umbracoApi.report.report(testInfo);
      await umbracoApi.login();
      await umbracoApi.users.setCurrentLanguage(enCulture);
    });
  
    test.afterEach(async ({ umbracoApi }) => {
      await umbracoApi.users.setCurrentLanguage(enCulture);
    });

test("Check the youtube link works as expected", async ({ page, umbracoUi }) => {
    // Action
    await umbracoUi.clickElement(umbracoUi.getGlobalHelp());
    let watchVideoLink = await page.locator('[key="help_umbracoLearningBase"]');
    await watchVideoLink.click();

    // Wait for a popup event
    const youtubeConsent = page.waitForEvent("popup");
    const youtubePopup = await youtubeConsent;

    if (youtubePopup) {
      // If the YouTube consent popup is displayed, interact with it
      const rejectAllButton = await youtubePopup.locator('[aria-label="Reject all"]').first();
      await rejectAllButton.waitFor({ state: "visible" });
      await rejectAllButton.click();

      // Assert the URL of the popup
      await expect(youtubePopup).toHaveURL(/.*UmbracoLearningBase/);
      await youtubePopup.close();

    } else {
      await expect(page).toHaveURL(/.*UmbracoLearningBase/);
      await page.close();
    }
  });
  test("Check the Our Umbraco link works as expected", async ({ page, umbracoUi }) => {
    // Action
    await umbracoUi.clickElement(umbracoUi.getGlobalHelp());
    let ourUmbracoLink = await page.locator('[key="help_umbracoForum"]');
    await ourUmbracoLink.click();
    let ourUmbraco = page.waitForEvent("popup");
    let ourUmbracoPopup = await ourUmbraco;

    //Assert
    await expect(ourUmbracoPopup).toHaveURL(/.*our.umbraco.com/);
    await ourUmbracoPopup.close();
  });
});