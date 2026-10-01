import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const contentIndexAlias = 'Umb_Content';
const documentTypeName = 'SearchIndexListDocumentType';
const documentName = 'SearchIndexListDocument';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);

  await umbracoUi.goToBackOffice();
  await umbracoUi.searchManagement.goToSearchTreeItem();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can see the index list with the expected columns and indexes', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Assert
  await umbracoUi.searchManagement.doesIndexTableHaveColumnHeaders(['Alias', 'Health Status', 'Document Count']);

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
  // Arrange
  const indexAlias = 'Umb_Members';

  // Act
  await umbracoUi.searchManagement.clickRebuildActionForIndex(indexAlias);
  await umbracoUi.searchManagement.doesModalHaveText('Are you sure you want to rebuild the search index');
  await umbracoUi.searchManagement.clickConfirmRebuildButtonAndWaitForResponse();

  // Assert
  await umbracoUi.searchManagement.doesRebuildStartedNotificationHaveText(`"${indexAlias}" has started`);
});

test('can refresh the index list', async ({umbracoApi, umbracoUi}) => {
  test.slow();

  // Arrange
  await umbracoUi.searchManagement.isIndexRowVisible(contentIndexAlias);
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName) ?? '';
  const documentId = await umbracoApi.document.createPublishedDefaultDocument(documentName, documentTypeId);
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(contentIndexAlias, documentName, documentId);
  await expect
    .poll(async () => (await umbracoApi.searchManagement.getIndex(contentIndexAlias)).healthStatus, {timeout: ConstantHelper.timeout.pageLoad})
    .toBe('Healthy');
  const documentCount = (await umbracoApi.searchManagement.getIndex(contentIndexAlias)).documentCount;

  // Act
  await umbracoUi.searchManagement.clickRefreshListButtonAndWaitForReload();

  // Assert
  await umbracoUi.searchManagement.doesIndexRowContainText(contentIndexAlias, ` ${new Intl.NumberFormat('en-US').format(documentCount)} document`);
});
