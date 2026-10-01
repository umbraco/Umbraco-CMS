import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const indexAlias = 'Umb_Content';
const documentTypeName = 'SearchIndexDetailDocumentType';
const documentName = 'SearchIndexDetailDocument';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);

  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createPublishedDefaultDocument(documentName, documentTypeId);
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(indexAlias, documentName, documentId);
  await expect
    .poll(async () => (await umbracoApi.searchManagement.getIndex(indexAlias)).healthStatus, {timeout: ConstantHelper.timeout.pageLoad})
    .toBe('Healthy');

  await umbracoUi.goToBackOffice();
  await umbracoUi.searchManagement.goToSearchTreeItem();
  await umbracoUi.searchManagement.goToIndexWithAlias(indexAlias);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can see the index statistics', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const providerName = (await umbracoApi.searchManagement.getIndex(indexAlias)).providerName;

  // Assert
  await umbracoUi.searchManagement.isStatsBoxVisible();
  await umbracoUi.searchManagement.doesStatsBoxContainText(indexAlias);
  await umbracoUi.searchManagement.doesStatsBoxContainText(providerName);
  await umbracoUi.searchManagement.doesStatsBoxHealthStatusHaveText('Healthy');
});

test('can rebuild the index', {tag: '@smoke'}, async ({umbracoUi}) => {
  // Act
  await umbracoUi.searchManagement.clickRebuildIndexWorkspaceAction();

  // Assert
  await umbracoUi.searchManagement.doesModalHaveText('Rebuild Search Index');
  await umbracoUi.searchManagement.doesModalHaveText('Are you sure you want to rebuild the search index');

  // Act
  await umbracoUi.searchManagement.clickConfirmRebuildButtonAndWaitForResponse();

  // Assert
  await umbracoUi.searchManagement.doesRebuildStartedNotificationHaveText(`"${indexAlias}" has started`);
});

// TODO: link the issue and unskip once the back office receives the IndexRebuildCompleted server event [AZ]
test.fixme('can see that the rebuild has completed', async ({umbracoUi}) => {
  test.slow();

  // Act
  await umbracoUi.searchManagement.clickRebuildIndexWorkspaceAction();
  await umbracoUi.searchManagement.clickConfirmRebuildButtonAndWaitForResponse();

  // Assert
  await umbracoUi.searchManagement.doesSuccessNotificationHaveText(`"${indexAlias}" has completed`, true, false, ConstantHelper.timeout.pageLoad);
  await umbracoUi.searchManagement.isStatsBoxVisible();
});
