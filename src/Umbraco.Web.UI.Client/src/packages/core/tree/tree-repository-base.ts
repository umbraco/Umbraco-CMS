import { UmbTreeStore } from './tree-store.interface.js';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from './types.js';
import { UmbTreeRepository } from './tree-repository.interface.js';
import type { UmbTreeDataSource, UmbTreeDataSourceConstructor } from './tree-data-source.interface.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbTreeRepositoryBase<
		TreeItemType extends UmbEntityTreeItemModel,
		TreeRootType extends UmbEntityTreeRootModel,
	>
	extends UmbRepositoryBase
	implements UmbTreeRepository<TreeItemType, TreeRootType>, UmbApi
{
	protected _init: Promise<unknown>;
	protected _treeStore?: UmbTreeStore<TreeItemType>;
	#treeSource: UmbTreeDataSource<TreeItemType, TreeRootType>;

	constructor(
		host: UmbControllerHost,
		treeSource: UmbTreeDataSourceConstructor<TreeItemType, TreeRootType>,
		treeStoreContextAlias: string | UmbContextToken<any, any>,
	) {
		super(host);
		this.#treeSource = new treeSource(this);

		this._init = this.consumeContext(treeStoreContextAlias, (instance) => {
			this._treeStore = instance;
		}).asPromise();
	}

	/**
	 * Request the tree root item
	 * @return {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	async requestTreeRoot() {
		if (!this.#treeSource.getTreeRoot?.()) {
			return { data: undefined, error: undefined };
		}

		return this.#treeSource.getTreeRoot();
	}

	/**
	 * Requests root items of a tree
	 * @return {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	async requestRootTreeItems() {
		await this._init;

		const { data, error } = await this.#treeSource.getRootItems();

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
	async requestTreeItemsOf(parentUnique: string | null) {
		if (parentUnique === undefined) throw new Error('Parent unique is missing');
		await this._init;

		const { data, error } = await this.#treeSource.getChildrenOf(parentUnique);

		if (data) {
			this._treeStore!.appendItems(data.items);
		}

		return { data, error, asObservable: () => this._treeStore!.childrenOf(parentUnique) };
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
