import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Telemetry tests', () => {

  test.beforeEach(async ({page, umbracoApi}, testInfo) => {
    await umbracoApi.telemetry.setTelemetryLevel("Basic");
  });

  test.afterEach(async ({page, umbracoApi}, testInfo) => {
    await umbracoApi.telemetry.setTelemetryLevel("Basic");
  });

  test('can change telemetry level', async ({page, umbracoApi, umbracoUi}) => {
    const expectedLevel = "Minimal";

    await page.goto(umbracoApi.baseUrl + '/umbraco');

    await page.pause();

    // Selects minimal as the telemetry level
    await page.getByRole('tab', { name: 'Settings' }).click();
    await page.getByRole('tab', {name: 'Telemetry Data'}).click();
    await page.locator('[name="telemetryLevel"] >> input[id=input]').fill('1');
    await page.getByRole('button', { name: 'Save telemetry settings' }).click();

    // Assert
    // UI
    await page.reload();
    await expect(await page.locator('[name="telemetryLevel"] >> input[id=input]')).toHaveValue('1');
    // API
    await expect(await umbracoApi.telemetry.checkTelemetryLevel(expectedLevel)).toBeTruthy();
  });
});
