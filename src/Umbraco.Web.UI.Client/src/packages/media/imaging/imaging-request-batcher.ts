import { generateImagingCacheKey, type UmbImagingResizeModel } from './types.js';
import { batchArray } from '@umbraco-cms/backoffice/utils';

const BATCH_SIZE = 40;

interface BatchedItem {
	unique: string;
	url?: string;
}

type FetchFn = (
	uniques: Array<string>,
	model?: UmbImagingResizeModel,
) => Promise<{ data?: Array<BatchedItem>; error?: unknown }>;

interface PendingRequest {
	resolve: (url: string | undefined) => void;
	reject: (err: unknown) => void;
}

interface PendingBatch {
	requests: Map<string, Array<PendingRequest>>;
	fetchFn: FetchFn;
	imagingModel?: UmbImagingResizeModel;
}

const pendingBatches = new Map<string, PendingBatch>();

/** Module-level URL cache: model key → (unique → url). */
const urlCache = new Map<string, Map<string, string>>();

/**
 * Flushes a pending batch, executing the API call and resolving all waiting promises.
 * @param {string} key - The batch key identifying the batch
 */
async function flush(key: string) {
	const batch = pendingBatches.get(key);

	if (!batch) return;

	try {
		pendingBatches.delete(key);

		const uniqueArray = Array.from(batch.requests.keys());
		let allItems: Array<BatchedItem> = [];
		const failedUniques = new Set<string>();
		let firstError: unknown;

		if (uniqueArray.length > BATCH_SIZE) {
			const chunks = batchArray(uniqueArray, BATCH_SIZE);
			const results = await Promise.allSettled(chunks.map((chunk) => batch.fetchFn(chunk, batch.imagingModel)));

			// A chunk can fail in two ways: the promise rejects (thrown error),
			// or it resolves with { error } (tryExecute convention). Both must
			// be treated as failures so callers get rejected rather than silently
			// receiving undefined and caching an empty string.

			for (let i = 0; i < results.length; i++) {
				const result = results[i];
				if (result.status === 'rejected') {
					firstError ??= result.reason;
					for (const unique of chunks[i]) {
						failedUniques.add(unique);
					}
				} else if (result.value.error) {
					firstError ??= result.value.error;
					for (const unique of chunks[i]) {
						failedUniques.add(unique);
					}
				} else if (result.value.data) {
					allItems = allItems.concat(result.value.data);
				}
			}

			// If every chunk failed, reject all callers with the first error.
			if (failedUniques.size === uniqueArray.length) {
				rejectAll(batch, firstError);
				return;
			}
		} else {
			const { data, error } = await batch.fetchFn(uniqueArray, batch.imagingModel);

			if (error) {
				rejectAll(batch, error);
				return;
			}

			allItems = data ?? [];
		}

		// Build a lookup from unique -> url and populate the cache.
		const urlMap = new Map<string, string | undefined>();
		let cacheForKey = urlCache.get(key);
		if (!cacheForKey) {
			cacheForKey = new Map();
			urlCache.set(key, cacheForKey);
		}
		for (const item of allItems) {
			// Cache both present and absent URLs. A missing URL (e.g. non-image
			// media like PDFs) is a valid result that should not trigger re-fetching.
			const url = item.url ?? '';
			urlMap.set(item.unique, url);
			cacheForKey.set(item.unique, url);
		}

		// Resolve successful callers; reject those whose chunk failed.
		for (const [unique, requests] of batch.requests) {
			if (failedUniques.has(unique)) {
				for (const request of requests) {
					request.reject(firstError);
				}
			} else {
				const url = urlMap.get(unique);
				for (const request of requests) {
					request.resolve(url);
				}
			}
		}
	} catch (error) {
		rejectAll(batch, error);
	}
}

/**
 * Rejects all pending requests in a batch with the given error.
 * @param {PendingBatch} batch - The batch whose requests should be rejected
 * @param {unknown} error - The error to reject with
 */
function rejectAll(batch: PendingBatch, error: unknown) {
	for (const requests of batch.requests.values()) {
		for (const request of requests) {
			request.reject(error);
		}
	}
}

/**
 * Batches individual imaging resize requests together.
 * Multiple calls within the same event loop turn are collected and sent as a single API request,
 * grouped by resize model configuration (width/height/mode/format).
 * @param {string} unique - The media item unique identifier
 * @param {UmbImagingResizeModel | undefined} imagingModel - The resize configuration
 * @param {FetchFn} fetchFn - The function to call with batched uniques
 * @returns {Promise<string | undefined>} A promise resolving to the URL for this specific unique, or undefined if not found
 */
export function batchImagingRequest(
	unique: string,
	imagingModel: UmbImagingResizeModel | undefined,
	fetchFn: FetchFn,
): Promise<string | undefined> {
	const key = generateImagingCacheKey(imagingModel);

	// Return cached URL immediately if available.
	const cached = urlCache.get(key)?.get(unique);
	if (cached !== undefined) {
		return Promise.resolve(cached);
	}

	let batch = pendingBatches.get(key);
	if (!batch) {
		batch = {
			requests: new Map(),
			fetchFn,
			imagingModel,
		};
		pendingBatches.set(key, batch);

		// Macrotask flush: Lit renders via microtasks, so setTimeout(0) waits for
		// a full render pass to complete before flushing. Multiple passes produce
		// separate batches, which is fine — pipelining paints results sooner.
		setTimeout(() => flush(key), 0);
	}

	return new Promise<string | undefined>((resolve, reject) => {
		let requests = batch!.requests.get(unique);
		if (!requests) {
			requests = [];
			batch!.requests.set(unique, requests);
		}
		requests.push({ resolve, reject });
	});
}

/**
 * Clears all cached imaging URLs for the given unique identifier.
 * Call this when a media item is saved or deleted to ensure fresh URLs are fetched.
 * @param {string} unique - The media item unique identifier to clear from cache
 */
export function clearImagingCache(unique: string): void {
	for (const cacheForKey of urlCache.values()) {
		cacheForKey.delete(unique);
	}
}

/**
 * Clears the entire imaging URL cache.
 * @internal
 */
export function clearAllImagingCache(): void {
	urlCache.clear();
}
