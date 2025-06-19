import type { UmbTreeItemModelBase, UmbTreeItemModel } from '../types.js';
import type { UmbTreeDataSource } from './tree-data-source.interface.js';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from './types.js';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

export interface UmbTreeServerDataSourceBaseArgs<
	ServerTreeItemType extends { hasChildren: boolean },
	ClientTreeItemType extends UmbTreeItemModelBase,
	TreeRootItemsRequestArgsType extends UmbTreeRootItemsRequestArgs = UmbTreeRootItemsRequestArgs,
	TreeChildrenOfRequestArgsType extends UmbTreeChildrenOfRequestArgs = UmbTreeChildrenOfRequestArgs,
	TreeAncestorsOfRequestArgsType extends UmbTreeAncestorsOfRequestArgs = UmbTreeAncestorsOfRequestArgs,
> {
	getRootItems: (
		args: TreeRootItemsRequestArgsType,
	) => Promise<UmbDataSourceResponse<UmbPagedModel<ServerTreeItemType>>>;
	getChildrenOf: (
		args: TreeChildrenOfRequestArgsType,
	) => Promise<UmbDataSourceResponse<UmbPagedModel<ServerTreeItemType>>>;
	getAncestorsOf: (args: TreeAncestorsOfRequestArgsType) => Promise<UmbDataSourceResponse<Array<ServerTreeItemType>>>;
	mapper: (item: ServerTreeItemType) => ClientTreeItemType;
}

/**
 * A data source for a tree that fetches data from the server
 * @class UmbTreeServerDataSourceBase
 * @implements {UmbTreeDataSource}
 */
export abstract class UmbTreeServerDataSourceBase<
	ServerTreeItemType extends { hasChildren: boolean },
	ClientTreeItemType extends UmbTreeItemModel,
	TreeRootItemsRequestArgsType extends UmbTreeRootItemsRequestArgs = UmbTreeRootItemsRequestArgs,
	TreeChildrenOfRequestArgsType extends UmbTreeChildrenOfRequestArgs = UmbTreeChildrenOfRequestArgs,
	TreeAncestorsOfRequestArgsType extends UmbTreeAncestorsOfRequestArgs = UmbTreeAncestorsOfRequestArgs,
> implements
		UmbTreeDataSource<
			ClientTreeItemType,
			TreeRootItemsRequestArgsType,
			TreeChildrenOfRequestArgsType,
			TreeAncestorsOfRequestArgsType
		>
{
	#host;
	#getRootItems;
	#getChildrenOf;
	#getAncestorsOf;
	#mapper;

	/**
	 * Creates an instance of UmbTreeServerDataSourceBase.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @param args
	 * @memberof UmbTreeServerDataSourceBase
	 */
	constructor(
		host: UmbControllerHost,
		args: UmbTreeServerDataSourceBaseArgs<
			ServerTreeItemType,
			ClientTreeItemType,
			TreeRootItemsRequestArgsType,
			TreeChildrenOfRequestArgsType,
			TreeAncestorsOfRequestArgsType
		>,
	) {
		this.#host = host;
		this.#getRootItems = args.getRootItems;
		this.#getChildrenOf = args.getChildrenOf;
		this.#getAncestorsOf = args.getAncestorsOf;
		this.#mapper = args.mapper;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @param {UmbTreeRootItemsRequestArgs} args
	 * @returns {*}
	 * @memberof UmbTreeServerDataSourceBase
	 */
	async getRootItems(args: TreeRootItemsRequestArgsType) {
		const { data, error } = await tryExecute(this.#host, this.#getRootItems(args));

		if (data) {
			const items = data?.items.map((item) => this.#mapper(item));
			return { data: { total: data.total, items } };
		}

		return { error };
	}

	/**
	 * Fetches the children of a given parent unique from the server
	 * @param {UmbTreeChildrenOfRequestArgs} args
	 * @returns {*}
	 * @memberof UmbTreeServerDataSourceBase
	 */
	async getChildrenOf(args: TreeChildrenOfRequestArgsType) {
		if (args.parent.unique === undefined) throw new Error('Parent unique is missing');

		const { data, error } = await tryExecute(this.#host, this.#getChildrenOf(args));

		if (data) {
			const items = data?.items.map((item: ServerTreeItemType) => this.#mapper(item));
			return { data: { total: data.total, items } };
		}

		return { error };
	}

	/**
	 * Fetches the ancestors of a given item from the server
	 * @param {UmbTreeAncestorsOfRequestArgs} args
	 * @returns {*}
	 * @memberof UmbTreeServerDataSourceBase
	 */
	async getAncestorsOf(args: TreeAncestorsOfRequestArgsType) {
		if (!args.treeItem.entityType) throw new Error('Parent unique is missing');

		const { data, error } = await tryExecute(this.#host, this.#getAncestorsOf(args));

		if (data) {
			const items = data?.map((item: ServerTreeItemType) => this.#mapper(item));
			return { data: items };
		}

		return { error };
	}
}
