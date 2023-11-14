import { UmbTreeStore } from '../store/tree-store.interface.js';
import { type UmbEntityTreeItemModel } from './types.js';
import {
	type UmbTreeRepository,
	type UmbTreeDataSource,
	UmbRepositoryBase,
	UmbTreeDataSourceConstructor,
} from '@umbraco-cms/backoffice/repository';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbEntityTreeRepositoryBase<TreeItemType extends UmbEntityTreeItemModel>
	extends UmbRepositoryBase
	implements UmbTreeRepository<TreeItemType>, UmbApi
{
	protected _init: Promise<unknown>;
	protected _treeStore?: UmbTreeStore;
	#treeSource: UmbTreeDataSource<TreeItemType>;

	constructor(
		host: UmbControllerHost,
		treeSource: UmbTreeDataSourceConstructor<TreeItemType>,
		treeStoreContextAlias: string | UmbContextToken<any, any>,
	) {
		super(host);
		this.#treeSource = new treeSource(this);

		this._init = this.consumeContext(treeStoreContextAlias, (instance) => {
			this._treeStore = instance as UmbTreeStore;
		}).asPromise();
	}

	async requestTreeRoot() {
		await this._init;

		const { data, error } = await this.#treeSource.getTreeRoot();

		return { data, error };
	}

	async requestRootTreeItems() {
		await this._init;

		const { data, error } = await this.#treeSource.getRootItems();

		if (data) {
			this._treeStore!.appendItems(data.items);
		}

		return { data, error, asObservable: () => this._treeStore!.rootItems };
	}

	async requestTreeItemsOf(parentId: string | null) {
		if (parentId === undefined) throw new Error('Parent id is missing');
		await this._init;

		const { data, error } = await this.#treeSource.getChildrenOf(parentId);

		if (data) {
			this._treeStore!.appendItems(data.items);
		}

		return { data, error, asObservable: () => this._treeStore!.childrenOf(parentId) };
	}

	async rootTreeItems() {
		await this._init;
		return this._treeStore!.rootItems;
	}

	async treeItemsOf(parentId: string | null) {
		if (parentId === undefined) throw new Error('Parent id is missing');
		await this._init;
		return this._treeStore!.childrenOf(parentId);
	}
}
