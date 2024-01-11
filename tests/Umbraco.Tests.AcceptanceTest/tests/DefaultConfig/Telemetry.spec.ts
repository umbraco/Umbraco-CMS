import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Telemetry tests', () => {

  test.beforeEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.telemetry.setLevel("Basic");
    await umbracoUi.goToBackOffice();
    await umbracoUi.telemetryData.goToSection(ConstantHelper.sections.settings);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.telemetry.setLevel("Basic");
  });

  test('can change telemetry level', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedLevel = "Minimal";
    const levelValue = "1";
    await umbracoUi.telemetryData.clickTelemetryDataTab();
    await umbracoUi.telemetryData.changeTelemetryDataLevelValue(levelValue);
    await umbracoUi.telemetryData.clickSaveButton();

    // Waits until the Telemetry level is saved
    await Promise.all([
      page.waitForResponse(resp => (umbracoApi.baseUrl + '/umbraco/management/api/v1/telemetry/level') && resp.status() === 200),
      await umbracoUi.telemetryData.clickSaveButton()
    ]);

    // Assert
    // UI
    // Waits until the page is reloaded and for the network response for the Telemetry level
    await Promise.all([
      page.waitForResponse(resp => (umbracoApi.baseUrl + '/umbraco/management/api/v1/telemetry/level') && resp.status() === 200),
      await umbracoUi.reloadPage()

    ]);

    await umbracoUi.telemetryData.doesTelemetryDataLevelHaveValue(levelValue);
    // API
    expect(await umbracoApi.telemetry.getLevel() == expectedLevel).toBeTruthy();
  });
});
