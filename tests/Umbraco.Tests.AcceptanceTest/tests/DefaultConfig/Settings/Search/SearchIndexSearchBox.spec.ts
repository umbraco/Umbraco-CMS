import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// Raised from the 60s default - chained index polls would otherwise hit the test timeout first.
test.describe.configure({timeout: 120000});

const documentTypeName = 'SearchIndexSearchBoxDocumentType';
const documentName = 'SearchIndexSearchBoxDocument';
const indexAlias = 'Umb_Content';
// Mirrors PAGE_SIZE in search-index-search-box.element.ts - the search box only renders its pagination once
// the result set spans more than one page.
const searchResultsPageSize = 10;

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);

  // The search box is disabled unless its index is Healthy, and a freshly installed instance indexes as
  // Empty - publish a document so Umb_Content has at least one document to search for.
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  await umbracoApi.document.publish(documentId);

  const indexes = await umbracoApi.searchManagement.getAllIndexes();
  const contentIndex = indexes.items.find((index) => index.indexAlias === indexAlias);
  expect(contentIndex, `the ${indexAlias} index must exist`).toBeTruthy();

  // A healthy index does not imply the document just published has been indexed - indexing is asynchronous -
  // so wait for the document itself to be findable, which is what every test below depends on.
  await expect
    .poll(async () => (await umbracoApi.searchManagement.getIndex(indexAlias)).healthStatus, {timeout: ConstantHelper.timeout.veryLong})
    .toBe('Healthy');
  await expect
    .poll(async () => (await umbracoApi.searchManagement.search(indexAlias, documentName)).total, {timeout: ConstantHelper.timeout.veryLong})
    .toBeGreaterThan(0);

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
  await umbracoUi.searchManagement.isSearchResultsTableVisible();
  await umbracoUi.searchManagement.isSearchNoResultsMessageVisible(false);
  await umbracoUi.searchManagement.doesSearchResultsTableContainText(documentName);
  await umbracoUi.searchManagement.isSearchPaginationVisible(apiResults.total > searchResultsPageSize);
});
