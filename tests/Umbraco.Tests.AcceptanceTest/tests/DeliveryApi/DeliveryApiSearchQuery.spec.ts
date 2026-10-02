import {expect} from '@playwright/test';
import {test} from '@umbraco/acceptance-test-helpers';

// Document Type
const documentTypeName = 'DeliveryApiSearchQueryDocumentType';
// Text-matching content
const zephyrContentNameA = 'DeliveryApiSearchQuery Zephyr Chronicle';
const zephyrContentNameB = 'DeliveryApiSearchQuery Zephyr Almanac';
const mundaneContentName = 'DeliveryApiSearchQuery Mundane Record';
const zephyrToken = 'Zephyr';
const mundaneToken = 'Mundane';
// Pagination content
const pagingToken = 'DeliveryApiSearchQueryPagingItem';
const pagingContentNames = [
  pagingToken + 'A',
  pagingToken + 'B',
  pagingToken + 'C',
  pagingToken + 'D',
  pagingToken + 'E',
];
// Lifecycle content
const unpublishContentName = 'DeliveryApiSearchQuery Unpublishable Item';
const unpublishToken = 'Unpublishable';
const trashContentName = 'DeliveryApiSearchQuery Trashable Item';
const trashToken = 'Trashable';
const renameBeforeContentName = 'DeliveryApiSearchQuery Originalword Item';
const renameBeforeToken = 'Originalword';
const renameAfterContentName = 'DeliveryApiSearchQuery Updatedword Item';
const renameAfterToken = 'Updatedword';

let documentTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName) ?? '';
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can match content whose name contains the search term', async ({umbracoApi}) => {
  // Arrange
  await umbracoApi.document.createPublishedDefaultDocument(zephyrContentNameA, documentTypeId);
  await umbracoApi.document.createPublishedDefaultDocument(zephyrContentNameB, documentTypeId);
  await umbracoApi.document.createPublishedDefaultDocument(mundaneContentName, documentTypeId);
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames('name:' + mundaneToken, undefined, [mundaneContentName]);

  // Act
  const contentItemsJson = await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames('name:' + zephyrToken, undefined, [zephyrContentNameA, zephyrContentNameB], 2);

  // Assert
  const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
  expect(returnedNames).not.toContain(mundaneContentName);
});

test('can match a name case-insensitively', async ({umbracoApi}) => {
  // Arrange
  await umbracoApi.document.createPublishedDefaultDocument(zephyrContentNameA, documentTypeId);
  await umbracoApi.document.createPublishedDefaultDocument(zephyrContentNameB, documentTypeId);
  await umbracoApi.document.createPublishedDefaultDocument(mundaneContentName, documentTypeId);
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames('name:' + mundaneToken, undefined, [mundaneContentName]);

  // Act
  const contentItemsJson = await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames('name:' + zephyrToken.toLowerCase(), undefined, [zephyrContentNameA, zephyrContentNameB], 2);

  // Assert
  const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
  expect(returnedNames).not.toContain(mundaneContentName);
});

test('returns no items for a non-matching term', async ({umbracoApi}) => {
  // Arrange
  await umbracoApi.document.createPublishedDefaultDocument(zephyrContentNameA, documentTypeId);
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames('name:' + zephyrToken, undefined, [zephyrContentNameA]);

  // Act
  const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, undefined, 'name:DeliveryApiSearchQueryNoSuchTermXyz');

  // Assert
  expect(contentItems.status()).toBe(200);
  const contentItemsJson = await contentItems.json();
  expect(contentItemsJson.total).toBe(0);
  expect(contentItemsJson.items).toEqual([]);
});

test('can paginate a filtered result set with skip and take', async ({umbracoApi}) => {
  test.slow();

  // Arrange
  for (const index of [2, 4, 0, 3, 1]) {
    await umbracoApi.document.createPublishedDefaultDocument(pagingContentNames[index], documentTypeId);
  }
  const filter = 'name:' + pagingToken;
  const sort = 'name:asc';
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames(filter, sort, pagingContentNames, pagingContentNames.length);

  // Act
  const firstPageResponse = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, undefined, filter, sort, 0, 2);
  const secondPageResponse = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, undefined, filter, sort, 2, 2);
  const thirdPageResponse = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, undefined, filter, sort, 4, 2);

  // Assert
  const firstPage = await firstPageResponse.json();
  const secondPage = await secondPageResponse.json();
  const thirdPage = await thirdPageResponse.json();
  expect(firstPage.total).toBe(pagingContentNames.length);
  expect(secondPage.total).toBe(pagingContentNames.length);
  expect(thirdPage.total).toBe(pagingContentNames.length);

  expect(firstPage.items.length).toBe(2);
  expect(secondPage.items.length).toBe(2);
  expect(thirdPage.items.length).toBe(1);

  const pagedNames = [...firstPage.items, ...secondPage.items, ...thirdPage.items].map((item: {name: string}) => item.name);
  expect(pagedNames).toEqual([...pagingContentNames]);
});

test('cannot find an unpublished document in the query results', async ({umbracoApi}) => {
  test.slow();

  // Arrange
  const contentId = await umbracoApi.document.createPublishedDefaultDocument(unpublishContentName, documentTypeId);
  const filter = 'name:' + unpublishToken;
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames(filter, undefined, [unpublishContentName]);

  // Act
  await umbracoApi.document.unpublish(contentId);

  // Assert
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryTotalIs(filter, 0);
});

test('cannot find a document moved to the recycle bin in the query results', async ({umbracoApi}) => {
  test.slow();

  // Arrange
  const contentId = await umbracoApi.document.createPublishedDefaultDocument(trashContentName, documentTypeId);
  const filter = 'name:' + trashToken;
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames(filter, undefined, [trashContentName]);

  // Act
  await umbracoApi.document.moveToRecycleBin(contentId);

  // Assert
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryTotalIs(filter, 0);
});

test('can find a renamed document by its new name', async ({umbracoApi}) => {
  test.slow();

  // Arrange
  const contentId = await umbracoApi.document.createPublishedDefaultDocument(renameBeforeContentName, documentTypeId);
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames('name:' + renameBeforeToken, undefined, [renameBeforeContentName]);

  // Act
  const document = await umbracoApi.document.getByName(renameBeforeContentName);
  document.variants[0].name = renameAfterContentName;
  await umbracoApi.document.update(contentId, document);
  await umbracoApi.document.publish(contentId);

  // Assert
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryTotalIs('name:' + renameBeforeToken, 0);
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames('name:' + renameAfterToken, undefined, [renameAfterContentName]);
});
