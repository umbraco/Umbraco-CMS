import { UmbStylesheetItemServerDataSource } from './stylesheet-item.server.data.js';
import { UMB_STYLESHEET_ITEM_STORE_CONTEXT_TOKEN, UmbStylesheetItemStore } from './stylesheet-item.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemDataSource, UmbItemRepository, UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { StylesheetItemResponseModel, UserItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbStylesheetItemRepository extends UmbRepositoryBase implements UmbItemRepository<UserItemResponseModel> {
	#init;
	#itemSource: UmbItemDataSource<StylesheetItemResponseModel>;
	#itemStore?: UmbStylesheetItemStore;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#itemSource = new UmbStylesheetItemServerDataSource(host);

		this.#init = this.consumeContext(UMB_STYLESHEET_ITEM_STORE_CONTEXT_TOKEN, (instance) => {
			this.#itemStore = instance;
		}).asPromise();
	}

	/**
	 * Requests the stylesheet items for the given ids
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbStylesheetItemRepository
	 */
	async requestItems(ids: Array<string>) {
		if (!ids) throw new Error('Ids are missing');
		await this.#init;

		const { data, error } = await this.#itemSource.getItems(ids);

		if (data) {
			this.#itemStore?.appendItems(data);
		}

		return { data, error, asObservable: () => this.#itemStore!.items(ids) };
	}

	/**
	 * Returns a promise with an observable of the stylesheet items for the given ids
	 * @param {Array<string>} ids
	 * @return {Promise<Observable<ItemResponseModelBaseModel[]>>}
	 * @memberof UmbStylesheetItemRepository
	 */
	async items(ids: Array<string>) {
		await this.#init;
		return this.#itemStore!.items(ids);
	}
}
