import type { UmbDataSourceResponse } from '../data-source-response.interface.js';
import type { UmbItemDataSource } from './item-data-source.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export interface UmbItemServerDataSourceBaseArgs<ServerItemType, ClientItemType extends { unique: string }> {
	getItems: (uniques: Array<string>) => Promise<UmbDataSourceResponse<Array<ServerItemType>>>;
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
	#getItems: (uniques: Array<string>) => Promise<UmbDataSourceResponse<Array<ServerItemType>>>;
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

		const batchSize = 40;
		if (uniques.length > batchSize) {
			const chunks = batchArray<string>(uniques, batchSize);
			const results = await Promise.allSettled(chunks.map((chunk) => tryExecute(this.#host, this.#getItems(chunk))));
			const data = results.find((result) => result.status === 'fulfilled')?.value.data;
			const error = results.find((result) => result.status === 'rejected')?.reason;
			if (data) {
				const items = data.map((item) => this.#mapper(item));
				return { data: items };
			}
			return { error };
		}

		const { data, error } = await tryExecute(this.#host, this.#getItems(uniques));

		if (data) {
			const items = data.map((item) => this.#mapper(item));
			return { data: items };
		}

		return { error };
	}
}

/**
 * Splits an array into chunks of a specified size
 * @param { Array<T> } array - The array to split
 * @param {number }batchSize - The size of each chunk
 * @returns {Array<Array<T>>} - An array of chunks
 */
function batchArray<T>(array: Array<T>, batchSize: number): Array<Array<T>> {
	const chunks: Array<Array<T>> = [];
	for (let i = 0; i < array.length; i += batchSize) {
		chunks.push(array.slice(i, i + batchSize));
	}
	return chunks;
}
