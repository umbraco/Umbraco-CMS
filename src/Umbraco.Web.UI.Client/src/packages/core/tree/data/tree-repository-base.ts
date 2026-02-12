import type { UmbTreeItemModel, UmbTreeRootModel } from '../types.js';
import type { UmbTreeRepository } from './tree-repository.interface.js';
import type { UmbTreeDataSource, UmbTreeDataSourceConstructor } from './tree-data-source.interface.js';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from './types.js';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * Base class for a tree repository.
 * @abstract
 * @class UmbTreeRepositoryBase
 * @augments {UmbRepositoryBase}
 * @implements {UmbTreeRepository}
 * @implements {UmbApi}
 * @template TreeItemType
 * @template TreeRootType
 */
export abstract class UmbTreeRepositoryBase<
		TreeItemType extends UmbTreeItemModel,
		TreeRootType extends UmbTreeRootModel,
		TreeRootItemsRequestArgsType extends UmbTreeRootItemsRequestArgs = UmbTreeRootItemsRequestArgs,
		TreeChildrenOfRequestArgsType extends UmbTreeChildrenOfRequestArgs = UmbTreeChildrenOfRequestArgs,
		TreeAncestorsOfRequestArgsType extends UmbTreeAncestorsOfRequestArgs = UmbTreeAncestorsOfRequestArgs,
	>
	extends UmbRepositoryBase
	implements
		UmbTreeRepository<
			TreeItemType,
			TreeRootType,
			TreeRootItemsRequestArgsType,
			TreeChildrenOfRequestArgsType,
			TreeAncestorsOfRequestArgsType
		>,
		UmbApi
{
	protected _treeSource: UmbTreeDataSource<TreeItemType>;

	/**
	 * Creates an instance of UmbTreeRepositoryBase.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @param {UmbTreeDataSourceConstructor<TreeItemType>} treeSourceConstructor - The constructor for the tree data source
	 * @memberof UmbTreeRepositoryBase
	 */
	constructor(host: UmbControllerHost, treeSourceConstructor: UmbTreeDataSourceConstructor<TreeItemType>) {
		super(host);
		this._treeSource = new treeSourceConstructor(this);
	}

	/**
	 * Request the tree root item
	 * @returns {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	abstract requestTreeRoot(): Promise<UmbRepositoryResponse<TreeRootType>>;

	/**
	 * Requests root items of a tree
	 * @param args
	 * @returns {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	async requestTreeRootItems(args: TreeRootItemsRequestArgsType) {
		const { data, error } = await this._treeSource.getRootItems(args);
		return { data, error, asObservable: () => undefined };
	}

	/**
	 * Requests tree items of a given parent
	 * @param {(string | null)} parentUnique
	 * @param args
	 * @returns {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	async requestTreeItemsOf(args: TreeChildrenOfRequestArgsType) {
		if (!args.parent) throw new Error('Parent is missing');
		if (args.parent.unique === undefined) throw new Error('Parent unique is missing');
		if (args.parent.entityType === null) throw new Error('Parent entity type is missing');

		const { data, error } = await this._treeSource.getChildrenOf(args);
		return { data, error, asObservable: () => undefined };
	}

	/**
	 * Requests ancestors of a given item
	 * @param {UmbTreeAncestorsOfRequestArgs} args
	 * @returns {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	async requestTreeItemAncestors(args: TreeAncestorsOfRequestArgsType) {
		if (args.treeItem.unique === undefined) throw new Error('Descendant unique is missing');

		const { data, error } = await this._treeSource.getAncestorsOf(args);

		// TODO: implement observable for ancestor items in the store
		// TODO: Fix the type of error, it should be UmbApiError, but currently it is any.
		return { data, error: error as any };
	}
}
