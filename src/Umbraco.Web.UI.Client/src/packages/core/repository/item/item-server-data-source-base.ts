import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';
import { CancelablePromise, ItemResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export interface UmbItemServerDataSourceBaseArgs<
	ServerItemType extends ItemResponseModelBaseModel,
	ClientItemType extends { unique: string },
> {
	getItems: (uniques: Array<string>) => CancelablePromise<Array<ServerItemType>>;
	mapper: (item: ServerItemType) => ClientItemType;
}

/**
 * A data source base for items that fetches items from the server
 * @export
 * @class UmbItemServerDataSourceBase
 * @implements {DocumentTreeDataSource}
 */
export abstract class UmbItemServerDataSourceBase<
	ServerItemType extends ItemResponseModelBaseModel,
	ClientItemType extends { unique: string },
> implements UmbItemDataSource<ClientItemType>
{
	#host: UmbControllerHost;
	#getItems: (uniques: Array<string>) => CancelablePromise<Array<ServerItemType>>;
	#mapper: (item: ServerItemType) => ClientItemType;

	/**
	 * Creates an instance of UmbItemServerDataSourceBase.
	 * @param {UmbControllerHost} host
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
	 * @return {*}
	 * @memberof UmbItemServerDataSourceBase
	 */
	async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');
		const { data, error } = await tryExecuteAndNotify(this.#host, this.#getItems(uniques));

		if (data) {
			const items = data.map((item) => this.#mapper(item));
			return { data: items };
		}

		return { error };
	}
}
