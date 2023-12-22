import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Telemetry tests', () => {

  test.beforeEach(async ({umbracoApi}, testInfo) => {
    await umbracoApi.telemetry.setLevel("Basic");
  });

  test.afterEach(async ({umbracoApi}, testInfo) => {
    await umbracoApi.telemetry.setLevel("Basic");
  });

  test('can change telemetry level', async ({page, umbracoApi, umbracoUi}) => {
    const expectedLevel = "Minimal";

    await page.goto(umbracoApi.baseUrl + '/umbraco');

    // Selects minimal as the telemetry level
    // TODO: USE THE TELEMETRY UIHELPER WHEN IT IS IMPLEMENTED, NOT THE TEMPLATE
    await umbracoUi.template.goToSection(ConstantHelper.sections.settings);
    await page.getByRole('tab', { name: 'Settings' }).click();
    await page.getByRole('tab', {name: 'Telemetry Data'}).click();
    await page.locator('[name="telemetryLevel"] >> input[id=input]').fill('1');
    await page.getByLabel('Save').click();
    // Assert
    // UI
    await page.reload();
    await expect(page.locator('[name="telemetryLevel"] >> input[id=input]')).toHaveValue('1');
    // API
    expect(await umbracoApi.telemetry.getLevel() == expectedLevel).toBeTruthy();
  });
});
