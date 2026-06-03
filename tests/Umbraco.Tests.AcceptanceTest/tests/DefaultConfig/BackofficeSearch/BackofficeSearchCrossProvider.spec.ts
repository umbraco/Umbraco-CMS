import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const documentTypeName = 'BackofficeSearchDocType';
const sharedSearchPrefix = 'BackofficeSearchShared';
const sharedDocumentName = sharedSearchPrefix + 'Doc';
const sharedMediaName = sharedSearchPrefix + 'Media';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(sharedDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.media.ensureNameNotExists(sharedMediaName);
});

test('can rerun the active query when switching providers', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  await umbracoApi.document.createDefaultDocument(sharedDocumentName, documentTypeId);
  await umbracoApi.media.createDefaultMediaFile(sharedMediaName);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();
  await umbracoUi.backofficeSearch.searchForDocument(sharedSearchPrefix);
  await umbracoUi.backofficeSearch.isSearchProviderActive('Documents');
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(sharedDocumentName);
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(sharedMediaName, false);

  // Act
  await umbracoUi.backofficeSearch.clickSearchProviderAndWaitForRerun('Media', ConstantHelper.apiEndpoints.mediaSearch);

  // Assert
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(sharedMediaName);
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(sharedDocumentName, false);
});
