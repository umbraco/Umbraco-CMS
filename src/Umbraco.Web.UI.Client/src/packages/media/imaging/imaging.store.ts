import { UMB_IMAGING_STORE_CONTEXT } from './imaging.store.token.js';
import type { UmbImagingResizeModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';

export class UmbImagingStore extends UmbContextBase implements UmbApi {
	#data;

	constructor(host: UmbControllerHost) {
		super(host, UMB_IMAGING_STORE_CONTEXT.toString());
		this.#data = new Map<string, Map<string, string>>();
	}

	/**
	 * Gets the data from the store.
	 * @param {string} unique - The media key
	 * @returns {Map<string, string> | undefined} - The data if it exists
	 */
	getData(unique: string) {
		return this.#data.get(unique);
	}

	/**
	 * Gets a specific crop if it exists.
	 * @param {string} unique - The media key
	 * @param {string} data - The resize configuration
	 * @returns {string | undefined} - The crop if it exists
	 */
	getCrop(unique: string, data?: UmbImagingResizeModel) {
		return this.#data.get(unique)?.get(this.#generateCropKey(data));
	}

	/**
	 * Adds a new crop to the store.
	 * @param {string} unique - The media key
	 * @param {string} urlInfo - The URL of the crop
	 * @param { | undefined} data - The resize configuration
	 */
	addCrop(unique: string, urlInfo: string, data?: UmbImagingResizeModel) {
		if (!this.#data.has(unique)) {
			this.#data.set(unique, new Map());
		}
		this.#data.get(unique)?.set(this.#generateCropKey(data), urlInfo);
	}

	/**
	 * Generates a unique key for the crop based on the width, height and mode.
	 * @param {UmbImagingResizeModel} data - The resize configuration
	 * @returns {string} - The crop key
	 */
	#generateCropKey(data?: UmbImagingResizeModel) {
		return data ? `${data.width}x${data.height};${data.mode}` : 'generic';
	}
}

export default UmbImagingStore;
