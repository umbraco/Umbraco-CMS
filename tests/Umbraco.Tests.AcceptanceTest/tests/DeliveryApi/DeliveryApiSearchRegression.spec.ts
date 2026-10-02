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
const sortSecondTypeContentName = includedContentNamePrefix + 'SecondType';
const sortContentNameA = includedContentNamePrefix + 'B';
const sortContentNameB = includedContentNamePrefix + 'A';
const isNotIncludedContentName = includedContentNamePrefix + 'ForIsNot';
const isNotExcludedContentName = excludedContentNamePrefix + 'ForIsNot';
const doesNotContainExcludedContentName = excludedContentNamePrefix + 'DoesNotContain';
const doesNotContainIncludedContentName = includedContentNamePrefix + 'DoesNotContain';
// Member Group
const memberGroupName = 'DeliveryApiSearchRegressionMemberGroup';

let documentTypeId = '';
let secondDocumentTypeId = '';
let loginPageContentId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondDocumentTypeName);
  await umbracoApi.memberGroup.ensureNameNotExists(memberGroupName);
  documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName) ?? '';
  secondDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(secondDocumentTypeName) ?? '';
  loginPageContentId = await umbracoApi.document.createPublishedDefaultDocument(loginPageContentName, secondDocumentTypeId);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondDocumentTypeName);
  await umbracoApi.memberGroup.ensureNameNotExists(memberGroupName);
});

test('can combine a contentType filter with a sort', async ({umbracoApi}) => {
  // Arrange
  await umbracoApi.document.createPublishedDefaultDocument(sortSecondTypeContentName, secondDocumentTypeId);
  await umbracoApi.document.createPublishedDefaultDocument(sortContentNameA, documentTypeId);
  await umbracoApi.document.createPublishedDefaultDocument(sortContentNameB, documentTypeId);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  const filter = 'contentType:' + documentTypeData.alias;
  const sort = 'name:asc';

  // Act
  const contentItemsJson = await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames(filter, sort, [sortContentNameA, sortContentNameB], 2);

  // Assert
  expect(contentItemsJson.items[0].name).toBe(sortContentNameB);
  expect(contentItemsJson.items[1].name).toBe(sortContentNameA);
});

test('can exclude content items using the contentType IsNot filter operator', async ({umbracoApi}) => {
  test.slow();

  // Arrange
  await umbracoApi.document.createPublishedDefaultDocument(isNotIncludedContentName, secondDocumentTypeId);
  await umbracoApi.document.createPublishedDefaultDocument(isNotExcludedContentName, documentTypeId);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  const filter = 'contentType:!' + documentTypeData.alias;
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames('contentType:' + documentTypeData.alias, undefined, [isNotExcludedContentName]);

  // Act
  const contentItemsJson = await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames(filter, undefined, [isNotIncludedContentName, loginPageContentName]);

  // Assert
  const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
  expect(returnedNames).not.toContain(isNotExcludedContentName);
});

test('can exclude content items using the name DoesNotContain filter operator', async ({umbracoApi}) => {
  test.slow();

  // Arrange
  await umbracoApi.document.createPublishedDefaultDocument(doesNotContainExcludedContentName, documentTypeId);
  await umbracoApi.document.createPublishedDefaultDocument(doesNotContainIncludedContentName, documentTypeId);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  const filter = 'name:!' + excludedContentNamePrefix;
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames('contentType:' + documentTypeData.alias, undefined, [doesNotContainExcludedContentName, doesNotContainIncludedContentName]);

  // Act
  const contentItemsJson = await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames(filter, undefined, [doesNotContainIncludedContentName]);

  // Assert
  const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
  expect(returnedNames).not.toContain(doesNotContainExcludedContentName);
});

test('cannot get member-protected content anonymously from a filter query or by id', async ({umbracoApi}) => {
  test.slow();

  // Arrange
  await umbracoApi.memberGroup.createDefaultMemberGroup(memberGroupName);
  const protectedContentId = await umbracoApi.document.createPublishedDefaultDocument(protectedContentName, documentTypeId);
  await umbracoApi.document.createPublishedDefaultDocument(unprotectedContentName, documentTypeId);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  const filter = 'contentType:' + documentTypeData.alias;
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames(filter, undefined, [protectedContentName, unprotectedContentName]);
  await umbracoApi.document.setPublicAccessForDocument(protectedContentId, [memberGroupName], loginPageContentId, loginPageContentId);

  // Act
  const contentItemsJson = await umbracoApi.contentDeliveryApi.waitUntilContentQueryExcludesNames(filter, undefined, [protectedContentName]);
  const directItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(protectedContentId);

  // Assert
  const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
  expect(returnedNames).toContain(unprotectedContentName);
  expect(directItem.status()).toBe(401);
});
