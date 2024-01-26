import type { UmbTreeItemModelBase } from '../types.js';
import type { UmbTreeDataSource } from './tree-data-source.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { CancelablePromise, TreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbPagedModel } from '@umbraco-cms/backoffice/repository';

export interface UmbTreeServerDataSourceBaseArgs<
	ServerTreeItemType extends TreeItemPresentationModel,
	ClientTreeItemType extends UmbTreeItemModelBase,
> {
	getRootItems: () => CancelablePromise<UmbPagedModel<ServerTreeItemType>>;
	getChildrenOf: (parentUnique: string | null) => CancelablePromise<UmbPagedModel<ServerTreeItemType>>;
	mapper: (item: ServerTreeItemType) => ClientTreeItemType;
}

/**
 * A data source for a tree that fetches data from the server
 * @export
 * @class UmbTreeServerDataSourceBase
 * @implements {UmbTreeDataSource}
 */
export abstract class UmbTreeServerDataSourceBase<
	ServerTreeItemType extends TreeItemPresentationModel,
	ClientTreeItemType extends UmbTreeItemModelBase,
> implements UmbTreeDataSource<ClientTreeItemType>
{
	#host: UmbControllerHost;
	#getRootItems: () => CancelablePromise<UmbPagedModel<ServerTreeItemType>>;
	#getChildrenOf: (parentUnique: string | null) => CancelablePromise<UmbPagedModel<ServerTreeItemType>>;
	#mapper: (item: ServerTreeItemType) => ClientTreeItemType;

	/**
	 * Creates an instance of UmbTreeServerDataSourceBase.
	 * @param {UmbControllerHost} host
	 * @memberof UmbTreeServerDataSourceBase
	 */
	constructor(host: UmbControllerHost, args: UmbTreeServerDataSourceBaseArgs<ServerTreeItemType, ClientTreeItemType>) {
		this.#host = host;
		this.#getRootItems = args.getRootItems;
		this.#getChildrenOf = args.getChildrenOf;
		this.#mapper = args.mapper;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbTreeServerDataSourceBase
	 */
	async getRootItems() {
		const { data, error } = await tryExecuteAndNotify(this.#host, this.#getRootItems());

		if (data) {
			const items = data?.items.map((item) => this.#mapper(item));
			return { data: { total: data.total, items } };
		}

		return { error };
	}

	/**
	 * Fetches the children of a given parent unique from the server
	 * @param {(string)} parentUnique
	 * @return {*}
	 * @memberof UmbTreeServerDataSourceBase
	 */
	async getChildrenOf(parentUnique: string | null) {
		if (parentUnique === undefined) throw new Error('Parent unique is missing');

		const { data, error } = await tryExecuteAndNotify(this.#host, this.#getChildrenOf(parentUnique));

		if (data) {
			const items = data?.items.map((item: ServerTreeItemType) => this.#mapper(item));
			return { data: { total: data.total, items } };
		}

		return { error };
	}
}
