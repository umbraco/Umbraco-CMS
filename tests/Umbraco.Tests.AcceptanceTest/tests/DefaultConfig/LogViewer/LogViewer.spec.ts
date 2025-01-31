import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

let startTelemetryLevel = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.logViewer.goToSettingsTreeItem('Log Viewer');
  startTelemetryLevel = await umbracoApi.telemetry.getLevel();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.telemetry.setLevel(startTelemetryLevel);
});

test('can search', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const telemetryLevel = 'Minimal';
  await umbracoApi.telemetry.setLevel(telemetryLevel);

  // Act
  await umbracoUi.logViewer.clickSearchButton();
  await umbracoUi.logViewer.waitUntilLoadingSpinnerInvisible();
  await umbracoUi.logViewer.enterSearchKeyword(telemetryLevel);
  await umbracoUi.logViewer.waitUntilLoadingSpinnerInvisible();

  // Assert
  // Checks if there is a log with the telemetry level to minimal
  await umbracoUi.logViewer.doesFirstLogHaveMessage('Telemetry level set to "' + telemetryLevel + '"');

});

test('can change the search log level', async ({umbracoUi}) => {
  // Arrange
  const logLevel = 'Information';

  // Act
  await umbracoUi.logViewer.clickSearchButton();
  await umbracoUi.logViewer.waitUntilLoadingSpinnerInvisible();
  await umbracoUi.logViewer.selectLogLevel(logLevel);
  await umbracoUi.logViewer.waitUntilLoadingSpinnerInvisible();

  // Assert
  // Check if the search log level indicator is visible
  await umbracoUi.logViewer.doesLogLevelIndicatorDisplay(logLevel);
});

test('can create a saved search', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const searchName = 'TestSavedSearch';
  const search = 'test saved search';
  await umbracoApi.logViewer.deleteSavedSearch(searchName);

  // Act
  await umbracoUi.logViewer.clickSearchButton();
  await umbracoUi.logViewer.waitUntilLoadingSpinnerInvisible();
  await umbracoUi.logViewer.enterSearchKeyword(search);
  await umbracoUi.logViewer.waitUntilLoadingSpinnerInvisible();
  await umbracoUi.logViewer.saveSearch(searchName);

  // Assert
  // Checks if the saved search is visible in the UI
  await umbracoUi.logViewer.clickOverviewButton();
  await expect(umbracoUi.logViewer.checkSavedSearch(searchName)).toBeVisible();
  expect(umbracoApi.logViewer.doesSavedSearchExist(searchName)).toBeTruthy();

  // Clean
  await umbracoApi.logViewer.deleteSavedSearch(searchName);
});

// TODO: unskip, currently flaky
test.skip('can create a complex saved search', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const searchName = 'ComplexTest';
  const search = "@Level='Fatal' or @Level='Error' or @Level='Warning'";
  await umbracoApi.logViewer.deleteSavedSearch(searchName);
  const logInformation = await umbracoApi.logViewer.getLevelCount();
  const expectedLogCountFatal = logInformation.fatal;
  const expectedLogCountError = logInformation.error;
  const expectedLogCountWarning = logInformation.warning;

  // Act
  await umbracoUi.logViewer.clickSearchButton();
  await umbracoUi.logViewer.waitUntilLoadingSpinnerInvisible();
  await umbracoUi.logViewer.enterSearchKeyword(search);
  await umbracoUi.logViewer.waitUntilLoadingSpinnerInvisible();
  // Checks if the complex search works before saving it.
  await umbracoUi.logViewer.doesLogLevelCountMatch('Fatal', expectedLogCountFatal);
  await umbracoUi.logViewer.doesLogLevelCountMatch('Error', expectedLogCountError);
  await umbracoUi.logViewer.doesLogLevelCountMatch('Warning', expectedLogCountWarning);
  await umbracoUi.logViewer.saveSearch(searchName);

  // Assert
  // Checks if the saved search is visible in the UI
  await umbracoUi.logViewer.clickOverviewButton();
  await expect(umbracoUi.logViewer.checkSavedSearch(searchName)).toBeVisible();
  expect(umbracoApi.logViewer.doesSavedSearchExist(searchName)).toBeTruthy();

  // Clean
  await umbracoApi.logViewer.deleteSavedSearch(searchName);
});

test('can delete a saved search', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const searchName = 'ToBeDeletedSearchName';
  const search = 'Acquiring MainDom';
  await umbracoApi.logViewer.deleteSavedSearch(searchName);
  await umbracoApi.logViewer.createSavedSearch(searchName, search);

  // Act
  await umbracoUi.logViewer.clickSearchButton();
  await umbracoUi.logViewer.waitUntilLoadingSpinnerInvisible();
  await umbracoUi.logViewer.clickSavedSearchesButton();
  await umbracoUi.logViewer.removeSavedSearchByName(searchName + ' ' + search);
  await umbracoUi.logViewer.clickDeleteButton();

  // Assert
  // Checks if the saved search is visible in the UI
  await umbracoUi.logViewer.clickOverviewButton();
  await expect(umbracoUi.logViewer.checkSavedSearch(searchName)).not.toBeVisible();
  expect(await umbracoApi.logViewer.doesSavedSearchExist(searchName)).toBeFalsy();
});

test('can expand a log entry', async ({umbracoUi}) => {
  // Arrange
  const search = "StartsWith(@MessageTemplate, 'The token')";

  // Act
  await umbracoUi.logViewer.clickSearchButton();
  await umbracoUi.logViewer.waitUntilLoadingSpinnerInvisible();
  await umbracoUi.logViewer.enterSearchKeyword(search);
  await umbracoUi.logViewer.waitUntilLoadingSpinnerInvisible();
  await umbracoUi.logViewer.clickFirstLogSearchResult();

  // Assert
  await umbracoUi.logViewer.doesDetailedLogHaveText('The token');
});

// Currently only works if the user is using the locale 'en-US' otherwise it will fail
test.skip('can sort logs by timestamp', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.logViewer.clickSearchButton();
  // Sorts logs by timestamp
  await umbracoUi.logViewer.clickSortLogByTimestampButton();
  // Gets the last log from the log viewer
  const lastLog = await umbracoApi.logViewer.getLog(0, 1, 'Descending');
  const dateToFormat = new Date(lastLog.items[0].timestamp);
  const lastLogTimestamp = new Intl.DateTimeFormat(locale, options).format(dateToFormat);

  // Assert
  await umbracoUi.logViewer.doesFirstLogHaveTimestamp(lastLogTimestamp);
});

// Will fail if there is not enough logs.
test.skip('can use pagination', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondPageLogs = await umbracoApi.logViewer.getLog(100, 100, 'Ascending');
  const firstLogOnSecondPage = secondPageLogs.items[0].renderedMessage;

  // Act
  await umbracoUi.logViewer.clickSearchButton();
  await umbracoUi.logViewer.clickPageNumber(2);

  // Assert
  await umbracoUi.logViewer.doesFirstLogHaveMessage(firstLogOnSecondPage);
  // TODO: Remove the comment below when the issue is resolved.
  // At the time this test was created, the UI only highlights page 1. Uncomment the line below when the issue is resolved.
  // await expect(page.getByLabel('Pagination navigation. Current page: 2.', {exact: true})).toBeVisible();
});

test('can use a saved search', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const searchName = 'TestSearch';
  const search = "StartsWith(@MessageTemplate, 'The token')";
  await umbracoApi.logViewer.deleteSavedSearch(searchName);
  await umbracoApi.logViewer.createSavedSearch(searchName, search);
  await umbracoUi.logViewer.goToSettingsTreeItem('Log Viewer');

  // Act
  await umbracoUi.waitForTimeout(2000);
  await umbracoUi.logViewer.clickSavedSearchByName(searchName);
  await umbracoUi.logViewer.waitUntilLoadingSpinnerInvisible();

  // Assert
  // Checks if the search has the correct search value
  await umbracoUi.logViewer.doesSearchBoxHaveValue(search);
  // Checks if the saved search found the correct logs
  await umbracoUi.logViewer.doesFirstLogHaveMessage('The token');

  // Clean
  await umbracoApi.logViewer.deleteSavedSearch(searchName);
});
