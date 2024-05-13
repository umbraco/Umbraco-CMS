import type { UmbTreeItemModelBase, UmbTreeItemModel } from '../types.js';
import type { UmbTreeDataSource } from './tree-data-source.interface.js';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from './types.js';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbPagedModel } from '@umbraco-cms/backoffice/repository';

export interface UmbTreeServerDataSourceBaseArgs<
	ServerTreeItemType extends { hasChildren: boolean },
	ClientTreeItemType extends UmbTreeItemModelBase,
> {
	getRootItems: (args: UmbTreeRootItemsRequestArgs) => Promise<UmbPagedModel<ServerTreeItemType>>;
	getChildrenOf: (args: UmbTreeChildrenOfRequestArgs) => Promise<UmbPagedModel<ServerTreeItemType>>;
	getAncestorsOf: (args: UmbTreeAncestorsOfRequestArgs) => Promise<Array<ServerTreeItemType>>;
	mapper: (item: ServerTreeItemType) => ClientTreeItemType;
}

/**
 * A data source for a tree that fetches data from the server
 * @export
 * @class UmbTreeServerDataSourceBase
 * @implements {UmbTreeDataSource}
 */
export abstract class UmbTreeServerDataSourceBase<
	ServerTreeItemType extends { hasChildren: boolean },
	ClientTreeItemType extends UmbTreeItemModel,
> implements UmbTreeDataSource<ClientTreeItemType>
{
	#host;
	#getRootItems;
	#getChildrenOf;
	#getAncestorsOf;
	#mapper;

	/**
	 * Creates an instance of UmbTreeServerDataSourceBase.
	 * @param {UmbControllerHost} host
	 * @memberof UmbTreeServerDataSourceBase
	 */
	constructor(host: UmbControllerHost, args: UmbTreeServerDataSourceBaseArgs<ServerTreeItemType, ClientTreeItemType>) {
		this.#host = host;
		this.#getRootItems = args.getRootItems;
		this.#getChildrenOf = args.getChildrenOf;
		this.#getAncestorsOf = args.getAncestorsOf;
		this.#mapper = args.mapper;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @param {UmbTreeRootItemsRequestArgs} args
	 * @return {*}
	 * @memberof UmbTreeServerDataSourceBase
	 */
	async getRootItems(args: UmbTreeRootItemsRequestArgs) {
		const { data, error } = await tryExecuteAndNotify(this.#host, this.#getRootItems(args));

		if (data) {
			const items = data?.items.map((item) => this.#mapper(item));
			return { data: { total: data.total, items } };
		}

		return { error };
	}

	/**
	 * Fetches the children of a given parent unique from the server
	 * @param {UmbTreeChildrenOfRequestArgs} args
	 * @return {*}
	 * @memberof UmbTreeServerDataSourceBase
	 */
	async getChildrenOf(args: UmbTreeChildrenOfRequestArgs) {
		if (args.parent.unique === undefined) throw new Error('Parent unique is missing');

		const { data, error } = await tryExecuteAndNotify(this.#host, this.#getChildrenOf(args));

		if (data) {
			const items = data?.items.map((item: ServerTreeItemType) => this.#mapper(item));
			return { data: { total: data.total, items } };
		}

		return { error };
	}

	/**
	 * Fetches the ancestors of a given item from the server
	 * @param {UmbTreeAncestorsOfRequestArgs} args
	 * @return {*}
	 * @memberof UmbTreeServerDataSourceBase
	 */
	async getAncestorsOf(args: UmbTreeAncestorsOfRequestArgs) {
		if (!args.treeItem.entityType) throw new Error('Parent unique is missing');

		const { data, error } = await tryExecuteAndNotify(this.#host, this.#getAncestorsOf(args));

		if (data) {
			const items = data?.map((item: ServerTreeItemType) => this.#mapper(item));
			return { data: items };
		}

		return { error };
	}
}
