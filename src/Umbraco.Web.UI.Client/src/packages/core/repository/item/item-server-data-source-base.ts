import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbItemDataSource } from './item-data-source.interface.js';

export interface UmbItemServerDataSourceBaseArgs<ServerItemType, ClientItemType extends { unique: string }> {
	getItems: (uniques: Array<string>) => Promise<Array<ServerItemType>>;
	mapper: (item: ServerItemType) => ClientItemType;
}

/**
 * A data source base for items that fetches items from the server
 * @class UmbItemServerDataSourceBase
 * @implements {DocumentTreeDataSource}
 */
export abstract class UmbItemServerDataSourceBase<ServerItemType, ClientItemType extends { unique: string }>
	implements UmbItemDataSource<ClientItemType>
{
	#host: UmbControllerHost;
	#getItems: (uniques: Array<string>) => Promise<Array<ServerItemType>>;
	#mapper: (item: ServerItemType) => ClientItemType;

	/**
	 * Creates an instance of UmbItemServerDataSourceBase.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @param args
	 * @memberof UmbItemServerDataSourceBase
	 */
	constructor(host: UmbControllerHost, args: UmbItemServerDataSourceBaseArgs<ServerItemType, ClientItemType>) {
		this.#host = host;
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
		if (!uniques) throw new Error('Uniques are missing');
		const { data, error } = await tryExecute(this.#host, this.#getItems(uniques));

		if (data) {
			const items = data.map((item) => this.#mapper(item));
			return { data: items };
		}

		return { error };
	}
}
