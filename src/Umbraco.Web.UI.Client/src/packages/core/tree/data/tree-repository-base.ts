import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '../types.js';
import type { UmbTreeStore } from './tree-store.interface.js';
import type { UmbTreeRepository } from './tree-repository.interface.js';
import type { UmbTreeDataSource, UmbTreeDataSourceConstructor } from './tree-data-source.interface.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * Base class for a tree repository.
 * @export
 * @abstract
 * @class UmbTreeRepositoryBase
 * @extends {UmbRepositoryBase}
 * @implements {UmbTreeRepository<TreeItemType, TreeRootType>}
 * @implements {UmbApi}
 * @template TreeItemType
 * @template TreeRootType
 */
export abstract class UmbTreeRepositoryBase<
		TreeItemType extends UmbUniqueTreeItemModel,
		TreeRootType extends UmbUniqueTreeRootModel,
	>
	extends UmbRepositoryBase
	implements UmbTreeRepository<TreeItemType, TreeRootType>, UmbApi
{
	protected _init: Promise<unknown>;
	protected _treeStore?: UmbTreeStore<TreeItemType>;
	#treeSource: UmbTreeDataSource<TreeItemType>;

	/**
	 * Creates an instance of UmbTreeRepositoryBase.
	 * @param {UmbControllerHost} host
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
		this.#treeSource = new treeSourceConstructor(this);

		this._init = this.consumeContext(treeStoreContextAlias, (instance) => {
			this._treeStore = instance;
		}).asPromise();
	}

	/**
	 * Request the tree root item
	 * @return {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	abstract requestTreeRoot(): Promise<{ data?: TreeRootType; error?: Error }>;

	/**
	 * Requests root items of a tree
	 * @return {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	async requestRootTreeItems(args: any) {
		await this._init;

		const { data, error } = await this.#treeSource.getRootItems(args);

		if (data) {
			this._treeStore!.appendItems(data.items);
		}

		return { data, error, asObservable: () => this._treeStore!.rootItems };
	}

	/**
	 * Requests tree items of a given parent
	 * @param {(string | null)} parentUnique
	 * @return {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	async requestTreeItemsOf(args: any) {
		if (args.parentUnique === undefined) throw new Error('Parent unique is missing');
		await this._init;

		const { data, error } = await this.#treeSource.getChildrenOf(args);

		if (data) {
			this._treeStore!.appendItems(data.items);
		}

		return { data, error, asObservable: () => this._treeStore!.childrenOf(args.parentUnique) };
	}

	/**
	 * Returns a promise with an observable of tree root items
	 * @return {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	async rootTreeItems() {
		await this._init;
		return this._treeStore!.rootItems;
	}

	/**
	 * Returns a promise with an observable of children items of a given parent
	 * @param {(string | null)} parentUnique
	 * @return {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	async treeItemsOf(parentUnique: string | null) {
		if (parentUnique === undefined) throw new Error('Parent unique is missing');
		await this._init;
		return this._treeStore!.childrenOf(parentUnique);
	}
}
