import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const documentTypeName = 'SearchIndexSearchBoxDocumentType';
const documentName = 'SearchIndexSearchBoxDocument';
const decoyDocumentName = 'SearchIndexSearchBoxDecoy';
const cultureDocumentTypeName = 'SearchIndexSearchBoxCultureDocumentType';
const cultureDocumentName = 'SearchIndexSearchBoxCultureDocument';
const englishIsoCode = 'en-US';
const danishIsoCode = 'da';
const englishSearchableValue = 'SearchBoxCultureEnglishValue1234567890';
const danishSearchableValue = 'SearchBoxCultureDanishValue1234567890';
const indexAlias = 'Umb_Content';
// Mirrors PAGE_SIZE in search-index-search-box.element.ts - the search box only renders its pagination once
// the result set spans more than one page.
const searchResultsPageSize = 10;
// A shared word token so one query matches every paging document, enough to need a second page.
const pagingToken = 'SearchIndexSearchBoxPaging';
const pagingDocumentNames = Array.from({length: searchResultsPageSize + 1}, (_, i) => `${pagingToken} Item${i + 1}`);

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  test.slow();
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
    .poll(async () => (await umbracoApi.searchManagement.getIndex(indexAlias)).healthStatus, {timeout: ConstantHelper.timeout.pageLoad})
    .toBe('Healthy');
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(indexAlias, documentName, documentId);

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

test.describe('results', () => {
  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(decoyDocumentName);
  });

  test('shows only the documents that match the query', async ({umbracoApi, umbracoUi}) => {
    // Arrange - an indexed document the query must not match, so a search that ignores the query cannot pass
    const documentTypeId = (await umbracoApi.documentType.getByName(documentTypeName)).id;
    const decoyDocumentId = await umbracoApi.document.createDefaultDocument(decoyDocumentName, documentTypeId);
    await umbracoApi.document.publish(decoyDocumentId);
    await umbracoApi.searchManagement.waitUntilDocumentIsFound(indexAlias, decoyDocumentName, decoyDocumentId);

    // Act
    await umbracoUi.searchManagement.searchForQueryAndWaitForResponse(documentName);

    // Assert - a single match fits on one page
    await umbracoUi.searchManagement.isSearchResultsTableVisible();
    await umbracoUi.searchManagement.isSearchNoResultsMessageVisible(false);
    await umbracoUi.searchManagement.doesSearchResultsTableContainText(documentName);
    await umbracoUi.searchManagement.doesSearchResultsTableNotContainText(decoyDocumentName);
    await umbracoUi.searchManagement.isSearchPaginationVisible(false);
  });
});

test('opens the document from its search result', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentId = (await umbracoApi.document.getByName(documentName)).id;
  await umbracoUi.searchManagement.searchForQueryAndWaitForResponse(documentName);

  // Act
  await umbracoUi.searchManagement.clickSearchResultForDocument(documentId);

  // Assert
  await expect(umbracoUi.page).toHaveURL(new RegExp(`/document/edit/${documentId}`));
});

test.describe('culture', () => {
  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(cultureDocumentName);
    await umbracoApi.documentType.ensureNameNotExists(cultureDocumentTypeName);
    await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
  });

  test('only finds a culture\'s value when that culture is selected', async ({umbracoApi, umbracoUi}) => {
    // Arrange - a second language makes the culture selector render, so the page is opened again afterwards
    await umbracoApi.language.createDanishLanguage();
    const textstringDataType = await umbracoApi.dataType.getByName('Textstring');
    const cultureDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(cultureDocumentTypeName, 'Textstring', textstringDataType.id, 'CultureGroup', true, true) ?? '';
    const cultureDocumentId = await umbracoApi.document.createDocumentWithTwoCultureSpecificValues(cultureDocumentName, cultureDocumentTypeId, 'Textstring', englishIsoCode, englishSearchableValue, danishIsoCode, danishSearchableValue) ?? '';
    await umbracoApi.document.publishDocumentWithCultures(cultureDocumentId, [englishIsoCode, danishIsoCode]);
    await umbracoApi.searchManagement.waitUntilDocumentIsFound(indexAlias, danishSearchableValue, cultureDocumentId, danishIsoCode);
    await umbracoUi.goToBackOffice();
    await umbracoUi.searchManagement.goToSearchTreeItem();
    await umbracoUi.searchManagement.goToIndexWithAlias(indexAlias);

    // Act & Assert - the default (English) culture must not see the Danish value
    await umbracoUi.searchManagement.searchForQueryAndWaitForResponse(danishSearchableValue);
    await umbracoUi.searchManagement.isSearchNoResultsMessageVisible();

    // Act & Assert - with Danish selected it is found
    await umbracoUi.searchManagement.selectSearchCulture('Danish');
    await umbracoUi.searchManagement.searchForQueryAndWaitForResponse(danishSearchableValue);
    await umbracoUi.searchManagement.isSearchResultsTableVisible();
    await umbracoUi.searchManagement.doesSearchResultsTableContainText(cultureDocumentId);
  });
});

test.describe('pagination', () => {
  test.afterEach(async ({umbracoApi}) => {
    for (const name of pagingDocumentNames) {
      await umbracoApi.document.ensureNameNotExists(name);
    }
  });

  test('shows pagination when the results span more than one page', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const documentTypeId = (await umbracoApi.documentType.getByName(documentTypeName)).id;
    for (const name of pagingDocumentNames) {
      const pagingDocumentId = await umbracoApi.document.createDefaultDocument(name, documentTypeId);
      await umbracoApi.document.publish(pagingDocumentId);
    }
    await expect
      .poll(async () => (await umbracoApi.searchManagement.search(indexAlias, pagingToken)).total, {timeout: ConstantHelper.timeout.pageLoad})
      .toBe(pagingDocumentNames.length);

    // Act
    await umbracoUi.searchManagement.searchForQueryAndWaitForResponse(pagingToken);

    // Assert - the exact count proves the query was applied; the document from beforeEach also exists and
    // would make an unfiltered search report more
    await umbracoUi.searchManagement.isSearchResultsTableVisible();
    await umbracoUi.searchManagement.doesSearchResultsCountHaveText(`Found ${pagingDocumentNames.length} results`);
    await umbracoUi.searchManagement.isSearchPaginationVisible();
  });
});
