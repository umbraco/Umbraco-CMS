import { UmbRepositoryBase } from '../repository-base.js';
import { UmbItemDataSource, UmbItemDataSourceConstructor } from './item-data-source.interface.js';
import { UmbItemRepository } from './item-repository.interface.js';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStore } from '@umbraco-cms/backoffice/store';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbItemRepositoryBase<ItemType extends { unique: string } = any>
	extends UmbRepositoryBase
	implements UmbItemRepository<ItemType>
{
	protected _init: Promise<unknown>;
	protected _itemStore?: UmbItemStore<ItemType>;
	#itemSource: UmbItemDataSource;

	constructor(
		host: UmbControllerHost,
		itemSource: UmbItemDataSourceConstructor,
		itemStoreContextAlias: string | UmbContextToken<any, any>,
	) {
		super(host);
		this.#itemSource = new itemSource(host);

		this._init = this.consumeContext(itemStoreContextAlias, (instance) => {
			this._itemStore = instance as UmbItemStore<ItemType>;
		}).asPromise();
	}

	/**
	 * Requests the items for the given uniques
	 * @param {Array<string>} uniques
	 * @return {*}
	 * @memberof UmbItemRepositoryBase
	 */
	async requestItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');
		await this._init;

		const { data, error } = await this.#itemSource.getItems(uniques);

		if (data) {
			this._itemStore!.appendItems(data);
		}

		return { data, error, asObservable: () => this._itemStore!.items(uniques) };
	}

	/**
	 * Returns a promise with an observable of the items for the given uniques
	 * @param {Array<string>} uniques
	 * @return {*}
	 * @memberof UmbItemRepositoryBase
	 */
	async items(uniques: Array<string>) {
		await this._init;
		return this._itemStore!.items(uniques);
	}
}
