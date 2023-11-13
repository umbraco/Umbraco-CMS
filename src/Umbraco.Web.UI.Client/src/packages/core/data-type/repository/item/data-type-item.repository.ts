import { UmbDataTypeRepositoryBase } from '../data-type-repository-base.js';
import { UmbDataTypeItemServerDataSource } from './data-type-item.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemDataSource, UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { DataTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbDataTypeItemRepository
	extends UmbDataTypeRepositoryBase
	implements UmbItemRepository<DataTypeItemResponseModel>
{
	#itemSource: UmbItemDataSource<DataTypeItemResponseModel>;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#itemSource = new UmbDataTypeItemServerDataSource(host);
	}

	/**
	 * Requests the Data Type items for the given ids
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbDataTypeItemRepository
	 */
	async requestItems(ids: Array<string>) {
		if (!ids) throw new Error('Ids are missing');
		await this._init;

		const { data, error } = await this.#itemSource.getItems(ids);

		if (data) {
			this._itemStore!.appendItems(data);
		}

		return { data, error, asObservable: () => this._itemStore!.items(ids) };
	}

	/**
	 * Returns a promise with an observable of the Data Type items for the given ids
	 * @param {Array<string>} ids
	 * @return {Promise<Observable<DataTypeItemResponseModel[]>>}
	 * @memberof UmbDataTypeItemRepository
	 */
	async items(ids: Array<string>) {
		await this._init;
		return this._itemStore!.items(ids);
	}
}
