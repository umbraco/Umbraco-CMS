import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.searchManagement.goToSearchTreeItem();
});

test('can see the index list with the expected columns and indexes', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Assert
  await umbracoUi.searchManagement.doesIndexTableHaveColumnHeaders(['Alias', 'Health Status', 'Document Count']);

  // The table renders once on navigation and never polls, so comparing it against a separately read API
  // response compares two moments: a spec that ran earlier can still be changing index state, and the row
  // then reports the status from before that change. Refresh and re-read together until the two agree,
  // rather than sampling each once and requiring them to have agreed by luck.
  await expect(async () => {
    await umbracoUi.searchManagement.clickRefreshListButtonAndWaitForReload();
    const indexes = await umbracoApi.searchManagement.getAllIndexes();
    // Without this the loop below is vacuous: if index registration broke entirely and the API returned no
    // items, iterating none of them would assert nothing and the test would still pass.
    expect(indexes.items.length).toBeGreaterThan(0);
    for (const index of indexes.items) {
      await umbracoUi.searchManagement.isIndexRowVisible(index.indexAlias);
      await umbracoUi.searchManagement.doesIndexRowContainText(index.indexAlias, index.healthStatus);
    }
  }).toPass({timeout: ConstantHelper.timeout.veryLong});
});

test('can rebuild an index from its row in the index list', async ({umbracoUi}) => {
  // Arrange - a rebuild empties the index for a while, so use one the other search specs do not depend on
  const indexAlias = 'Umb_Members';

  // Act
  await umbracoUi.searchManagement.clickRebuildIndexActionForIndex(indexAlias);
  await umbracoUi.searchManagement.doesRebuildConfirmModalHaveText('Are you sure you want to rebuild the search index');
  await umbracoUi.searchManagement.clickConfirmRebuildButtonAndWaitForResponse();

  // Assert
  await umbracoUi.searchManagement.doesRebuildStartedNotificationHaveText(`"${indexAlias}" has started`);
});

test('can refresh the index list', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const indexes = await umbracoApi.searchManagement.getAllIndexes();
  expect(indexes.items.length).toBeGreaterThan(0);

  // Act
  await umbracoUi.searchManagement.clickRefreshListButtonAndWaitForReload();

  // Assert
  for (const index of indexes.items) {
    await umbracoUi.searchManagement.isIndexRowVisible(index.indexAlias);
  }
});
