import {expect} from '@playwright/test';
import {test} from '@umbraco/acceptance-test-helpers';

// Raised from the 60s default - chained index polls would otherwise hit the test timeout first.
test.describe.configure({timeout: 120000});

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

test.beforeEach(async ({umbracoApi}) => {
  documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName) ?? '';
  secondDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(secondDocumentTypeName) ?? '';
  // Uses secondDocumentTypeId so it never matches a contentType filter/sort scoped to documentTypeId in the tests below.
  loginPageContentId = await umbracoApi.document.createDefaultDocument(loginPageContentName, secondDocumentTypeId) ?? '';
  await umbracoApi.document.publish(loginPageContentId);
});

// Teardown belongs here rather than at the end of a test body: a failing test skips its own trailing
// cleanup, and with workers: 1 that residue lands on the next spec, which gates on index membership.
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

    // Act - the total is part of the wait, not a check after it (see DeliveryApiSearchQuery for why)
    const contentItemsJson = await umbracoApi.contentDeliveryApi.queryUntilNamesPresent(filter, sort, [firstTypeContentNameA, firstTypeContentNameB], 2);

    // Assert
    // Only the two documentTypeName items should be returned - the secondDocumentTypeName item and the login page must be excluded
    expect(contentItemsJson.items[0].name).toBe(firstTypeContentNameB);
    expect(contentItemsJson.items[1].name).toBe(firstTypeContentNameA);
  });

  test('can exclude content items using the contentType IsNot filter operator', async ({umbracoApi}) => {
    // Arrange - the filter needs something of the excluded type to actually exclude, otherwise it passes
    // just as well when IsNot is a no-op that returns everything
    const secondTypeContentName = includedContentNamePrefix + 'ForIsNot';
    const secondTypeContentId = await umbracoApi.document.createDefaultDocument(secondTypeContentName, secondDocumentTypeId);
    await umbracoApi.document.publish(secondTypeContentId);
    const firstTypeContentName = excludedContentNamePrefix + 'ForIsNot';
    const firstTypeContentId = await umbracoApi.document.createDefaultDocument(firstTypeContentName, documentTypeId);
    await umbracoApi.document.publish(firstTypeContentId);

    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    // "contentType:!alias" is the IsNot operator - it must exclude everything of that content type.
    const filter = 'contentType:!' + documentTypeData.alias;

    // Prove the excluded document is indexed before asserting its absence, so an unindexed document cannot
    // stand in for an excluded one.
    await umbracoApi.contentDeliveryApi.queryUntilNamesPresent('contentType:' + documentTypeData.alias, undefined, [firstTypeContentName]);

    // Act
    // The login page (also secondDocumentTypeId) must be included alongside secondTypeContentName - both are excluded by the filter's type.
    const contentItemsJson = await umbracoApi.contentDeliveryApi.queryUntilNamesPresent(filter, undefined, [secondTypeContentName, loginPageContentName]);

    // Assert
    const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
    expect(returnedNames).not.toContain(firstTypeContentName);
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

    // Prove both are indexed before asserting the negation - otherwise an excluded document that simply
    // has not been indexed yet satisfies the assertion.
    await umbracoApi.contentDeliveryApi.queryUntilNamesPresent('contentType:' + (await umbracoApi.documentType.getByName(documentTypeName)).alias, undefined, [excludedContentName, includedContentName]);

    // Act
    const contentItemsJson = await umbracoApi.contentDeliveryApi.queryUntilNamesPresent(filter, undefined, [includedContentName]);

    // Assert
    const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
    expect(returnedNames).not.toContain(excludedContentName);
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
    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    const filter = 'contentType:' + documentTypeData.alias;

    // Both documents must be indexed before protection is applied, otherwise the protected document being
    // absent below proves nothing - it would simply not have been indexed yet.
    await umbracoApi.contentDeliveryApi.queryUntilNamesPresent(filter, undefined, [protectedContentName, unprotectedContentName]);
    await umbracoApi.document.setPublicAccessForDocument(protectedContentId, [memberGroupName], loginPageContentId, loginPageContentId);

    // Act
    // Protection is a second async index write on top of the publish, so wait on the disappearance itself
    // rather than on some other write that has no ordering relationship with it.
    const contentItemsJson = await umbracoApi.contentDeliveryApi.queryUntilNamesAbsent(filter, undefined, [protectedContentName]);
    const directItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(protectedContentId);

    // Assert
    const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
    expect(returnedNames).toContain(unprotectedContentName);

    // Protected content that exists but requires member access returns 401, not 404 (per ByIdContentApiController).
    expect(directItem.status()).toBe(401);
  });
});
