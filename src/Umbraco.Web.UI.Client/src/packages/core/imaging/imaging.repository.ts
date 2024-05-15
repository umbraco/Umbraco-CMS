import type { UmbImagingModel } from './types.js';
import { UmbImagingServerDataSource } from './imaging.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbImagingRepository extends UmbControllerBase implements UmbApi {
	//protected _init: Promise<unknown>;
	//protected _itemStore?: UmbImagingStore;
	#itemSource: UmbImagingServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#itemSource = new UmbImagingServerDataSource(host);

		/*this._init = this.consumeContext(UMB_IMAGING_STORE_CONTEXT, (instance) => {
			this._itemStore = instance as UmbImagingStore;
		}).asPromise();*/
	}

	/**
	 * Requests the items for the given uniques
	 * @param {Array<string>} uniques
	 * @return {*}
	 * @memberof UmbImagingRepository
	 */
	async requestResizedItems({ uniques, height, width, mode }: UmbImagingModel) {
		if (!uniques.length) throw new Error('Uniques are missing');
		//await this._init;

		const { data, error: _error } = await this.#itemSource.getItems({ uniques, height, width, mode });
		const error: any = _error;
		/*if (data) {
			this._itemStore!.appendItems(data);
		}
		return { data, error, asObservable: () => this._itemStore!.items(uniques) };*/

		return { data, error };
	}
}
