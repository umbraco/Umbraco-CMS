import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const documentTypeName = 'SearchIndexSearchBoxDocumentType';
const documentName = 'SearchIndexSearchBoxDocument';
let indexAlias = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);

  // The search box is disabled unless its index is Healthy, and a freshly installed instance indexes as
  // Empty - publish a document so Umb_Content has at least one document to search for.
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  await umbracoApi.document.publish(documentId);

  const indexes = await umbracoApi.searchManagement.getAllIndexes();
  indexAlias = indexes.items.find((index) => index.indexAlias === 'Umb_Content').indexAlias;
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

test('shows no results message for a query with no matches', {tag: '@smoke'}, async ({umbracoUi}) => {
  // Act
  await umbracoUi.searchManagement.searchForQueryAndWaitForResponse('ThisQueryShouldNotMatchAnyIndexedDocument1234567890');

  // Assert
  await umbracoUi.searchManagement.isSearchNoResultsMessageVisible();
  await umbracoUi.searchManagement.isSearchResultsTableVisible(false);
});

test('shows search results for the index content', async ({umbracoApi, umbracoUi}) => {
  // Arrange - determine the expected outcome via the API rather than assuming test data exists
  const apiResults = await umbracoApi.searchManagement.search(indexAlias, documentName);

  // Act
  await umbracoUi.searchManagement.searchForQueryAndWaitForResponse(documentName);

  // Assert
  expect(apiResults.total).toBeGreaterThan(0);
  await umbracoUi.searchManagement.isSearchResultsTableVisible();
  await umbracoUi.searchManagement.isSearchNoResultsMessageVisible(false);
  await umbracoUi.searchManagement.doesSearchResultsTableContainText(documentName);

  // Pagination only renders once results span more than one page
  await umbracoUi.searchManagement.isSearchPaginationVisible(apiResults.total > 10);
});
