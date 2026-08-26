import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

let indexAlias = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  const indexes = await umbracoApi.searchManagement.getAllIndexes();
  indexAlias = indexes.items[0].indexAlias;

  await umbracoUi.goToBackOffice();
  await umbracoUi.searchManagement.goToSearchTreeItem();
  await umbracoUi.searchManagement.goToIndexWithAlias(indexAlias);
});

test('can see the index statistics', async ({umbracoUi}) => {
  // Assert
  await umbracoUi.searchManagement.isStatsBoxVisible();
  await umbracoUi.searchManagement.doesStatsBoxContainText(indexAlias);
  await umbracoUi.searchManagement.doesStatsBoxContainText('Provider Name');
  await umbracoUi.searchManagement.doesStatsBoxContainText('Document Count');
});

test('can rebuild the index', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.searchManagement.clickRebuildIndexEntityAction();

  // Assert
  await umbracoUi.searchManagement.doesRebuildConfirmModalHaveText('Rebuild Search Index');
  await umbracoUi.searchManagement.doesRebuildConfirmModalHaveText('Are you sure you want to rebuild the search index');

  // Act - confirm the rebuild
  await umbracoUi.searchManagement.clickConfirmRebuildButtonAndWaitForResponse();

  // Completion is asserted via the API - the "IndexRebuildCompleted" server event didn't reach the browser in manual testing.
  await umbracoUi.searchManagement.doesRebuildStartedNotificationHaveText(`"${indexAlias}" has started`);

  await expect
    .poll(
      async () => (await umbracoApi.searchManagement.getIndex(indexAlias)).healthStatus,
      {timeout: ConstantHelper.timeout.pageLoad},
    )
    .not.toBe('Rebuilding');

  await umbracoUi.reloadPage();
  await umbracoUi.searchManagement.isStatsBoxVisible();
  const healthStatusText = await umbracoUi.searchManagement.getStatsBoxHealthStatusText();
  expect(healthStatusText).not.toContain('Rebuilding');
});
