import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.searchManagement.goToSearchTreeItem();
});

test('can see the index list with the expected columns and indexes', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Assert
  await umbracoUi.searchManagement.doesIndexTableHaveColumnHeaders(['Alias', 'Health Status', 'Document Count']);

  // The table does not update by itself, so refresh and re-read together until both agree on index state
  await expect(async () => {
    await umbracoUi.searchManagement.clickRefreshListButtonAndWaitForReload();
    const indexes = await umbracoApi.searchManagement.getAllIndexes();
    expect(indexes.items.length).toBeGreaterThan(0);
    for (const index of indexes.items) {
      await umbracoUi.searchManagement.isIndexRowVisible(index.indexAlias);
      await umbracoUi.searchManagement.doesIndexRowContainText(index.indexAlias, index.healthStatus);
    }
  }).toPass({timeout: ConstantHelper.timeout.veryLong});
});

test('can rebuild an index from its row in the index list', async ({umbracoUi}) => {
  // Arrange - not Umb_Content, which the other search specs depend on
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
