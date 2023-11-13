import { DATA_TYPE_ROOT_ENTITY_TYPE } from '../entities.js';
import { UmbDataTypeRepositoryBase } from '../repository/data-type-repository-base.js';
import { UmbDataTypeTreeServerDataSource } from './data-type.tree.server.data.js';
import type { UmbTreeRepository, UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { FolderTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';
export class UmbDataTypeTreeRepository
	extends UmbDataTypeRepositoryBase
	implements UmbTreeRepository<FolderTreeItemResponseModel>, UmbApi
{
	#treeSource: UmbTreeDataSource<FolderTreeItemResponseModel>;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#treeSource = new UmbDataTypeTreeServerDataSource(this);
	}

	async requestTreeRoot() {
		await this._init;

		const data = {
			id: null,
			type: DATA_TYPE_ROOT_ENTITY_TYPE,
			name: 'Data Types',
			icon: 'icon-folder',
			hasChildren: true,
		};

		return { data };
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
		await this._init;
		if (parentId === undefined) throw new Error('Parent id is missing');

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
