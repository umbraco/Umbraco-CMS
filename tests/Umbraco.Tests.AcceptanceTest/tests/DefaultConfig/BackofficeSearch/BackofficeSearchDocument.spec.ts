import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

const documentTypeName = 'BackofficeSearchDocType';
const childDocumentTypeName = 'BackofficeSearchDocTypeChild';
const documentNamePrefix = 'BackofficeSearchDoc';
const documentName = documentNamePrefix + 'Item';
const secondDocumentName = documentNamePrefix + 'Second';
const childDocumentName = documentNamePrefix + 'Child';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.document.ensureNameNotExists(secondDocumentName);
  await umbracoApi.document.ensureNameNotExists(childDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test('can find a document by name', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  await umbracoApi.document.waitUntilIndexed(documentName, documentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();
  await umbracoUi.backofficeSearch.searchForDocument(documentName);

  // Assert
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(documentName);
});

test('can navigate to the document workspace from a search result', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  await umbracoApi.document.waitUntilIndexed(documentName, documentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();
  await umbracoUi.backofficeSearch.searchForDocument(documentName);
  await umbracoUi.backofficeSearch.clickSearchResult(documentName);

  // Assert
  await expect(umbracoUi.page).toHaveURL(new RegExp(`section/content/workspace/document/edit/${documentId}`));
});

test('can navigate between search results with arrow keys', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  const secondDocumentId = await umbracoApi.document.createDefaultDocument(secondDocumentName, documentTypeId);
  await umbracoApi.document.waitUntilIndexed(documentNamePrefix, documentId);
  await umbracoApi.document.waitUntilIndexed(documentNamePrefix, secondDocumentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();
  await umbracoUi.backofficeSearch.searchForDocument(documentNamePrefix);
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(documentName);
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(secondDocumentName);

  // Act
  await umbracoUi.backofficeSearch.pressArrowDown();
  const indexAfterFirstDown = await umbracoUi.backofficeSearch.getActiveSearchResultIndex();
  await umbracoUi.backofficeSearch.pressArrowDown();
  const indexAfterSecondDown = await umbracoUi.backofficeSearch.getActiveSearchResultIndex();
  await umbracoUi.backofficeSearch.pressArrowUp();
  const indexAfterUp = await umbracoUi.backofficeSearch.getActiveSearchResultIndex();

  // Assert
  expect(indexAfterFirstDown).toBe(0);
  expect(indexAfterSecondDown).toBe(1);
  expect(indexAfterUp).toBe(0);
});

test('clears search results when the input is emptied', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  await umbracoApi.document.waitUntilIndexed(documentName, documentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();
  await umbracoUi.backofficeSearch.searchForDocument(documentName);
  await umbracoUi.backofficeSearch.clearSearchQuery();

  // Assert
  await umbracoUi.backofficeSearch.isNavigationTipsVisible();
  await umbracoUi.backofficeSearch.doesSearchResultHaveCount(0);
});

test('can find a child document by name', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  const parentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  const childDocumentId = await umbracoApi.document.createDefaultDocumentWithParent(childDocumentName, childDocumentTypeId, parentId);
  await umbracoApi.document.waitUntilIndexed(childDocumentName, childDocumentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();
  await umbracoUi.backofficeSearch.searchForDocument(childDocumentName);

  // Assert
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(childDocumentName);
});

test('can find a document by partial name match', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  await umbracoApi.document.waitUntilIndexed(documentNamePrefix, documentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();
  await umbracoUi.backofficeSearch.searchForDocument(documentNamePrefix);

  // Assert
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(documentName);
});
