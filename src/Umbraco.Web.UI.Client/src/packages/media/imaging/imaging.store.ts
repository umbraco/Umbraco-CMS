import { UMB_IMAGING_STORE_CONTEXT } from './imaging.store.token.js';
import type { UmbImagingModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';

export class UmbImagingStore extends UmbContextBase<never> implements UmbApi {
	#data;

	constructor(host: UmbControllerHost) {
		super(host, UMB_IMAGING_STORE_CONTEXT.toString());
		this.#data = new Map<string, Map<string, string>>();
	}

	/**
	 * Gets the data from the store.
	 * @param unique
	 */
	getData(unique: string) {
		return this.#data.get(unique);
	}

	/**
	 * Gets a specific crop if it exists.
	 * @param unique
	 * @param data
	 */
	getCrop(unique: string, data?: UmbImagingModel) {
		return this.#data.get(unique)?.get(this.#generateCropKey(data));
	}

	/**
	 * Adds a new crop to the store.
	 * @param unique
	 * @param urlInfo
	 * @param data
	 */
	addCrop(unique: string, urlInfo: string, data?: UmbImagingModel) {
		if (!this.#data.has(unique)) {
			this.#data.set(unique, new Map());
		}
		this.#data.get(unique)?.set(this.#generateCropKey(data), urlInfo);
	}

	/**
	 * Generates a unique key for the crop based on the width, height and mode.
	 * @param data
	 */
	#generateCropKey(data?: UmbImagingModel) {
		return data ? `${data.width}x${data.height};${data.mode}` : 'generic';
	}
}

export default UmbImagingStore;
