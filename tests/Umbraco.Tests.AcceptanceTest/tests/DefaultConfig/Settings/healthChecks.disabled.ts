import { test, ApiHelpers, UiHelpers, ConstantHelper } from '@umbraco/playwright-testhelpers';
import { expect } from "@playwright/test";

test.describe('Health Checks', () => {
    test.beforeEach(async ({ page, umbracoApi }) => {
        await umbracoApi.login();
    });

    test('Can check all groups', async ({ page, umbracoApi, umbracoUi }) => {

        await umbracoUi.goToSection("settings");

        await page.locator('[data-element="tab-settingsHealthCheck"]').click();

        await page.waitForSelector('.umb-panel-group__details-status-actions > umb-button > div > button');
        await page.click('.umb-panel-group__details-status-actions > umb-button > div > button');
		
        await expect(await page.locator('text=Configuration 2 failed')).toBeVisible();
        await expect(await page.locator('text=Data Integrity 2 passed')).toBeVisible();
        await expect(await page.locator('text=Live Environment 1 failed')).toBeVisible();
        await expect(await page.locator('text=Permissions 4 passed')).toBeVisible();
        await expect(await page.locator('text=Security 2 passed 2 warning 5 failed')).toBeVisible();
        await expect(await page.locator('text=Services 1 failed')).toBeVisible();

    });
});
