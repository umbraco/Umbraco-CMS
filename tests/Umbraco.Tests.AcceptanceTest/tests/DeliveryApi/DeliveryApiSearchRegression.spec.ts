import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// Document Type
const documentTypeName = 'DeliveryApiSearchRegressionDocumentType';
const secondDocumentTypeName = 'DeliveryApiSearchRegressionSecondDocumentType';
// Content
const loginPageContentName = 'DeliveryApiSearchRegressionLoginPage';
const protectedContentName = 'DeliveryApiSearchRegressionProtectedContent';
const unprotectedContentName = 'DeliveryApiSearchRegressionUnprotectedContent';
const excludedContentNamePrefix = 'DeliveryApiSearchRegressionExcluded';
const includedContentNamePrefix = 'DeliveryApiSearchRegressionIncluded';
// Member Group
const memberGroupName = 'DeliveryApiSearchRegressionMemberGroup';

let documentTypeId = '';
let secondDocumentTypeId = '';
let loginPageContentId = '';

// Content indexing is asynchronous and its latency varies, so poll the query itself rather than a flat wait
// followed by a single check - a flat wait can consistently undershoot under load.
async function queryUntilNamesPresent(umbracoApi, filter: string | undefined, sort: string | undefined, expectedNames: string[]) {
  let contentItemsJson;
  await expect
    .poll(
      async () => {
        const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, undefined, filter, sort);
        contentItemsJson = await contentItems.json();
        const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
        return expectedNames.every((name) => returnedNames.includes(name));
      },
      {timeout: ConstantHelper.timeout.pageLoad},
    )
    .toBe(true);
  return contentItemsJson;
}

test.beforeEach(async ({umbracoApi}) => {
  documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName) ?? '';
  secondDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(secondDocumentTypeName) ?? '';
  // Uses secondDocumentTypeId so it never matches a contentType filter/sort scoped to documentTypeId in the tests below.
  loginPageContentId = await umbracoApi.document.createDefaultDocument(loginPageContentName, secondDocumentTypeId) ?? '';
  await umbracoApi.document.publish(loginPageContentId);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(protectedContentName);
  await umbracoApi.document.ensureNameNotExists(unprotectedContentName);
  await umbracoApi.document.ensureNameNotExists(loginPageContentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondDocumentTypeName);
  await umbracoApi.memberGroup.ensureNameNotExists(memberGroupName);
});

test.describe('filter and sort content items', () => {
  test('can combine a contentType filter with a sort', async ({umbracoApi}) => {
    // Arrange
    const secondTypeContentName = includedContentNamePrefix + 'SecondType';
    const firstTypeContentNameA = includedContentNamePrefix + 'B';
    const firstTypeContentNameB = includedContentNamePrefix + 'A';

    const secondTypeContentId = await umbracoApi.document.createDefaultDocument(secondTypeContentName, secondDocumentTypeId);
    await umbracoApi.document.publish(secondTypeContentId);
    const firstTypeContentIdA = await umbracoApi.document.createDefaultDocument(firstTypeContentNameA, documentTypeId);
    await umbracoApi.document.publish(firstTypeContentIdA);
    const firstTypeContentIdB = await umbracoApi.document.createDefaultDocument(firstTypeContentNameB, documentTypeId);
    await umbracoApi.document.publish(firstTypeContentIdB);

    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    const filter = 'contentType:' + documentTypeData.alias;
    const sort = 'name:asc';

    // Act
    const contentItemsJson = await queryUntilNamesPresent(umbracoApi, filter, sort, [firstTypeContentNameA, firstTypeContentNameB]);

    // Assert
    // Only the two documentTypeName items should be returned - the secondDocumentTypeName item and the login page must be excluded
    expect(contentItemsJson.total).toBe(2);
    expect(contentItemsJson.items[0].name).toBe(firstTypeContentNameB);
    expect(contentItemsJson.items[1].name).toBe(firstTypeContentNameA);

    // Clean
    await umbracoApi.document.ensureNameNotExists(secondTypeContentName);
    await umbracoApi.document.ensureNameNotExists(firstTypeContentNameA);
    await umbracoApi.document.ensureNameNotExists(firstTypeContentNameB);
  });

  test('can exclude content items using the contentType IsNot filter operator', async ({umbracoApi}) => {
    // Arrange
    const secondTypeContentName = includedContentNamePrefix + 'ForIsNot';
    const secondTypeContentId = await umbracoApi.document.createDefaultDocument(secondTypeContentName, secondDocumentTypeId);
    await umbracoApi.document.publish(secondTypeContentId);

    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    // "contentType:!alias" is the IsNot operator - it must exclude everything of that content type.
    const filter = 'contentType:!' + documentTypeData.alias;

    // Act
    // The login page (also secondDocumentTypeId) must be included alongside secondTypeContentName - both are excluded by the filter's type.
    await queryUntilNamesPresent(umbracoApi, filter, undefined, [secondTypeContentName, loginPageContentName]);

    // Clean
    await umbracoApi.document.ensureNameNotExists(secondTypeContentName);
  });

  test('can exclude content items using the name DoesNotContain filter operator', async ({umbracoApi}) => {
    // Arrange
    const excludedContentName = excludedContentNamePrefix + 'DoesNotContain';
    const includedContentName = includedContentNamePrefix + 'DoesNotContain';
    const excludedContentId = await umbracoApi.document.createDefaultDocument(excludedContentName, documentTypeId);
    await umbracoApi.document.publish(excludedContentId);
    const includedContentId = await umbracoApi.document.createDefaultDocument(includedContentName, documentTypeId);
    await umbracoApi.document.publish(includedContentId);

    // "name:!value" is the DoesNotContain operator
    const filter = 'name:!' + excludedContentNamePrefix;

    // Act
    const contentItemsJson = await queryUntilNamesPresent(umbracoApi, filter, undefined, [includedContentName]);

    // Assert
    const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
    expect(returnedNames).not.toContain(excludedContentName);

    // Clean
    await umbracoApi.document.ensureNameNotExists(excludedContentName);
    await umbracoApi.document.ensureNameNotExists(includedContentName);
  });
});

test.describe('member-protected content is excluded from anonymous requests', () => {
  // Protected content filtering now runs through AccessContext at the searcher/index layer instead of a
  // post-query filter, so this is the main regression risk introduced by the new search stack.
  test('excludes a member-protected content item from filter query results and direct-by-id fetch for anonymous requests', async ({umbracoApi}) => {
    // Arrange
    await umbracoApi.memberGroup.createDefaultMemberGroup(memberGroupName);
    const protectedContentId = await umbracoApi.document.createDefaultDocument(protectedContentName, documentTypeId) ?? '';
    await umbracoApi.document.publish(protectedContentId);
    const unprotectedContentId = await umbracoApi.document.createDefaultDocument(unprotectedContentName, documentTypeId) ?? '';
    await umbracoApi.document.publish(unprotectedContentId);
    await umbracoApi.document.setPublicAccessForDocument(protectedContentId, [memberGroupName], loginPageContentId, loginPageContentId);

    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    const filter = 'contentType:' + documentTypeData.alias;

    // Act
    // Protection is a second async update on top of the initial index write, so poll until the unprotected
    // document (indexed after it) appears, proving both writes have landed.
    const contentItemsJson = await queryUntilNamesPresent(umbracoApi, filter, undefined, [unprotectedContentName]);
    const directItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(protectedContentId);

    // Assert
    const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
    expect(returnedNames).not.toContain(protectedContentName);

    // Protected content that exists but requires member access returns 401, not 404 (per ByIdContentApiController).
    expect(directItem.status()).toBe(401);
  });
});
