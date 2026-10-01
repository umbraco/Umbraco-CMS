import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

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

  // An index without documents reports "Empty" rather than "Healthy", and a rebuild has nothing to repopulate.
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
  // Arrange - document count is left out: other specs' teardown can change it while the box is open
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

  // Act
  await umbracoUi.searchManagement.clickConfirmRebuildButtonAndWaitForResponse();

  // Assert
  await umbracoUi.searchManagement.doesRebuildStartedNotificationHaveText(`"${indexAlias}" has started`);
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(indexAlias, documentName, documentId);
  // The rebuild empties the index before repopulating it
  await expect
    .poll(async () => (await umbracoApi.searchManagement.getIndex(indexAlias)).healthStatus, {timeout: ConstantHelper.timeout.pageLoad})
    .toBe('Healthy');
});

// TODO: link the issue and unskip once the back office receives the IndexRebuildCompleted server event
test.fixme('shows the rebuild as completed when it finishes', async ({umbracoUi}) => {
  // Act
  await umbracoUi.searchManagement.clickRebuildIndexEntityAction();
  await umbracoUi.searchManagement.clickConfirmRebuildButtonAndWaitForResponse();

  // Assert
  await umbracoUi.searchManagement.doesSuccessNotificationHaveText(`"${indexAlias}" has completed`, true, false, ConstantHelper.timeout.pageLoad);
  await umbracoUi.searchManagement.isStatsBoxVisible();
});
