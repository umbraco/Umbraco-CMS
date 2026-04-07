import { UMB_IMAGING_STORE_CONTEXT } from './imaging.store.token.js';
import { generateImagingCacheKey, type UmbImagingResizeModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * @deprecated Imaging URL caching is now handled by the request batcher module.
 * Use {@link batchImagingRequest} for cached URL lookups and {@link clearImagingCache} for
 * cache invalidation. This class will be removed in Umbraco 19.
 */
export class UmbImagingStore extends UmbContextBase implements UmbApi {
	#data = new Map<string, Map<string, string>>();

	#hasWarned = false;

	constructor(host: UmbControllerHost) {
		super(host, UMB_IMAGING_STORE_CONTEXT.toString());
	}

	#warnDeprecation() {
		if (this.#hasWarned) return;
		this.#hasWarned = true;
		new UmbDeprecation({
			removeInVersion: '19.0.0',
			deprecated: 'UmbImagingStore',
			solution:
				'Imaging URL caching is now handled by the request batcher. Use batchImagingRequest() for cached lookups and clearImagingCache() for invalidation, both from @umbraco-cms/backoffice/imaging',
		}).warn();
	}

	/**
	 * Gets the data from the store.
	 * @param {string} unique - The media key
	 * @returns {Map<string, string> | undefined} - The data if it exists
	 */
	getData(unique: string): Map<string, string> | undefined {
		this.#warnDeprecation();
		return this.#data.get(unique);
	}

	/**
	 * Gets a specific crop if it exists.
	 * @param {string} unique - The media key
	 * @param {string} data - The resize configuration
	 * @returns {string | undefined} - The crop if it exists
	 */
	getCrop(unique: string, data?: UmbImagingResizeModel): string | undefined {
		this.#warnDeprecation();
		return this.#data.get(unique)?.get(generateImagingCacheKey(data));
	}

	/**
	 * Adds a new crop to the store.
	 * @param {string} unique - The media key
	 * @param {string} urlInfo - The URL of the crop
	 * @param {UmbImagingResizeModel | undefined} data - The resize configuration
	 */
	addCrop(unique: string, urlInfo: string, data?: UmbImagingResizeModel) {
		this.#warnDeprecation();
		if (!this.#data.has(unique)) {
			this.#data.set(unique, new Map());
		}
		this.#data.get(unique)?.set(generateImagingCacheKey(data), urlInfo);
	}

	/**
	 * Clears all crops from the store.
	 */
	clear() {
		this.#warnDeprecation();
		this.#data.clear();
	}

	/**
	 * Clears the crop for a specific unique identifier.
	 * @param {string} unique - The unique identifier for the media item
	 */
	clearCropByUnique(unique: string) {
		this.#warnDeprecation();
		this.#data.delete(unique);
	}

	/**
	 * Clears the crop for a specific unique identifier and resize configuration.
	 * @param {string} unique - The unique identifier for the media item
	 * @param {UmbImagingResizeModel | undefined} data - The resize configuration
	 */
	clearCropByConfiguration(unique: string, data?: UmbImagingResizeModel) {
		this.#warnDeprecation();
		this.#data.get(unique)?.delete(generateImagingCacheKey(data));
	}
}

export default UmbImagingStore;
