import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

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

  // We wait until we are sure that the Telemetry level has been saved before we continue.
  await Promise.all([
    page.waitForResponse(resp => resp.url().includes(umbracoApi.baseUrl + '/umbraco/management/api/v1/telemetry/level') && resp.status() === 200),
    await umbracoUi.telemetryData.clickSaveButton()
  ]);

  // Assert
  // UI
  await umbracoUi.reloadPage();
  await expect(page.locator('[name="telemetryLevel"] >> input[id=input]')).toHaveValue(levelValue, {timeout: 20000});
  // await umbracoUi.telemetryData.doesTelemetryDataLevelHaveValue(levelValue);

  // API
  expect(await umbracoApi.telemetry.getLevel() == expectedLevel).toBeTruthy();
});
