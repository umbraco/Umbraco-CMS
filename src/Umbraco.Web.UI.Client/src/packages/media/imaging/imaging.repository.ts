import { UmbImagingCropMode, type UmbImagingResizeModel } from './types.js';
import { UmbImagingServerDataSource } from './imaging.server.data.js';
import { UMB_IMAGING_STORE_CONTEXT } from './imaging.store.token.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbMediaUrlModel } from '@umbraco-cms/backoffice/media';

export class UmbImagingRepository extends UmbRepositoryBase implements UmbApi {
	#dataStore?: typeof UMB_IMAGING_STORE_CONTEXT.TYPE;
	#itemSource: UmbImagingServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#itemSource = new UmbImagingServerDataSource(host);

		this.consumeContext(UMB_IMAGING_STORE_CONTEXT, (instance) => {
			this.#dataStore = instance;
		});
	}

	/**
	 * Requests the items for the given uniques
	 * @param {Array<string>} uniques - The uniques
	 * @param {UmbImagingResizeModel} imagingModel - The imaging model
	 * @returns {Promise<{ data: UmbMediaUrlModel[] }>}
	 * @memberof UmbImagingRepository
	 */
	async requestResizedItems(
		uniques: Array<string>,
		imagingModel?: UmbImagingResizeModel,
	): Promise<{ data: UmbMediaUrlModel[] }> {
		if (!uniques.length) throw new Error('Uniques are missing');
		if (!this.#dataStore) throw new Error('Data store is missing');

		const urls = new Map<string, string>();

		for (const unique of uniques) {
			const existingCrop = this.#dataStore.getCrop(unique, imagingModel);
			if (existingCrop !== undefined) {
				urls.set(unique, existingCrop);
				continue;
			}

			const { data: urlModels, error } = await this.#itemSource.getItems([unique], imagingModel);

			if (error) {
				console.error('[UmbImagingRepository] Error fetching items', error);
				continue;
			}

			const url = urlModels?.[0].url;

			this.#dataStore.addCrop(unique, url ?? '', imagingModel);

			if (url) {
				urls.set(unique, url);
			}
		}

		return { data: Array.from(urls).map(([unique, url]) => ({ unique, url })) };
	}

	/**
	 * Requests the thumbnail URLs for the given uniques
	 * @param {Array<string>} uniques
	 * @param {number} height
	 * @param {number} width
	 * @param {ImageCropModeModel} mode - The crop mode
	 * @memberof UmbImagingRepository
	 */
	async requestThumbnailUrls(uniques: Array<string>, height: number, width: number, mode = UmbImagingCropMode.MIN) {
		const imagingModel: UmbImagingResizeModel = { height, width, mode };
		return this.requestResizedItems(uniques, imagingModel);
	}
}

export { UmbImagingRepository as api };
