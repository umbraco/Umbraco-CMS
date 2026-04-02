import { UMB_IMAGING_STORE_CONTEXT } from './imaging.store.token.js';
import type { UmbImagingResizeModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';

export class UmbImagingStore extends UmbContextBase implements UmbApi {
	#data = new Map<string, Map<string, string>>();

	constructor(host: UmbControllerHost) {
		super(host, UMB_IMAGING_STORE_CONTEXT.toString());
	}

	/**
	 * Gets the data from the store.
	 * @param {string} unique - The media key
	 * @returns {Map<string, string> | undefined} - The data if it exists
	 */
	getData(unique: string): Map<string, string> | undefined {
		return this.#data.get(unique);
	}

	/**
	 * Gets a specific crop if it exists.
	 * @param {string} unique - The media key
	 * @param {string} data - The resize configuration
	 * @returns {string | undefined} - The crop if it exists
	 */
	getCrop(unique: string, data?: UmbImagingResizeModel): string | undefined {
		return this.#data.get(unique)?.get(this.#generateCropKey(data));
	}

	/**
	 * Adds a new crop to the store.
	 * @param {string} unique - The media key
	 * @param {string} urlInfo - The URL of the crop
	 * @param {UmbImagingResizeModel | undefined} data - The resize configuration
	 */
	addCrop(unique: string, urlInfo: string, data?: UmbImagingResizeModel) {
		if (!this.#data.has(unique)) {
			this.#data.set(unique, new Map());
		}
		this.#data.get(unique)?.set(this.#generateCropKey(data), urlInfo);
	}

	/**
	 * Clears all crops from the store.
	 */
	clear() {
		this.#data.clear();
	}

	/**
	 * Clears the crop for a specific unique identifier.
	 * @param {string} unique - The unique identifier for the media item
	 */
	clearCropByUnique(unique: string) {
		this.#data.delete(unique);
	}

	/**
	 * Clears the crop for a specific unique identifier and resize configuration.
	 * @param {string} unique - The unique identifier for the media item
	 * @param {UmbImagingResizeModel | undefined} data - The resize configuration
	 */
	clearCropByConfiguration(unique: string, data?: UmbImagingResizeModel) {
		this.#data.get(unique)?.delete(this.#generateCropKey(data));
	}

	/**
	 * Generates a unique key for the crop based on the width, height, mode and format.
	 * @param {UmbImagingResizeModel} data - The resize configuration
	 * @returns {string} - The crop key
	 */
	#generateCropKey(data?: UmbImagingResizeModel): string {
		return data ? `${data.width}x${data.height};${data.mode};${data.format}` : 'generic';
	}
}

export default UmbImagingStore;
