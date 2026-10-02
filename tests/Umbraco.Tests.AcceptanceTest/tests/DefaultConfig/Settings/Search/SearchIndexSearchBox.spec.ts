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
const searchResultsPageSize = 10;
const pagingToken = 'SearchIndexSearchBoxPaging';
const pagingDocumentNames = Array.from({length: searchResultsPageSize + 1}, (_, i) => `${pagingToken} Item${i + 1}`);

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(cultureDocumentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createPublishedDefaultDocument(documentName, documentTypeId);

  await expect
    .poll(async () => (await umbracoApi.searchManagement.getIndex(indexAlias)).healthStatus, {timeout: ConstantHelper.timeout.pageLoad})
    .toBe('Healthy');
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(indexAlias, documentName, documentId);

  await umbracoUi.goToBackOffice();
  await umbracoUi.searchManagement.goToSearchTreeItem();
  await umbracoUi.searchManagement.goToIndexWithAlias(indexAlias);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(cultureDocumentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
});

test('can see a no results message for a query with no matches', {tag: '@smoke'}, async ({umbracoUi}) => {
  // Act
  await umbracoUi.searchManagement.searchForQueryAndWaitForResponse('ThisQueryShouldNotMatchAnyIndexedDocument1234567890');

  // Assert
  await umbracoUi.searchManagement.isSearchNoResultsMessageVisible();
  await umbracoUi.searchManagement.isSearchResultsTableVisible(false);
});

test('can see only the documents that match the query', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = (await umbracoApi.documentType.getByName(documentTypeName)).id;
  const decoyDocumentId = await umbracoApi.document.createPublishedDefaultDocument(decoyDocumentName, documentTypeId);
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(indexAlias, decoyDocumentName, decoyDocumentId);
  await umbracoUi.searchManagement.searchForQueryAndWaitForResponse(decoyDocumentName);
  await umbracoUi.searchManagement.doesSearchResultsTableContainText(decoyDocumentName);

  // Act
  await umbracoUi.searchManagement.searchForQueryAndWaitForResponse(documentName);

  // Assert
  await umbracoUi.searchManagement.isSearchResultsTableVisible();
  await umbracoUi.searchManagement.isSearchNoResultsMessageVisible(false);
  await umbracoUi.searchManagement.doesSearchResultsTableContainText(documentName);
  await umbracoUi.searchManagement.doesSearchResultsTableNotContainText(decoyDocumentName);
  await umbracoUi.searchManagement.isSearchPaginationVisible(false);
});

test('can open a document from its search result', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentId = (await umbracoApi.document.getByName(documentName)).id;
  await umbracoUi.searchManagement.searchForQueryAndWaitForResponse(documentName);

  // Act
  await umbracoUi.searchManagement.clickSearchResultForDocument(documentId);

  // Assert
  await expect(umbracoUi.page).toHaveURL(new RegExp(`/document/edit/${documentId}`));
});

test('can only find a culture\'s value when that culture is selected', async ({umbracoApi, umbracoUi}) => {
  test.slow();

  // Arrange
  await umbracoApi.language.createDanishLanguage();
  const textstringDataType = await umbracoApi.dataType.getByName('Textstring');
  const cultureDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(cultureDocumentTypeName, 'Textstring', textstringDataType.id, 'CultureGroup', true, true) ?? '';
  const cultureDocumentId = await umbracoApi.document.createDocumentWithTwoCultureSpecificValues(cultureDocumentName, cultureDocumentTypeId, 'Textstring', englishIsoCode, englishSearchableValue, danishIsoCode, danishSearchableValue) ?? '';
  await umbracoApi.document.publishDocumentWithCultures(cultureDocumentId, [englishIsoCode, danishIsoCode]);
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(indexAlias, danishSearchableValue, cultureDocumentId, danishIsoCode);
  await umbracoUi.goToBackOffice();
  await umbracoUi.searchManagement.goToSearchTreeItem();
  await umbracoUi.searchManagement.goToIndexWithAlias(indexAlias);

  // Act
  await umbracoUi.searchManagement.searchForQueryAndWaitForResponse(danishSearchableValue);

  // Assert
  await umbracoUi.searchManagement.isSearchNoResultsMessageVisible();

  // Act
  await umbracoUi.searchManagement.selectSearchCulture('Danish');
  await umbracoUi.searchManagement.searchForQueryAndWaitForResponse(danishSearchableValue);

  // Assert
  await umbracoUi.searchManagement.isSearchResultsTableVisible();
  await umbracoUi.searchManagement.doesSearchResultsTableContainText(cultureDocumentId);
});

test('can see pagination when the results span more than one page', async ({umbracoApi, umbracoUi}) => {
  test.slow();

  // Arrange
  const documentTypeId = (await umbracoApi.documentType.getByName(documentTypeName)).id;
  for (const name of pagingDocumentNames) {
    await umbracoApi.document.createPublishedDefaultDocument(name, documentTypeId);
  }
  await expect
    .poll(async () => (await umbracoApi.searchManagement.search(indexAlias, pagingToken)).total, {timeout: ConstantHelper.timeout.pageLoad})
    .toBe(pagingDocumentNames.length);

  // Act
  await umbracoUi.searchManagement.searchForQueryAndWaitForResponse(pagingToken);

  // Assert
  await umbracoUi.searchManagement.isSearchResultsTableVisible();
  await umbracoUi.searchManagement.doesSearchResultsCountHaveText(`Found ${pagingDocumentNames.length} results`);
  await umbracoUi.searchManagement.isSearchPaginationVisible();
});
