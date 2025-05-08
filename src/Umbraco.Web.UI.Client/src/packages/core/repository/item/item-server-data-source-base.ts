import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbDataSourceResponse } from '../data-source-response.interface.js';
import type { UmbItemDataSource } from './item-data-source.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export interface UmbItemServerDataSourceBaseArgs<ServerItemType, ClientItemType extends { unique: string }> {
	getItems?: (uniques: Array<string>) => Promise<UmbDataSourceResponse<Array<ServerItemType>>>;
	mapper: (item: ServerItemType) => ClientItemType;
}

/**
 * A data source base for items that fetches items from the server
 * @class UmbItemServerDataSourceBase
 * @implements {DocumentTreeDataSource}
 */
export abstract class UmbItemServerDataSourceBase<ServerItemType, ClientItemType extends { unique: string }>
	extends UmbControllerBase
	implements UmbItemDataSource<ClientItemType>
{
	#getItems?: (uniques: Array<string>) => Promise<UmbDataSourceResponse<Array<ServerItemType>>>;
	#mapper: (item: ServerItemType) => ClientItemType;

	/**
	 * Creates an instance of UmbItemServerDataSourceBase.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @param args
	 * @memberof UmbItemServerDataSourceBase
	 */
	constructor(host: UmbControllerHost, args: UmbItemServerDataSourceBaseArgs<ServerItemType, ClientItemType>) {
		super(host);
		this.#getItems = args.getItems;
		this.#mapper = args.mapper;
	}

	/**
	 * Fetches the items for the given uniques from the server
	 * @param {Array<string>} uniques
	 * @returns {*}
	 * @memberof UmbItemServerDataSourceBase
	 */
	async getItems(uniques: Array<string>) {
		if (!this.#getItems) throw new Error('getItems is not implemented');
		if (!uniques) throw new Error('Uniques are missing');

		const { data, error } = await tryExecute(this, this.#getItems(uniques));

		return { data: this._getMappedItems(data), error };
	}

	protected _getMappedItems(items: Array<ServerItemType> | undefined): Array<ClientItemType> | undefined {
		if (!items) return undefined;
		if (!this.#mapper) throw new Error('Mapper is not implemented');
		return items.map((item) => this.#mapper(item));
	}
}
