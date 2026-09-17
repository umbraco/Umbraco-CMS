import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// Raised from the 60s default - chained index polls would otherwise hit the test timeout first.
test.describe.configure({timeout: 120000});

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

test('can refresh the index list', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const indexes = await umbracoApi.searchManagement.getAllIndexes();
  const indexAlias = indexes.items[0].indexAlias;

  // Act
  await umbracoUi.searchManagement.clickRefreshListButtonAndWaitForReload();

  // Assert
  await umbracoUi.searchManagement.isIndexRowVisible(indexAlias);
});
