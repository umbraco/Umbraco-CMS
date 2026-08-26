import {test} from '@umbraco/acceptance-test-helpers';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.searchManagement.goToSearchTreeItem();
});

test('can see the index list with the expected columns and indexes', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const indexes = await umbracoApi.searchManagement.getAllIndexes();

  // Assert
  await umbracoUi.searchManagement.doesIndexTableHaveColumnHeaders(['Alias', 'Health Status', 'Document Count']);
  for (const index of indexes.items) {
    await umbracoUi.searchManagement.isIndexRowVisible(index.indexAlias);
    await umbracoUi.searchManagement.doesIndexRowContainText(index.indexAlias, index.healthStatus);
  }
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
