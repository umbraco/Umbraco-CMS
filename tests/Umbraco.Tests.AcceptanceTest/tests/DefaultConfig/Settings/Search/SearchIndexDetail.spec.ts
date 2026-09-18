import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// Raised from the 60s default - chained index polls would otherwise hit the test timeout first.
test.describe.configure({timeout: 120000});

// The rebuild below is a global side effect, so which index it hits must not depend on the order the API
// returns them in.
const indexAlias = 'Umb_Content';
const documentTypeName = 'SearchIndexDetailDocumentType';
const documentName = 'SearchIndexDetailDocument';

let documentId = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);

  const indexes = await umbracoApi.searchManagement.getAllIndexes();
  expect(indexes.items.some((index) => index.indexAlias === indexAlias), `the ${indexAlias} index must exist`).toBeTruthy();

  // An empty index reports "Empty", a sibling of "Healthy" rather than a subset, and a rebuild of it stays
  // "Empty" forever - so publish a document for the index to be healthy about and to repopulate with.
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId) ?? '';
  await umbracoApi.document.publish(documentId);
  await expect
    .poll(async () => (await umbracoApi.searchManagement.getIndex(indexAlias)).documentCount, {timeout: ConstantHelper.timeout.veryLong})
    .toBeGreaterThan(0);

  await umbracoUi.goToBackOffice();
  await umbracoUi.searchManagement.goToSearchTreeItem();
  await umbracoUi.searchManagement.goToIndexWithAlias(indexAlias);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can see the index statistics', async ({umbracoApi, umbracoUi}) => {
  // Arrange - assert the reported values, not just that the labels rendered. The box renders in beforeEach
  // and reloads only on a rebuild, so only compare values that cannot drift in between - document count
  // does, whenever another spec's teardown de-indexes, and the box would never catch up.
  const providerName = (await umbracoApi.searchManagement.getIndex(indexAlias)).providerName;

  // Assert
  await umbracoUi.searchManagement.isStatsBoxVisible();
  await umbracoUi.searchManagement.doesStatsBoxContainText(indexAlias);
  await umbracoUi.searchManagement.doesStatsBoxContainText(providerName);
  expect(await umbracoUi.searchManagement.getStatsBoxHealthStatusText()).toContain('Healthy');
});

test('can rebuild the index', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.searchManagement.clickRebuildIndexEntityAction();

  // Assert
  await umbracoUi.searchManagement.doesRebuildConfirmModalHaveText('Rebuild Search Index');
  await umbracoUi.searchManagement.doesRebuildConfirmModalHaveText('Are you sure you want to rebuild the search index');

  // Act - confirm the rebuild
  await umbracoUi.searchManagement.clickConfirmRebuildButtonAndWaitForResponse();

  await umbracoUi.searchManagement.doesRebuildStartedNotificationHaveText(`"${indexAlias}" has started`);

  // The index is still usable. Note this holds before the rebuild starts too, so it is a sanity check,
  // not evidence the rebuild ran.
  const index = await umbracoApi.searchManagement.getIndex(indexAlias);
  expect(index.healthStatus).toBe('Healthy');

  // The PUT only confirms the rebuild was queued, not that repopulation reached this document yet.
  await expect
    .poll(
      async () => (await umbracoApi.searchManagement.search(indexAlias, documentName)).documents.some((document: {id: string}) => document.id === documentId),
      {timeout: ConstantHelper.timeout.veryLong},
    )
    .toBeTruthy();
});
