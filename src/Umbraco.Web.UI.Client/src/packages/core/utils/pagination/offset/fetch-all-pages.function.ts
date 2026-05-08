import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

/**
 * A function that returns a single page of an offset-paginated collection.
 * @template T - The type of items in the page.
 */
export type UmbOffsetPageFetcher<T> = (
	skip: number,
	take: number,
) => Promise<UmbDataSourceResponse<UmbPagedModel<T>>>;

/**
 * Pages through an offset-paginated data source, accumulating every item until `total` has been reached.
 * Use when a caller genuinely needs the full set rather than a single page — for example, populating a
 * dropdown of every configured language. Returns the same `{ data: { items, total } }` shape as a
 * single-page fetch, or `{ error }` if any page fails.
 *
 * Breaks out if the server reports a total but returns an empty page, to avoid spinning indefinitely
 * on a misbehaving source.
 * @param {UmbOffsetPageFetcher} fetchPage - Called once per page with the current `skip` and `take`.
 * @param {number} take - Page size used for every request.
 * @returns {Promise} A promise resolving to all items, or the first error encountered.
 */
export async function fetchAllPages<T>(
	fetchPage: UmbOffsetPageFetcher<T>,
	take: number,
): Promise<UmbDataSourceResponse<UmbPagedModel<T>>> {
	const allItems: Array<T> = [];
	let skip = 0;
	let total = Number.POSITIVE_INFINITY;

	while (allItems.length < total) {
		const { data, error } = await fetchPage(skip, take);
		if (error || !data) return { error };

		allItems.push(...data.items);
		total = data.total;
		skip += data.items.length;

		// Defensive guard against infinite loops if the server reports a total but returns no items.
		if (data.items.length === 0) break;
	}

	return { data: { items: allItems, total: allItems.length } };
}
