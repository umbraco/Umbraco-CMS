import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// The rebuild below is a global side effect, so which index it hits must not depend on the order the API
// returns them in.
const indexAlias = 'Umb_Content';
const documentTypeName = 'SearchIndexDetailDocumentType';
const documentName = 'SearchIndexDetailDocument';

let documentId = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  test.slow();
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);

  const indexes = await umbracoApi.searchManagement.getAllIndexes();
  expect(indexes.items.some((index) => index.indexAlias === indexAlias), `the ${indexAlias} index must exist`).toBeTruthy();

  // An empty index reports "Empty", a sibling of "Healthy" rather than a subset, and a rebuild of it stays
  // "Empty" forever - so publish a document for the index to be healthy about and to repopulate with.
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId) ?? '';
  await umbracoApi.document.publish(documentId);
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(indexAlias, documentName, documentId);

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

  // The PUT only confirms the rebuild was queued, not that repopulation reached this document yet.
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(indexAlias, documentName, documentId);

  // A rebuild resets the index before repopulating it, so it reads "Empty" for a while - poll rather than sample.
  await expect
    .poll(async () => (await umbracoApi.searchManagement.getIndex(indexAlias)).healthStatus, {timeout: ConstantHelper.timeout.pageLoad})
    .toBe('Healthy');
});

// TODO: link the issue and unskip once the back office receives the IndexRebuildCompleted server event - today
// the workspace stays on its "Rebuilding index..." state after the rebuild finishes. [AZ]
test.fixme('shows the rebuild as completed when it finishes', async ({umbracoUi}) => {
  // Act
  await umbracoUi.searchManagement.clickRebuildIndexEntityAction();
  await umbracoUi.searchManagement.clickConfirmRebuildButtonAndWaitForResponse();

  // Assert
  await umbracoUi.searchManagement.doesSuccessNotificationHaveText(`"${indexAlias}" has completed`, true, false, ConstantHelper.timeout.pageLoad);
  await umbracoUi.searchManagement.isStatsBoxVisible();
});
