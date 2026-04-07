import { UmbImagingCropMode } from './types.js';
import type { UmbImagingResizeModel } from './types.js';
import { batchImagingRequest } from './imaging-request-batcher.js';
import { UmbImagingServerDataSource } from './imaging.server.data.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbMediaUrlModel } from '@umbraco-cms/backoffice/media';

/**
 * Repository for managing imaging-related data.
 * You can use it to request (cached) thumbnail URLs or new resized images.
 */
export class UmbImagingRepository extends UmbRepositoryBase implements UmbApi {
	#itemSource = new UmbImagingServerDataSource(this);

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

		const results = await Promise.allSettled(
			uniques.map((unique) =>
				batchImagingRequest(unique, imagingModel, (batchUniques, model) =>
					this.#itemSource.getItems(batchUniques, model),
				),
			),
		);

		const urls: UmbMediaUrlModel[] = [];
		for (let i = 0; i < uniques.length; i++) {
			const result = results[i];
			if (result.status === 'fulfilled' && result.value) {
				urls.push({ unique: uniques[i], url: result.value });
			} else if (result.status === 'rejected') {
				console.error('[UmbImagingRepository] Error fetching item', result.reason);
			}
		}

		return { data: urls };
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
