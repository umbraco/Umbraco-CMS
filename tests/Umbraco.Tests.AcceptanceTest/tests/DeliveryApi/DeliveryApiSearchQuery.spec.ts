import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// Exercises the new search stack through the Delivery API content query surface
// (/umbraco/delivery/api/v2/content): name text-matching, skip/take pagination, and
// content-lifecycle index updates (unpublish, recycle bin, edit + republish).
//
// The name filter ("name:value") runs as a text filter over the analyzed name field, so it
// matches whole word tokens case-insensitively (with a trailing wildcard), not arbitrary
// substrings. The test data therefore uses multi-word names so the query term is a real token.
//
// Not covered here, by design: facet aggregation and relevance ranking. The Delivery
// API content query exposes only fetch/filter/sort/skip/take - it has no facet parameter
// or response, and no relevance-scored sort - so those capabilities of the search stack
// cannot be asserted through this endpoint. They live only against a search-consuming
// front-end, which the acceptance project does not host.

// Document Type
const documentTypeName = 'DeliveryApiSearchQueryDocumentType';
// Text-matching content - "Zephyr" is a distinct word token shared by two names and absent from the third
const zephyrContentNameA = 'DeliveryApiSearchQuery Zephyr Chronicle';
const zephyrContentNameB = 'DeliveryApiSearchQuery Zephyr Almanac';
const mundaneContentName = 'DeliveryApiSearchQuery Mundane Record';
const zephyrToken = 'Zephyr';
// Pagination content - a shared token plus a sortable suffix so ordering is deterministic
const pagingToken = 'DeliveryApiSearchQueryPagingItem';
const pagingContentNames = [
  pagingToken + 'A',
  pagingToken + 'B',
  pagingToken + 'C',
  pagingToken + 'D',
  pagingToken + 'E',
];
// Lifecycle content - each name carries a distinct word token so a name filter isolates it
const unpublishContentName = 'DeliveryApiSearchQuery Unpublishable Item';
const unpublishToken = 'Unpublishable';
const trashContentName = 'DeliveryApiSearchQuery Trashable Item';
const trashToken = 'Trashable';
const renameBeforeContentName = 'DeliveryApiSearchQuery Originalword Item';
const renameBeforeToken = 'Originalword';
const renameAfterContentName = 'DeliveryApiSearchQuery Updatedword Item';
const renameAfterToken = 'Updatedword';

let documentTypeId = '';

async function createAndPublishDocument(umbracoApi, name: string): Promise<string> {
  const contentId = await umbracoApi.document.createDefaultDocument(name, documentTypeId) ?? '';
  await umbracoApi.document.publish(contentId);
  return contentId;
}

async function queryContent(umbracoApi, filter?: string, sort?: string, skip?: number, take?: number) {
  const response = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, undefined, filter, sort, skip, take);
  return await response.json();
}

// Content indexing is asynchronous and its latency varies, so poll the query itself rather than a flat wait
// followed by a single check - a flat wait can consistently undershoot under load.
async function queryUntilNamesPresent(umbracoApi, filter: string | undefined, sort: string | undefined, expectedNames: string[]) {
  let contentItemsJson;
  await expect
    .poll(
      async () => {
        contentItemsJson = await queryContent(umbracoApi, filter, sort, 0, 100);
        const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
        return expectedNames.every((name) => returnedNames.includes(name));
      },
      {timeout: ConstantHelper.timeout.pageLoad},
    )
    .toBe(true);
  return contentItemsJson;
}

// De-indexing is asynchronous, so poll until the query stops matching. Assert on total rather than on the
// returned items: total comes from the index, whereas items are the index hits mapped through the published-
// content cache (GetByIds().WhereNotNull()). A stale index that still matches the document would be masked at
// the items level - the cache alone drops an unpublished doc, and after a rename it already reports the new
// name - so only total dropping to zero proves the document actually left the index.
async function queryUntilTotalIsZero(umbracoApi, filter: string | undefined) {
  await expect
    .poll(async () => (await queryContent(umbracoApi, filter)).total, {timeout: ConstantHelper.timeout.pageLoad})
    .toBe(0);
}

test.beforeEach(async ({umbracoApi}) => {
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
    const contentItemsJson = await queryUntilNamesPresent(umbracoApi, 'name:' + zephyrToken, undefined, [zephyrContentNameA, zephyrContentNameB]);

    // Assert - only the two names containing the token match; the mundane document must be excluded
    const returnedNames = contentItemsJson.items.map((item: {name: string}) => item.name);
    expect(returnedNames).not.toContain(mundaneContentName);
    expect(contentItemsJson.total).toBe(2);
  });

  test('can match a name case-insensitively', async ({umbracoApi}) => {
    // Act - the token is stored capitalised ("Zephyr") but queried in lower case
    const contentItemsJson = await queryUntilNamesPresent(umbracoApi, 'name:' + zephyrToken.toLowerCase(), undefined, [zephyrContentNameA, zephyrContentNameB]);

    // Assert
    expect(contentItemsJson.total).toBe(2);
  });

  test('returns no items for a non-matching term', async ({umbracoApi}) => {
    // Arrange - prove indexing has landed so an empty result can only mean "no match"
    await queryUntilNamesPresent(umbracoApi, 'name:' + zephyrToken, undefined, [zephyrContentNameA]);

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
    for (const name of pagingContentNames) {
      await createAndPublishDocument(umbracoApi, name);
    }
    const filter = 'name:' + pagingToken;
    const sort = 'name:asc';

    // Wait until every document is queryable before paging, otherwise page boundaries shift under async indexing.
    await queryUntilNamesPresent(umbracoApi, filter, sort, pagingContentNames);

    // Act
    const firstPage = await queryContent(umbracoApi, filter, sort, 0, 2);
    const secondPage = await queryContent(umbracoApi, filter, sort, 2, 2);
    const thirdPage = await queryContent(umbracoApi, filter, sort, 4, 2);

    // Assert - total reflects the full match count on every page, independent of take
    expect(firstPage.total).toBe(pagingContentNames.length);
    expect(secondPage.total).toBe(pagingContentNames.length);
    expect(thirdPage.total).toBe(pagingContentNames.length);

    // take caps the page length; the final page holds the remainder
    expect(firstPage.items.length).toBe(2);
    expect(secondPage.items.length).toBe(2);
    expect(thirdPage.items.length).toBe(1);

    // Pages are disjoint and ordered, so concatenating them reproduces the sorted set exactly
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
    await queryUntilNamesPresent(umbracoApi, filter, undefined, [unpublishContentName]);

    // Act
    await umbracoApi.document.unpublish(contentId);

    // Assert - the index must drop the now-unpublished document, not just the published content cache
    await queryUntilTotalIsZero(umbracoApi, filter);
  });

  test('excludes a document moved to the recycle bin from query results', async ({umbracoApi}) => {
    // Arrange
    const contentId = await createAndPublishDocument(umbracoApi, trashContentName);
    const filter = 'name:' + trashToken;
    await queryUntilNamesPresent(umbracoApi, filter, undefined, [trashContentName]);

    // Act
    await umbracoApi.document.moveToRecycleBin(contentId);

    // Assert
    await queryUntilTotalIsZero(umbracoApi, filter);
  });

  test('reindexes a renamed document under its new name', async ({umbracoApi}) => {
    // Arrange
    const contentId = await createAndPublishDocument(umbracoApi, renameBeforeContentName);
    await queryUntilNamesPresent(umbracoApi, 'name:' + renameBeforeToken, undefined, [renameBeforeContentName]);

    // Act - rename and republish
    const document = await umbracoApi.document.getByName(renameBeforeContentName);
    document.variants[0].name = renameAfterContentName;
    await umbracoApi.document.update(contentId, document);
    await umbracoApi.document.publish(contentId);

    // Assert - the index reflects the update: the old token no longer matches, the new token now does
    await queryUntilTotalIsZero(umbracoApi, 'name:' + renameBeforeToken);
    await queryUntilNamesPresent(umbracoApi, 'name:' + renameAfterToken, undefined, [renameAfterContentName]);
  });
});
