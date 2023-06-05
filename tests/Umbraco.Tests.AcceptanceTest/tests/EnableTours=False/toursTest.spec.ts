import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import {umbracoConfig} from "../../umbraco.config";

test.describe('Test', () => {

    test.beforeEach(async ({page, umbracoApi}) => {
        await umbracoApi.login(true);
    });

    test('Check if tours exist', async ({page, umbracoApi, umbracoUi}) => {
        // Need to go to the correct page when I set skipCheckTours in login to true
        await page.goto(umbracoConfig.environment.baseUrl + '/umbraco');
        await page.locator('[data-element="global-help"]').click();
        // Assert
        await expect(await page.locator('[data-element="help-tours"]')).not.toBeVisible();
    });

});