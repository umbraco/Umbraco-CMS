import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Log Viewer tests', () => {

  test('can search', async ({page, umbracoApi, umbracoUi, request}) => {
    // Arrange
    const startTelemetryLevel = await umbracoApi.telemetry.getLevel();
    const telemetryLevel = "Minimal";
    await umbracoApi.telemetry.setLevel(telemetryLevel);

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSettingsTreeItem('Log Viewer');
    await page.getByRole('tab', {name: 'Search'}).click({force: true});
    await page.getByPlaceholder('Search logs...').fill(telemetryLevel);

    // Assert
    // Checks if there is a log with the telemetry level to minimal
    await expect(page.getByRole('group').first()).toContainText('Telemetry level set to "' + telemetryLevel + '"');

    // Clean
    await umbracoApi.telemetry.setLevel(startTelemetryLevel);
  });

  test('can change the search log level', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const logInformation = await umbracoApi.logViewer.getLevelCount();
    const expectedLogCount = Math.min(logInformation.information, 100);

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSettingsTreeItem('Log Viewer');
    await page.getByRole('tab', {name: 'Search'}).click({force: true});
    await page.getByRole('button', {name: 'Select log levels'}).click();
    await page.locator('.log-level-menu-item').getByText('Information').click();

    // Assert
    // Check if the search log level indicator is visible
    await expect(page.locator('.log-level-button-indicator', {hasText: 'Information'})).toBeVisible();
    // Check if the log count matches the expected count
    await expect(page.locator('umb-log-viewer-message').locator('umb-log-viewer-level-tag', {hasText: 'Information'})).toHaveCount(expectedLogCount);
  });

  test('can create a saved search', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const searchName = 'Test';
    const search = 'Acquiring MainDom';
    await umbracoApi.logViewer.deleteSavedSearch(searchName);

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSettingsTreeItem('Log Viewer');
    await page.getByRole('tab', {name: 'Search'}).click({force: true});
    await page.getByPlaceholder('Search logs...').fill(search);
    await page.getByLabel("Save search").click({force: true});
    await page.getByLabel("Search name").fill(searchName);
    await page.locator('uui-dialog-layout').getByLabel("Save search").click();

    // Assert
    // Checks if the saved search is visible in the UI
    await page.getByRole('tab', {name: 'Overview'}).click({force: true});
    await expect(page.locator('#saved-searches').getByLabel(searchName, {exact: true})).toBeVisible();
    expect(umbracoApi.logViewer.doesSavedSearchExist(searchName)).toBeTruthy();

    // Clean
    await umbracoApi.logViewer.deleteSavedSearch(searchName);
  });

  test('can create a complex saved search', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const searchName = 'ComplexTest';
    const search = "@Level='Fatal' or @Level='Error' or @Level='Warning'";
    await umbracoApi.logViewer.deleteSavedSearch(searchName);
    const logInformation = await umbracoApi.logViewer.getLevelCount();
    const expectedLogCountFatal = logInformation.fatal;
    const expectedLogCountError = logInformation.error;
    const expectedLogCountWarning = logInformation.warning;

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSettingsTreeItem('Log Viewer');
    await page.getByRole('tab', {name: 'Search'}).click({force: true});
    await page.getByPlaceholder('Search logs...').fill(search);
    // Checks if the complex search works before saving it.
    await expect(page.locator('umb-log-viewer-message').locator('umb-log-viewer-level-tag', {hasText: 'Fatal'})).toHaveCount(expectedLogCountFatal);
    await expect(page.locator('umb-log-viewer-message').locator('umb-log-viewer-level-tag', {hasText: 'Error'})).toHaveCount(expectedLogCountError);
    await expect(page.locator('umb-log-viewer-message').locator('umb-log-viewer-level-tag', {hasText: 'Warning'})).toHaveCount(expectedLogCountWarning);
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

  test('can delete a saved search', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const searchName = 'ToBeDeletedSearchName';
    const search = 'Acquiring MainDom';
    await umbracoApi.logViewer.deleteSavedSearch(searchName);
    await umbracoApi.logViewer.createSavedSearch(searchName, search);

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSettingsTreeItem('Log Viewer');
    await page.getByRole('tab', {name: 'Search'}).click({force: true});
    await page.getByLabel('Saved searches').click();
    await page.locator('li').filter({hasText: searchName + ' ' + search}).getByLabel('Remove saved search').click({force: true});

    // Assert
    // Checks if the saved search is visible in the UI
    await page.getByRole('tab', {name: 'Overview'}).click({force: true});
    await expect(page.locator('#saved-searches').getByLabel(searchName)).not.toBeVisible();
    expect(await umbracoApi.logViewer.doesSavedSearchExist(searchName)).toBeFalsy();
  });

  test('can expand a log entry', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const search = "@MessageTemplate='Acquiring MainDom.'";

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSettingsTreeItem('Log Viewer');
    await page.getByRole('tab', {name: 'Search'}).click({force: true});
    await page.getByPlaceholder('Search logs...').fill(search);
    await page.getByRole('group').locator('#message').first().click();

    // Assert
    await expect(page.getByRole('list').getByText('Acquiring MainDom.')).toBeVisible();
  });

  // Currently only works if the user is using the locale 'en-US' otherwise it will fail
  test('can sort logs by timestamp', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const locale = 'en-US';
    const options: Intl.DateTimeFormatOptions = {
      year: 'numeric',
      month: 'numeric',
      day: 'numeric',
      hour: 'numeric',
      minute: 'numeric',
      second: 'numeric',
      hour12: true,
    };

    //Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSettingsTreeItem('Log Viewer');
    await page.getByRole('tab', {name: 'Search'}).click({force: true});
    // Sorts logs by timestamp
    await page.getByLabel('Sort logs').click();
    // Gets the last log from the log viewer
    const lastLog = await umbracoApi.logViewer.getLog(0, 1, "Descending");
    const dateToFormat = new Date(lastLog.items[0].timestamp);
    const lastLogTimestamp = new Intl.DateTimeFormat(locale, options).format(dateToFormat);

    // Assert
    await expect(page.locator('umb-log-viewer-message').locator('[id="timestamp"]').first()).toContainText(lastLogTimestamp);
  });

  // Will fail if there is not enough logs.
  test('can use pagination', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const secondPageLogs = await umbracoApi.logViewer.getLog(100, 100, 'Ascending');
    const firstLogOnSecondPage = secondPageLogs.items[0].renderedMessage;

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSettingsTreeItem('Log Viewer');
    await page.getByRole('tab', {name: 'Search'}).click({force: true});
    await page.getByLabel('Go to page 2', {exact: true}).click();

    // Assert
    await expect(page.locator('umb-log-viewer-message').locator('[id="message"]').first()).toContainText(firstLogOnSecondPage);
    // TODO: Remove the comment below when the issue is resolved.
    // At the time this test was created, the UI only highlights page 1. Uncomment the line below when the issue is resolved.
    // await expect(page.getByLabel('Pagination navigation. Current page: 2.', {exact: true})).toBeVisible();
  });

  test('can use a saved search', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const searchName = 'TestSearch';
    const search = "StartsWith(@MessageTemplate, 'Acquiring MainDom.')";
    await umbracoApi.logViewer.deleteSavedSearch(searchName);
    await umbracoApi.logViewer.createSavedSearch(searchName, search);

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSettingsTreeItem('Log Viewer');
    await page.locator('#saved-searches').getByLabel(searchName).click();

    // Assert
    // Checks if the search has the correct search value
    await expect(page.getByPlaceholder('Search logs...')).toHaveValue(search);
    // Checks if the saved search found the correct logs
    await expect(page.getByRole('group').locator('#message', {hasText: 'Acquiring MainDom.'}).first()).toBeVisible();

    // Clean
    await umbracoApi.logViewer.deleteSavedSearch(searchName);
  });
});
