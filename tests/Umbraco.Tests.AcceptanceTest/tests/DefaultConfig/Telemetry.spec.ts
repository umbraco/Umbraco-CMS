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

  test('can change telemetry level', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedLevel = "Minimal";
    const levelValue = "1";
    await umbracoUi.telemetryData.clickTelemetryDataTab();
    await umbracoUi.telemetryData.changeTelemetryDataLevelValue(levelValue);
    await umbracoUi.telemetryData.clickSaveButton();
    // Assert
    // UI
    await umbracoUi.reloadPage();
    await umbracoUi.telemetryData.doesTelemetryDataLevelHaveValue(levelValue)
    // API
    expect(await umbracoApi.telemetry.getLevel() == expectedLevel).toBeTruthy();
  });
});
