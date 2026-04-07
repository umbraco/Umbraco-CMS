import { UmbImagingCropMode } from './types.js';
import type { UmbImagingResizeModel } from './types.js';
import { batchImagingRequest } from './imaging-request-batcher.js';
import { UmbImagingServerDataSource } from './imaging.server.data.js';
import { UMB_IMAGING_STORE_CONTEXT } from './imaging.store.token.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { consumeContext } from '@umbraco-cms/backoffice/context-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbMediaUrlModel } from '@umbraco-cms/backoffice/media';

/**
 * Repository for managing imaging-related data.
 * You can use it to request (cached) thumbnail URLs or new resized images.
 */
export class UmbImagingRepository extends UmbRepositoryBase implements UmbApi {
	#itemSource = new UmbImagingServerDataSource(this);

	@consumeContext({ context: UMB_IMAGING_STORE_CONTEXT })
	private _dataStore?: typeof UMB_IMAGING_STORE_CONTEXT.TYPE;

	/**
	 * Requests the items for the given uniques
	 * @param {Array<string>} uniques - The uniques
	 * @param {UmbImagingResizeModel} imagingModel - The imaging model
	 * @returns {Promise<{ data: UmbMediaUrlModel[] }>} - The resized absolute media URLs
	 * @memberof UmbImagingRepository
	 */
	async requestResizedItems(
		uniques: Array<string>,
		imagingModel?: UmbImagingResizeModel,
	): Promise<{ data: UmbMediaUrlModel[] }> {
		if (!uniques.length) throw new Error('Uniques are missing');

		const urls = new Map<string, string>();
		const uncachedUniques: Array<string> = [];

		// Serve what we can from the in-memory crop cache.
		for (const unique of uniques) {
			// The store is used opportunistically.
			// If not yet available, everything is a cache miss and we fetch.
			const existingCrop = this._dataStore?.getCrop(unique, imagingModel);
			if (existingCrop !== undefined) {
				urls.set(unique, existingCrop);
			} else {
				uncachedUniques.push(unique);
			}
		}

		// For cache misses, hand each unique to the request batcher.
		// The batcher collects all requests that arrive within the same event loop
		// turn (e.g. many <umb-imaging-thumbnail> elements becoming visible at once)
		// and combines them into a single API call to /imaging/resize/urls. This turns
		// what was previously N sequential HTTP requests into one batched request,
		// significantly reducing network overhead when rendering media collections
		// or picker grids.
		//
		// Each call to batchImagingRequest returns a Promise that resolves with
		// the URL for that specific unique once the shared batch completes.
		// Promise.allSettled lets us handle partial failures: if one item errors,
		// the rest still resolve and get cached normally.
		if (uncachedUniques.length > 0) {
			const results = await Promise.allSettled(
				uncachedUniques.map((unique) =>
					batchImagingRequest(unique, imagingModel, (batchUniques, model) =>
						this.#itemSource.getItems(batchUniques, model),
					),
				),
			);

			// Populate the cache with results so subsequent requests are instant.
			for (let i = 0; i < uncachedUniques.length; i++) {
				const result = results[i];
				if (result.status === 'fulfilled') {
					const url = result.value;
					this._dataStore?.addCrop(uncachedUniques[i], url ?? '', imagingModel);
					if (url) {
						urls.set(uncachedUniques[i], url);
					}
				} else {
					console.error('[UmbImagingRepository] Error fetching item', result.reason);
				}
			}
		}

		return { data: Array.from(urls).map(([unique, url]) => ({ unique, url })) };
	}

	/**
	 * Internal method to clear the cache for a specific image.
	 * @param {string} unique The unique identifier for the media item
	 * @internal
	 */
	// eslint-disable-next-line @typescript-eslint/naming-convention
	async _internal_clearCropByUnique(unique: string) {
		this._dataStore?.clearCropByUnique(unique);
	}

	/**
	 * Requests the thumbnail URLs for the given uniques
	 * @param {Array<string>} uniques - The unique identifiers for the media items
	 * @param {number} height - The desired height in pixels of the thumbnail
	 * @param {number} width - The desired width in pixels of the thumbnail
	 * @param {UmbImagingCropMode} mode - The crop mode
	 * @returns {Promise<{ data: UmbMediaUrlModel[] }>} - The resized absolute media URLs
	 * @memberof UmbImagingRepository
	 * @deprecated Use {@link UmbImagingRepository#requestResizedItems} instead for more flexibility and control over the imaging model. This will be removed in Umbraco 18.
	 */
	async requestThumbnailUrls(
		uniques: Array<string>,
		height: number,
		width: number,
		mode: UmbImagingCropMode = UmbImagingCropMode.MIN,
	): Promise<{ data: UmbMediaUrlModel[] }> {
		const imagingModel: UmbImagingResizeModel = { height, width, mode };
		return this.requestResizedItems(uniques, imagingModel);
	}
}

export { UmbImagingRepository as api };
