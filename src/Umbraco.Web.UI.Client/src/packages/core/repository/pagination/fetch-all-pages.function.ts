import type { UmbDataSourceResponse } from '../data-source-response.interface.js';
import type { UmbPagedModel } from '../types.js';

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
 * If the server reports a higher `total` than it actually delivers — i.e. an empty page is returned
 * before `allItems.length` reaches `total` — the function fails with an error rather than silently
 * truncating, since a partial "fetch all" result would mislead the caller.
 * @param {UmbOffsetPageFetcher} fetchPage - Called once per page with the current `skip` and `take`.
 * @param {number} take - Page size used for every request. Must be a positive finite number.
 * @returns {Promise} A promise resolving to all items, or the first error encountered.
 * @throws {RangeError} If `take` is not a positive finite number.
 */
export async function fetchAllPages<T>(
	fetchPage: UmbOffsetPageFetcher<T>,
	take: number,
): Promise<UmbDataSourceResponse<UmbPagedModel<T>>> {
	if (!Number.isFinite(take) || take <= 0) {
		throw new RangeError(`fetchAllPages: \`take\` must be a positive finite number, got ${take}.`);
	}

	const allItems: Array<T> = [];
	let skip = 0;
	let total = Number.POSITIVE_INFINITY;

	while (allItems.length < total) {
		const { data, error } = await fetchPage(skip, take);
		if (error) return { error };
		if (!data) return { error: new Error('fetchAllPages: page fetcher returned neither data nor error.') };

		// If the server reports more items than it delivers, fail rather than silently truncating —
		// also guards against an infinite loop on a misbehaving source.
		if (data.items.length === 0 && allItems.length < data.total) {
			return {
				error: new Error(
					`fetchAllPages: page fetcher returned an empty page after ${allItems.length} items but reported a total of ${data.total}.`,
				),
			};
		}

		allItems.push(...data.items);
		total = data.total;
		skip += data.items.length;
	}

	return { data: { items: allItems, total: allItems.length } };
}
