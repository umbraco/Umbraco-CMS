import { UmbUserRepositoryBase } from '../user-repository-base.js';
import { UmbUserItemServerDataSource } from './user-item.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemDataSource, UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { UserItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbUserItemRepository extends UmbUserRepositoryBase implements UmbItemRepository<UserItemResponseModel> {
	#itemSource: UmbItemDataSource<UserItemResponseModel>;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#itemSource = new UmbUserItemServerDataSource(host);
	}

	/**
	 * Requests the user items for the given ids
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbUserItemRepository
	 */
	async requestItems(ids: Array<string>) {
		if (!ids) throw new Error('Ids are missing');
		await this.init;

		const { data, error } = await this.#itemSource.getItems(ids);

		if (data) {
			this.itemStore?.appendItems(data);
		}

		return { data, error, asObservable: () => this.itemStore!.items(ids) };
	}

	/**
	 * Returns a promise with an observable of the user items for the given ids
	 * @param {Array<string>} ids
	 * @return {Promise<Observable<ItemResponseModelBaseModel[]>>}
	 * @memberof UmbUserItemRepository
	 */
	async items(ids: Array<string>) {
		await this.init;
		return this.itemStore!.items(ids);
	}
}
