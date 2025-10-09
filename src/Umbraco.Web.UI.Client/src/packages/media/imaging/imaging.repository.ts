import { UmbImagingCropMode, type UmbImagingResizeModel } from './types.js';
import { UmbImagingServerDataSource } from './imaging.server.data.js';
import { UMB_IMAGING_STORE_CONTEXT } from './imaging.store.token.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbMediaUrlModel } from '@umbraco-cms/backoffice/media';

/**
 * Repository for managing imaging-related data.
 * You can use it to request (cached) thumbnail URLs or new resized images.
 */
export class UmbImagingRepository extends UmbRepositoryBase implements UmbApi {
	#init;
	#itemSource = new UmbImagingServerDataSource(this);
	#dataStore?: typeof UMB_IMAGING_STORE_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#init = this.consumeContext(UMB_IMAGING_STORE_CONTEXT, (instance) => {
			if (instance) {
				this.#dataStore = instance;
			}
		}).asPromise({ preventTimeout: true });
	}

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

		await this.#init;

		if (!this.#dataStore) {
			console.warn('[UmbImagingRepository] No data store available. All thumbnails are uncached.');
		}

		const urls = new Map<string, string>();

		for (const unique of uniques) {
			const existingCrop = this.#dataStore?.getCrop(unique, imagingModel);
			if (existingCrop !== undefined) {
				urls.set(unique, existingCrop);
				continue;
			}

			const { data: urlModels, error } = await this.#itemSource.getItems([unique], imagingModel);

			if (error) {
				console.error('[UmbImagingRepository] Error fetching items', error);
				continue;
			}

			const url = urlModels?.[0]?.url;

			this.#dataStore?.addCrop(unique, url ?? '', imagingModel);

			if (url) {
				urls.set(unique, url);
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
		await this.#init;
		this.#dataStore?.clearCropByUnique(unique);
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
