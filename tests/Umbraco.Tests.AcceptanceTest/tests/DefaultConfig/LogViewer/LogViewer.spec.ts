import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Log Viewer tests', () => {

  test.beforeEach(async ({umbracoUi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoUi.logViewer.goToSettingsTreeItem('Log Viewer');
  });

  test('can search', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const startTelemetryLevel = await umbracoApi.telemetry.getLevel();
    const telemetryLevel = "Minimal";
    await umbracoApi.telemetry.setLevel(telemetryLevel);

    // Act
    await umbracoUi.logViewer.clickSearchButton();
    await umbracoUi.logViewer.enterSearchKeyword(telemetryLevel);

    // Assert
    // Checks if there is a log with the telemetry level to minimal
    await umbracoUi.waitForTimeout(1000);
    await umbracoUi.logViewer.doesFirstLogHaveMessage('Telemetry level set to "' + telemetryLevel + '"');

    // Clean
    await umbracoApi.telemetry.setLevel(startTelemetryLevel);
  });

  test('can change the search log level', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const logInformation = await umbracoApi.logViewer.getLevelCount();
    const expectedLogCount = Math.min(logInformation.information, 100);

    // Act
    await umbracoUi.logViewer.clickSearchButton();
    await umbracoUi.logViewer.selectLogLevel('Information');

    // Assert
    // Check if the search log level indicator is visible
    await umbracoUi.logViewer.doesLogLevelIndicatorDisplay('Information');
    // Check if the log count matches the expected count
    await umbracoUi.logViewer.doesLogLevelCountMatch('Information', expectedLogCount);
  });

  test('can create a saved search', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const searchName = 'TestSavedSearch';
    const search = 'test saved search';
    await umbracoApi.logViewer.deleteSavedSearch(searchName);

    // Act
    await umbracoUi.logViewer.clickSearchButton();
    await umbracoUi.logViewer.enterSearchKeyword(search);
    await umbracoUi.logViewer.saveSearch(searchName);

    // Assert
    // Checks if the saved search is visible in the UI
    await umbracoUi.logViewer.clickOverviewButton();
    await expect(umbracoUi.logViewer.checkSavedSearch(searchName)).toBeVisible();
    expect(umbracoApi.logViewer.doesSavedSearchExist(searchName)).toBeTruthy();

    // Clean
    await umbracoApi.logViewer.deleteSavedSearch(searchName);
  });

  test('can create a complex saved search', async ({umbracoApi, umbracoUi}) => {
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
    await umbracoUi.logViewer.enterSearchKeyword(search);
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
    await umbracoUi.logViewer.clickSavedSearchesButton();
    await umbracoUi.logViewer.removeSavedSearchByName(searchName + ' ' + search);

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
    await umbracoUi.logViewer.enterSearchKeyword(search);
    await umbracoUi.logViewer.clickFirstLogSearchResult();

    // Assert
    await umbracoUi.logViewer.doesDetailedLogHaveText('The token');
  });

  // Currently only works if the user is using the locale 'en-US' otherwise it will fail
  test('can sort logs by timestamp', async ({umbracoApi, umbracoUi}) => {
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
    const lastLog = await umbracoApi.logViewer.getLog(0, 1, "Descending");
    const dateToFormat = new Date(lastLog.items[0].timestamp);
    const lastLogTimestamp = new Intl.DateTimeFormat(locale, options).format(dateToFormat);

    // Assert
    await umbracoUi.logViewer.doesFirstLogHaveTimestamp(lastLogTimestamp);
  });

  // Will fail if there is not enough logs.
  test('can use pagination', async ({umbracoApi, umbracoUi}) => {
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
    // Need to reload page to get the latest saved search list after creating new saved search by api
    await umbracoUi.reloadPage();

    // Act
    await umbracoUi.logViewer.clickSavedSearchByName(searchName);

    // Assert
    // Checks if the search has the correct search value
    await umbracoUi.logViewer.doesSearchBoxHaveValue(search);
    // Checks if the saved search found the correct logs
    await umbracoUi.logViewer.doesFirstLogHaveMessage('The token');

    // Clean
    await umbracoApi.logViewer.deleteSavedSearch(searchName);
  });
});
