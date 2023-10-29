import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Log Viewer tests', () => {

  test('can search in log viewer', async ({page, umbracoApi, umbracoUi, request}) => {
    const telemetryLevel = "Minimal";
    await umbracoApi.telemetry.setLevel(telemetryLevel);

    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSection('Settings');

    // await page.getByLabel('Log Viewer').click();
    await page.getByLabel('Log Viewer').click();

    await page.getByRole('tab', {name: 'Search'}).click({force: true});

    // After TestIDs are added, please for the love of everything, update this locator
    await page.getByPlaceholder('Search logs...').fill(telemetryLevel);

    // Assert
    // Checks if there is a log with the telemetry level to minimal
    await expect(page.getByRole('group').first()).toContainText('Telemetry level set to "' + telemetryLevel + '"');
  });

  test('can change the search log level', async ({page, umbracoApi, umbracoUi}) => {
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSection('Settings');

    await page.getByLabel('Log Viewer').click();
    await page.getByRole('tab', {name: 'Search'}).click({force: true});

    await page.getByRole('button', {name: 'Select log levels'}).click();
    await page.locator('.log-level-menu-item').getByText('Information').click();

    // Assert
    // Checks if the search log level was updated
    await expect(page.locator('.log-level-button-indicator', {hasText: 'Information'})).toBeVisible();
  });

  test('can add a saved search', async ({page, umbracoApi, umbracoUi}) => {
    const searchName = 'Test';
    const search = 'Acquiring MainDom';

    await umbracoApi.logViewer.deleteSavedSearch(searchName);

    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSection('Settings');

    await page.getByLabel('Log Viewer').click();
    await page.getByRole('tab', {name: 'Search'}).click({force: true});
    await page.getByPlaceholder('Search logs...').fill(search);
    await page.getByLabel("Save search").click({force: true});

    await page.getByLabel("Search name").fill(searchName);
    await page.locator('uui-dialog-layout').getByLabel("Save search").click();

    // Assert
    // Checks if the saved search is visible in the UI
    await page.getByRole('tab', {name: 'Overview'}).click({force: true});
    await expect(page.locator('#saved-searches').getByLabel(searchName)).toBeVisible();
    expect(umbracoApi.logViewer.doesSavedSearchExist(searchName)).toBeTruthy();

    // Clean
    await umbracoApi.logViewer.deleteSavedSearch(searchName);
  });

  test('can use a saved search', async ({page, umbracoApi, umbracoUi}) => {
    const searchName = 'Test';
    const search = 'Acquiring MainDom';

    await umbracoApi.logViewer.deleteSavedSearch(searchName);

    await umbracoApi.logViewer.createSavedSearch(searchName, search);

    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSection('Settings');

    await page.getByRole('tab', {name: 'Settings'}).click();
    await page.getByLabel('Log Viewer').click();
    await page.locator('#saved-searches').getByLabel(searchName).click();

    // Assert
    await expect(page.getByPlaceholder('Search logs...')).toHaveValue(search);

    // Clean
    await umbracoApi.logViewer.deleteSavedSearch(searchName);
  });

  // Do we want this test?
  test('can update date', async ({page, umbracoApi, umbracoUi}) => {
    const searchName = 'Test';
    const search = 'Acquiring MainDom';

    await umbracoApi.logViewer.deleteSavedSearch(searchName);

    await umbracoApi.logViewer.createSavedSearch(searchName, search);

    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await page.waitForURL(umbracoApi.baseUrl + '/umbraco');

    await umbracoUi.goToSection('Settings');

    await page.getByRole('tab', {name: 'Settings'}).click();

    await page.getByLabel('Log Viewer').click();

    await page.locator('#start-date').fill("2023-07-01");

    // await page.pause();
    await page.locator('#saved-searches').getByLabel(searchName).click();

    // Assert
    await expect(page.getByPlaceholder('Search logs...')).toHaveValue(search);

    // Clean
    await umbracoApi.logViewer.deleteSavedSearch(searchName);
  });
});
