import {expect} from '@playwright/test';
import {ApiHelpers, test} from '@umbraco/acceptance-test-helpers';

// Document Type
const documentTypeName = 'DeliveryApiSearchQueryDocumentType';
// Text-matching content
const zephyrContentNameA = 'DeliveryApiSearchQuery Zephyr Chronicle';
const zephyrContentNameB = 'DeliveryApiSearchQuery Zephyr Almanac';
const mundaneContentName = 'DeliveryApiSearchQuery Mundane Record';
const zephyrToken = 'Zephyr';
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

async function createAndPublishDocument(umbracoApi: ApiHelpers, name: string): Promise<string> {
  const contentId = await umbracoApi.document.createDefaultDocument(name, documentTypeId) ?? '';
  await umbracoApi.document.publish(contentId);
  return contentId;
}

async function queryContent(umbracoApi: ApiHelpers, filter?: string, sort?: string, skip?: number, take?: number) {
  const response = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, undefined, filter, sort, skip, take);
  return await response.json();
}

test.beforeEach(async ({umbracoApi}) => {
  test.slow();
  documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName) ?? '';
});

test.afterEach(async ({umbracoApi}) => {
  for (const name of [zephyrContentNameA, zephyrContentNameB, mundaneContentName, ...pagingContentNames]) {
    await umbracoApi.document.ensureNameNotExists(name);
  }
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.describe('name text-matching', () => {
  test.beforeEach(async ({umbracoApi}) => {
    await createAndPublishDocument(umbracoApi, zephyrContentNameA);
    await createAndPublishDocument(umbracoApi, zephyrContentNameB);
    await createAndPublishDocument(umbracoApi, mundaneContentName);
  });

  test('can match content whose name contains the search term', async ({umbracoApi}) => {
    // Act
    const contentItemsJson = await umbracoApi.contentDeliveryApi.queryUntilNamesPresent('name:' + zephyrToken, undefined, [zephyrContentNameA, zephyrContentNameB], 2, 0, 100);

    // Assert
    const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
    expect(returnedNames).not.toContain(mundaneContentName);
  });

  test('can match a name case-insensitively', async ({umbracoApi}) => {
    // Act
    const contentItemsJson = await umbracoApi.contentDeliveryApi.queryUntilNamesPresent('name:' + zephyrToken.toLowerCase(), undefined, [zephyrContentNameA, zephyrContentNameB], 2, 0, 100);

    // Assert
    const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
    expect(returnedNames).not.toContain(mundaneContentName);
  });

  test('returns no items for a non-matching term', async ({umbracoApi}) => {
    // Arrange
    await umbracoApi.contentDeliveryApi.queryUntilNamesPresent('name:' + zephyrToken, undefined, [zephyrContentNameA]);

    // Act
    const contentItemsJson = await queryContent(umbracoApi, 'name:DeliveryApiSearchQueryNoSuchTermXyz');

    // Assert
    expect(contentItemsJson.total).toBe(0);
    expect(contentItemsJson.items).toEqual([]);
  });
});

test.describe('skip and take pagination', () => {
  test('can paginate a filtered result set with skip and take', async ({umbracoApi}) => {
    // Arrange
    for (const index of [2, 4, 0, 3, 1]) {
      await createAndPublishDocument(umbracoApi, pagingContentNames[index]);
    }
    const filter = 'name:' + pagingToken;
    const sort = 'name:asc';

    await umbracoApi.contentDeliveryApi.queryUntilNamesPresent(filter, sort, pagingContentNames, pagingContentNames.length, 0, 100);

    // Act
    const firstPage = await queryContent(umbracoApi, filter, sort, 0, 2);
    const secondPage = await queryContent(umbracoApi, filter, sort, 2, 2);
    const thirdPage = await queryContent(umbracoApi, filter, sort, 4, 2);

    // Assert
    expect(firstPage.total).toBe(pagingContentNames.length);
    expect(secondPage.total).toBe(pagingContentNames.length);
    expect(thirdPage.total).toBe(pagingContentNames.length);

    expect(firstPage.items.length).toBe(2);
    expect(secondPage.items.length).toBe(2);
    expect(thirdPage.items.length).toBe(1);

    const pagedNames = [...firstPage.items, ...secondPage.items, ...thirdPage.items].map((item: {name: string}) => item.name);
    expect(pagedNames).toEqual([...pagingContentNames]);
  });
});

test.describe('content lifecycle updates the index', () => {
  test.afterEach(async ({umbracoApi}) => {
    for (const name of [unpublishContentName, trashContentName, renameBeforeContentName, renameAfterContentName]) {
      await umbracoApi.document.ensureNameNotExists(name);
    }
    await umbracoApi.document.emptyRecycleBin();
  });

  test('excludes an unpublished document from query results', async ({umbracoApi}) => {
    // Arrange
    const contentId = await createAndPublishDocument(umbracoApi, unpublishContentName);
    const filter = 'name:' + unpublishToken;
    await umbracoApi.contentDeliveryApi.queryUntilNamesPresent(filter, undefined, [unpublishContentName]);

    // Act
    await umbracoApi.document.unpublish(contentId);

    // Assert
    await umbracoApi.contentDeliveryApi.queryUntilTotalIs(filter, 0);
  });

  test('excludes a document moved to the recycle bin from query results', async ({umbracoApi}) => {
    // Arrange
    const contentId = await createAndPublishDocument(umbracoApi, trashContentName);
    const filter = 'name:' + trashToken;
    await umbracoApi.contentDeliveryApi.queryUntilNamesPresent(filter, undefined, [trashContentName]);

    // Act
    await umbracoApi.document.moveToRecycleBin(contentId);

    // Assert
    await umbracoApi.contentDeliveryApi.queryUntilTotalIs(filter, 0);
  });

  test('reindexes a renamed document under its new name', async ({umbracoApi}) => {
    // Arrange
    const contentId = await createAndPublishDocument(umbracoApi, renameBeforeContentName);
    await umbracoApi.contentDeliveryApi.queryUntilNamesPresent('name:' + renameBeforeToken, undefined, [renameBeforeContentName]);

    // Act
    const document = await umbracoApi.document.getByName(renameBeforeContentName);
    document.variants[0].name = renameAfterContentName;
    await umbracoApi.document.update(contentId, document);
    await umbracoApi.document.publish(contentId);

    // Assert
    await umbracoApi.contentDeliveryApi.queryUntilTotalIs('name:' + renameBeforeToken, 0);
    await umbracoApi.contentDeliveryApi.queryUntilNamesPresent('name:' + renameAfterToken, undefined, [renameAfterContentName]);
  });
});
