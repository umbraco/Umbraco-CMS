import { UmbRepositoryBase } from '../repository-base.js';
import type { UmbItemDataSource, UmbItemDataSourceConstructor } from './item-data-source.interface.js';
import type { UmbItemRepository } from './item-repository.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbItemStore } from '@umbraco-cms/backoffice/store';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { of } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbItemRepositoryBase<ItemType extends { unique: string }>
	extends UmbRepositoryBase
	implements UmbItemRepository<ItemType>
{
	protected _init: Promise<unknown>;
	protected _itemStore?: UmbItemStore<ItemType>;
	#itemSource: UmbItemDataSource<ItemType>;

	constructor(
		host: UmbControllerHost,
		itemSource: UmbItemDataSourceConstructor<ItemType>,
		itemStoreContextAlias: string | UmbContextToken<any, any>,
	) {
		super(host);
		this.#itemSource = new itemSource(host);

		this._init = this.consumeContext(itemStoreContextAlias, (instance) => {
			this._itemStore = instance;
		})
			.asPromise({ preventTimeout: true })
			// Ignore the error, we can assume that the flow was stopped (asPromise failed), but it does not mean that the consumption was not successful.
			.catch(() => undefined);
	}

	/**
	 * Requests the items for the given uniques
	 * @param {Array<string>} uniques
	 * @returns {*}
	 * @memberof UmbItemRepositoryBase
	 */
	async requestItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');
		try {
			await this._init;
		} catch {
			return {
				asObservable: () => undefined,
			};
		}

		const { data, error } = await this.#itemSource.getItems(uniques);

		if (data) {
			this._itemStore?.appendItems(data);
		}

		return { data, error, asObservable: () => this._itemStore?.items(uniques) };
	}

	/**
	 * Returns a promise with an observable of the items for the given uniques
	 * @param {Array<string>} uniques
	 * @returns {*}
	 * @memberof UmbItemRepositoryBase
	 */
	async items(uniques: Array<string>) {
		try {
			await this._init;
		} catch {
			return undefined;
		}

		if (!this._itemStore) {
			// If store is gone, then we are most likely in a disassembled state.
			return of([]);
		}

		return this._itemStore.items(uniques);
	}
}
