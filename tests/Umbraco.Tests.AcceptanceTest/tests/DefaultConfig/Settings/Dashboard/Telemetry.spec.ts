import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

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

  await umbracoUi.telemetryData.clickSaveButtonAndWaitForTelemetryLevelToBeSaved();

  // Assert
  // UI
  await umbracoUi.reloadPage();
  await umbracoUi.telemetryData.doesTelemetryDataLevelHaveValue(levelValue);

  // API
  expect(await umbracoApi.telemetry.getLevel() == expectedLevel).toBeTruthy();
});
