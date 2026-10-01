import {expect} from '@playwright/test';
import {test} from '@umbraco/acceptance-test-helpers';

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
  test.slow();
  documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName) ?? '';
  secondDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(secondDocumentTypeName) ?? '';
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
    const contentItemsJson = await umbracoApi.contentDeliveryApi.queryUntilNamesPresent(filter, sort, [firstTypeContentNameA, firstTypeContentNameB], 2);

    // Assert
    expect(contentItemsJson.items[0].name).toBe(firstTypeContentNameB);
    expect(contentItemsJson.items[1].name).toBe(firstTypeContentNameA);
  });

  test('can exclude content items using the contentType IsNot filter operator', async ({umbracoApi}) => {
    // Arrange
    const secondTypeContentName = includedContentNamePrefix + 'ForIsNot';
    const secondTypeContentId = await umbracoApi.document.createDefaultDocument(secondTypeContentName, secondDocumentTypeId);
    await umbracoApi.document.publish(secondTypeContentId);
    const firstTypeContentName = excludedContentNamePrefix + 'ForIsNot';
    const firstTypeContentId = await umbracoApi.document.createDefaultDocument(firstTypeContentName, documentTypeId);
    await umbracoApi.document.publish(firstTypeContentId);

    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    const filter = 'contentType:!' + documentTypeData.alias;

    // Wait for indexing, so its absence below means it was excluded rather than not indexed yet
    await umbracoApi.contentDeliveryApi.queryUntilNamesPresent('contentType:' + documentTypeData.alias, undefined, [firstTypeContentName]);

    // Act
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

    const filter = 'name:!' + excludedContentNamePrefix;

    // Wait for indexing, so the excluded document's absence below means it was excluded rather than not indexed yet
    await umbracoApi.contentDeliveryApi.queryUntilNamesPresent('contentType:' + (await umbracoApi.documentType.getByName(documentTypeName)).alias, undefined, [excludedContentName, includedContentName]);

    // Act
    const contentItemsJson = await umbracoApi.contentDeliveryApi.queryUntilNamesPresent(filter, undefined, [includedContentName]);

    // Assert
    const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
    expect(returnedNames).not.toContain(excludedContentName);
  });
});

test.describe('member-protected content is excluded from anonymous requests', () => {
  test('excludes a member-protected content item from filter query results and direct-by-id fetch for anonymous requests', async ({umbracoApi}) => {
    // Arrange
    await umbracoApi.memberGroup.createDefaultMemberGroup(memberGroupName);
    const protectedContentId = await umbracoApi.document.createDefaultDocument(protectedContentName, documentTypeId) ?? '';
    await umbracoApi.document.publish(protectedContentId);
    const unprotectedContentId = await umbracoApi.document.createDefaultDocument(unprotectedContentName, documentTypeId) ?? '';
    await umbracoApi.document.publish(unprotectedContentId);
    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    const filter = 'contentType:' + documentTypeData.alias;

    // Wait for indexing before protecting, so the absence below means it was excluded rather than not indexed yet
    await umbracoApi.contentDeliveryApi.queryUntilNamesPresent(filter, undefined, [protectedContentName, unprotectedContentName]);
    await umbracoApi.document.setPublicAccessForDocument(protectedContentId, [memberGroupName], loginPageContentId, loginPageContentId);

    // Act
    const contentItemsJson = await umbracoApi.contentDeliveryApi.queryUntilNamesAbsent(filter, undefined, [protectedContentName]);
    const directItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(protectedContentId);

    // Assert
    const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
    expect(returnedNames).toContain(unprotectedContentName);

    expect(directItem.status()).toBe(401);
  });
});
