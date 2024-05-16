import type { UmbImagingModel } from './types.js';
import { UmbImagingServerDataSource } from './imaging.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbImagingRepository extends UmbControllerBase implements UmbApi {
	#itemSource: UmbImagingServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#itemSource = new UmbImagingServerDataSource(host);
	}

	/**
	 * Requests the items for the given uniques
	 * @param {Array<string>} uniques
	 * @return {*}
	 * @memberof UmbImagingRepository
	 */
	async requestResizedItems(uniques: Array<string>, imagingModel?: UmbImagingModel) {
		if (!uniques.length) throw new Error('Uniques are missing');

		const { data, error: _error } = await this.#itemSource.getItems(uniques, imagingModel);
		const error: any = _error;
		return { data, error };
	}
}
