import type { UmbTreeItemModel, UmbTreeRootModel } from '../types.js';
import type { UmbTreeStore } from './tree-store.interface.js';
import type { UmbTreeRepository } from './tree-repository.interface.js';
import type { UmbTreeDataSource, UmbTreeDataSourceConstructor } from './tree-data-source.interface.js';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from './types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbProblemDetails } from '@umbraco-cms/backoffice/resources';

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
	protected _init: Promise<unknown>;
	protected _treeStore?: UmbTreeStore<TreeItemType>;
	protected _treeSource: UmbTreeDataSource<TreeItemType>;

	/**
	 * Creates an instance of UmbTreeRepositoryBase.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @param {UmbTreeDataSourceConstructor<TreeItemType>} treeSourceConstructor
	 * @param {(string | UmbContextToken<any, any>)} treeStoreContextAlias
	 * @memberof UmbTreeRepositoryBase
	 */
	constructor(
		host: UmbControllerHost,
		treeSourceConstructor: UmbTreeDataSourceConstructor<TreeItemType>,
		treeStoreContextAlias: string | UmbContextToken<any, any>,
	) {
		super(host);
		this._treeSource = new treeSourceConstructor(this);

		this._init = this.consumeContext(treeStoreContextAlias, (instance) => {
			this._treeStore = instance;
		}).asPromise({ preventTimeout: true });
	}

	/**
	 * Request the tree root item
	 * @returns {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	abstract requestTreeRoot(): Promise<{ data?: TreeRootType; error?: UmbProblemDetails }>;

	/**
	 * Requests root items of a tree
	 * @param args
	 * @returns {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	async requestTreeRootItems(args: TreeRootItemsRequestArgsType) {
		await this._init;

		const { data, error } = await this._treeSource.getRootItems(args);
		if (!this._treeStore) {
			// If the tree store is not available, then we most likely are in a destructed setting.
			return {};
		}
		if (data) {
			this._treeStore?.appendItems(data.items);
		}

		// TODO: Notice we are casting the error here, is that right?
		return { data, error: error as unknown as UmbProblemDetails, asObservable: () => this._treeStore!.rootItems };
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
		await this._init;

		const { data, error: _error } = await this._treeSource.getChildrenOf(args);
		const error: any = _error;
		if (data) {
			this._treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this._treeStore!.childrenOf(args.parent.unique) };
	}

	/**
	 * Requests ancestors of a given item
	 * @param {UmbTreeAncestorsOfRequestArgs} args
	 * @returns {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	async requestTreeItemAncestors(args: TreeAncestorsOfRequestArgsType) {
		if (args.treeItem.unique === undefined) throw new Error('Descendant unique is missing');
		await this._init;

		const { data, error: _error } = await this._treeSource.getAncestorsOf(args);
		const error: any = _error;
		// TODO: implement observable for ancestor items in the store
		return { data, error };
	}

	/**
	 * Returns a promise with an observable of tree root items
	 * @returns {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	async rootTreeItems() {
		await this._init;
		return this._treeStore!.rootItems;
	}

	/**
	 * Returns a promise with an observable of children items of a given parent
	 * @param {(string | null)} parentUnique
	 * @returns {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	async treeItemsOf(parentUnique: string | null) {
		if (parentUnique === undefined) throw new Error('Parent unique is missing');
		await this._init;
		return this._treeStore!.childrenOf(parentUnique);
	}
}
