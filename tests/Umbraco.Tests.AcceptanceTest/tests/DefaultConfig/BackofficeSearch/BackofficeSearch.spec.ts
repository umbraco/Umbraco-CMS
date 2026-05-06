import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

const documentTypeName = 'BackofficeSearchDocType';

test.describe('Modal behavior', () => {
  test.beforeEach(async ({umbracoUi}) => {
    await umbracoUi.goToBackOffice();
  });

  test('can open and close the backoffice search modal', {tag: '@smoke'}, async ({umbracoUi}) => {
    // Act
    await umbracoUi.backofficeSearch.clickSearchHeaderButton();

    // Assert
    await umbracoUi.backofficeSearch.isSearchModalVisible();
    await umbracoUi.backofficeSearch.isNavigationTipsVisible();

    // Act
    await umbracoUi.backofficeSearch.clickOutsideToCloseModal();

    // Assert
    await umbracoUi.backofficeSearch.isSearchModalVisible(false);
  });

  test('can default to the Documents provider when in the Content section', async ({umbracoUi}) => {
    // Arrange
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.backofficeSearch.clickSearchHeaderButton();

    // Assert
    await umbracoUi.backofficeSearch.isSearchProviderActive('Documents');
  });

  test('can show the no results message when nothing matches', async ({umbracoUi}) => {
    // Act
    await umbracoUi.backofficeSearch.clickSearchHeaderButton();
    await umbracoUi.backofficeSearch.searchForQuery('zzz-no-such-content-can-exist-zzz');

    // Assert
    await umbracoUi.backofficeSearch.isNoResultsMessageVisible();
    expect(await umbracoUi.backofficeSearch.getSearchResultsCount()).toBe(0);
  });

  test('can re-open the modal in a fresh state', async ({umbracoUi}) => {
    // Act
    await umbracoUi.backofficeSearch.clickSearchHeaderButton();
    await umbracoUi.backofficeSearch.enterSearchQuery('anything');
    await umbracoUi.backofficeSearch.clickOutsideToCloseModal();
    await umbracoUi.backofficeSearch.isSearchModalVisible(false);
    await umbracoUi.backofficeSearch.clickSearchHeaderButton();

    // Assert
    await umbracoUi.backofficeSearch.isNavigationTipsVisible();
    expect(await umbracoUi.backofficeSearch.getSearchResultsCount()).toBe(0);
  });
});

test.describe('Document search', () => {
  const childDocumentTypeName = 'BackofficeSearchDocTypeChild';
  const documentNamePrefix = 'BackofficeSearchDoc';
  const documentName = documentNamePrefix + 'Item';
  const secondDocumentName = documentNamePrefix + 'Second';
  const childDocumentName = documentNamePrefix + 'Child';

  test.beforeEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.document.ensureNameNotExists(documentName);
    await umbracoApi.document.ensureNameNotExists(secondDocumentName);
    await umbracoApi.document.ensureNameNotExists(childDocumentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
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
    await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.backofficeSearch.clickSearchHeaderButton();
    await umbracoUi.backofficeSearch.searchForQuery(documentName);

    // Assert
    await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(documentName);
  });

  test('can navigate to the document workspace from a search result', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
    const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.backofficeSearch.clickSearchHeaderButton();
    await umbracoUi.backofficeSearch.searchForQuery(documentName);

    // Assert
    const href = await umbracoUi.backofficeSearch.getSearchResultHref(documentName);
    expect(href).toContain('section/content/workspace/document/edit/' + documentId);
  });

  test('can navigate between search results with arrow keys', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
    await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
    await umbracoApi.document.createDefaultDocument(secondDocumentName, documentTypeId);

    // Act
    await umbracoUi.backofficeSearch.clickSearchHeaderButton();
    await umbracoUi.backofficeSearch.searchForQuery(documentNamePrefix);
    await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(documentName);
    await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(secondDocumentName);

    // Assert
    await umbracoUi.backofficeSearch.pressArrowDown();
    expect(await umbracoUi.backofficeSearch.getActiveSearchResultIndex()).toBe(0);

    await umbracoUi.backofficeSearch.pressArrowDown();
    expect(await umbracoUi.backofficeSearch.getActiveSearchResultIndex()).toBe(1);

    await umbracoUi.backofficeSearch.pressArrowUp();
    expect(await umbracoUi.backofficeSearch.getActiveSearchResultIndex()).toBe(0);
  });

  test('can restore the navigation tips by clearing the input', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
    await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.backofficeSearch.clickSearchHeaderButton();
    await umbracoUi.backofficeSearch.searchForQuery(documentName);
    await umbracoUi.backofficeSearch.clearSearchQuery();

    // Assert
    await umbracoUi.backofficeSearch.isNavigationTipsVisible();
    expect(await umbracoUi.backofficeSearch.getSearchResultsCount()).toBe(0);
  });

  test('can find a child document by name', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
    const parentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
    await umbracoApi.document.createDefaultDocumentWithParent(childDocumentName, childDocumentTypeId, parentId);

    // Act
    await umbracoUi.backofficeSearch.clickSearchHeaderButton();
    await umbracoUi.backofficeSearch.searchForQuery(childDocumentName);

    // Assert
    await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(childDocumentName);
  });

  test('can find a document by partial name match', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
    await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);

    // Act
    await umbracoUi.backofficeSearch.clickSearchHeaderButton();
    await umbracoUi.backofficeSearch.searchForQuery(documentNamePrefix);

    // Assert
    await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(documentName);
  });
});

test.describe('Media search', () => {
  const mediaName = 'BackofficeSearchMedia';

  test.beforeEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.media.ensureNameNotExists(mediaName);
    await umbracoUi.goToBackOffice();
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.media.ensureNameNotExists(mediaName);
  });

  test('can find a media item by name', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.media.createDefaultMediaFile(mediaName);

    // Act
    await umbracoUi.backofficeSearch.clickSearchHeaderButton();
    await umbracoUi.backofficeSearch.clickSearchProvider('Media');
    await umbracoUi.backofficeSearch.searchForQuery(mediaName, undefined, ConstantHelper.apiEndpoints.mediaSearch);

    // Assert
    await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(mediaName);
  });
});

test.describe('Member search', () => {
  const memberName = 'BackofficeSearchMember';
  const memberUsername = 'backofficeSearchMember';
  const memberEmail = 'backoffice-search-member@acceptance.test';
  const memberPassword = '0123456789';

  test.beforeEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.member.ensureNameNotExists(memberName);
    await umbracoUi.goToBackOffice();
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.member.ensureNameNotExists(memberName);
  });

  test('can find a member by name', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const defaultMemberType = await umbracoApi.memberType.getByName('Member');
    await umbracoApi.member.createDefaultMember(
      memberName,
      defaultMemberType.id,
      memberEmail,
      memberUsername,
      memberPassword,
    );

    // Act
    await umbracoUi.backofficeSearch.clickSearchHeaderButton();
    await umbracoUi.backofficeSearch.clickSearchProvider('Members');
    await umbracoUi.backofficeSearch.searchForQuery(memberName, undefined, ConstantHelper.apiEndpoints.memberSearch);

    // Assert
    await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(memberName);
  });
});

test.describe('Cross-provider', () => {
  const sharedSearchToken = 'BackofficeSearchShared';
  const sharedDocumentName = sharedSearchToken + 'Doc';
  const sharedMediaName = sharedSearchToken + 'Media';

  test.beforeEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.document.ensureNameNotExists(sharedDocumentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.media.ensureNameNotExists(sharedMediaName);
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

    // Act
    await umbracoUi.backofficeSearch.clickSearchHeaderButton();
    await umbracoUi.backofficeSearch.isSearchProviderActive('Documents');
    await umbracoUi.backofficeSearch.searchForQuery(sharedSearchToken);
    await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(sharedDocumentName);
    await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(sharedMediaName, false);
    await umbracoUi.backofficeSearch.clickSearchProvider('Media');
    await umbracoUi.backofficeSearch.searchForQuery(sharedSearchToken, undefined, ConstantHelper.apiEndpoints.mediaSearch);

    // Assert
    await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(sharedMediaName);
    await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(sharedDocumentName, false);
  });
});
