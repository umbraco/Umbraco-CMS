import { DATA_TYPE_ROOT_ENTITY_TYPE } from '../entities.js';
import { UmbDataTypeTreeServerDataSource } from './sources/data-type.tree.server.data.js';
import { UmbDataTypeMoveServerDataSource } from './sources/data-type-move.server.data.js';
import { UmbDataTypeServerDataSource } from './sources/data-type.server.data.js';
import { UmbDataTypeFolderServerDataSource } from './sources/data-type-folder.server.data.js';
import { UmbDataTypeCopyServerDataSource } from './sources/data-type-copy.server.data.js';
import { UmbDataTypeRepositoryBase } from './data-type-repository-base.js';
import type {
	UmbTreeRepository,
	UmbDetailRepository,
	UmbFolderRepository,
	UmbMoveRepository,
	UmbCopyRepository,
	UmbTreeDataSource,
	UmbDataSource,
	UmbFolderDataSource,
	UmbMoveDataSource,
	UmbCopyDataSource,
} from '@umbraco-cms/backoffice/repository';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	CreateDataTypeRequestModel,
	CreateFolderRequestModel,
	DataTypeResponseModel,
	FolderModelBaseModel,
	FolderTreeItemResponseModel,
	UpdateDataTypeRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';
export class UmbDataTypeRepository
	extends UmbDataTypeRepositoryBase
	implements
		UmbTreeRepository<FolderTreeItemResponseModel>,
		UmbDetailRepository<CreateDataTypeRequestModel, any, UpdateDataTypeRequestModel, DataTypeResponseModel>,
		UmbFolderRepository,
		UmbMoveRepository,
		UmbCopyRepository,
		UmbApi
{
	#treeSource: UmbTreeDataSource<FolderTreeItemResponseModel>;
	#detailSource: UmbDataSource<CreateDataTypeRequestModel, any, UpdateDataTypeRequestModel, DataTypeResponseModel>;
	#folderSource: UmbFolderDataSource;
	#moveSource: UmbMoveDataSource;
	#copySource: UmbCopyDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		// TODO: figure out how spin up get the correct data source
		this.#treeSource = new UmbDataTypeTreeServerDataSource(this);
		this.#detailSource = new UmbDataTypeServerDataSource(this);
		this.#folderSource = new UmbDataTypeFolderServerDataSource(this);
		this.#moveSource = new UmbDataTypeMoveServerDataSource(this);
		this.#copySource = new UmbDataTypeCopyServerDataSource(this);
	}

	// TREE:
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

	// DETAILS:
	async createScaffold(parentId: string | null) {
		if (parentId === undefined) throw new Error('Parent id is missing');
		await this._init;

		return this.#detailSource.createScaffold(parentId);
	}

	async requestById(id: string) {
		if (!id) throw new Error('Key is missing');
		await this._init;

		const { data, error } = await this.#detailSource.get(id);

		if (data) {
			this._detailStore!.append(data);
		}

		return { data, error, asObservable: () => this._detailStore!.byId(id) };
	}

	async byId(id: string) {
		if (!id) throw new Error('Key is missing');
		await this._init;
		return this._detailStore!.byId(id);
	}

	async byPropertyEditorUiAlias(propertyEditorUiAlias: string) {
		if (!propertyEditorUiAlias) throw new Error('propertyEditorUiAlias is missing');
		await this._init;
		return this._detailStore!.withPropertyEditorUiAlias(propertyEditorUiAlias);
	}

	async create(dataType: CreateDataTypeRequestModel) {
		if (!dataType) throw new Error('Data Type is missing');
		if (!dataType.id) throw new Error('Data Type id is missing');
		await this._init;

		const { error } = await this.#detailSource.insert(dataType);

		if (!error) {
			// TODO: We need to push a new item to the tree store to update the tree. How do we want to create the tree items?
			const treeItem = createTreeItem(dataType);
			this._treeStore!.appendItems([treeItem]);
			//this.#detailStore?.append(dataType);

			const notification = { data: { message: `Data Type created` } };
			this._notificationContext!.peek('positive', notification);
		}

		return { error };
	}

	async save(id: string, updatedDataType: UpdateDataTypeRequestModel) {
		if (!id) throw new Error('Data Type id is missing');
		if (!updatedDataType) throw new Error('Data Type is missing');
		await this._init;

		const { error } = await this.#detailSource.update(id, updatedDataType);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server
			// Consider notify a workspace if a template is updated in the store while someone is editing it.
			// TODO: would be nice to align the stores on methods/methodNames.
			this._detailStore!.updateItem(id, updatedDataType);
			// TODO: This is parsing on the full models to the tree and item store. Those should only contain the data they need. I don't know, at this point, if thats a repository or store responsibility.
			this._treeStore!.updateItem(id, updatedDataType);
			this._itemStore!.updateItem(id, updatedDataType);

			const notification = { data: { message: `Data Type saved` } };
			this._notificationContext!.peek('positive', notification);
		}

		return { error };
	}

	async delete(id: string) {
		if (!id) throw new Error('Data Type id is missing');
		await this._init;

		const { error } = await this.#detailSource.delete(id);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server.
			// Consider notify a workspace if a template is deleted from the store while someone is editing it.
			// TODO: would be nice to align the stores on methods/methodNames.
			this._detailStore!.remove([id]);
			this._treeStore!.removeItem(id);
			this._itemStore!.removeItem(id);

			const notification = { data: { message: `Data Type deleted` } };
			this._notificationContext!.peek('positive', notification);
		}

		return { error };
	}

	// Folder:
	async createFolderScaffold(parentId: string | null) {
		if (parentId === undefined) throw new Error('Parent id is missing');
		await this._init;
		return this.#folderSource.createScaffold(parentId);
	}

	// TODO: temp create type until backend is ready. Remove the id addition when new types are generated.
	async createFolder(folderRequest: CreateFolderRequestModel & { id?: string | undefined }) {
		if (!folderRequest) throw new Error('folder request is missing');
		await this._init;

		const { error } = await this.#folderSource.insert(folderRequest);

		if (!error) {
			// TODO: We need to push a new item to the tree store to update the tree. How do we want to create the tree items?
			const folderTreeItem = createFolderTreeItem(folderRequest);
			this._treeStore!.appendItems([folderTreeItem]);
		}

		return { error };
	}

	async deleteFolder(id: string) {
		if (!id) throw new Error('Key is missing');
		await this._init;

		const { error } = await this.#folderSource.delete(id);

		if (!error) {
			this._treeStore!.removeItem(id);
		}

		return { error };
	}

	async updateFolder(id: string, folder: FolderModelBaseModel) {
		if (!id) throw new Error('Key is missing');
		if (!folder) throw new Error('Folder data is missing');
		await this._init;

		const { error } = await this.#folderSource.update(id, folder);

		if (!error) {
			this._treeStore!.updateItem(id, { name: folder.name });
		}

		return { error };
	}

	async requestFolder(id: string) {
		if (!id) throw new Error('Key is missing');
		await this._init;
		return await this.#folderSource.get(id);
	}

	// Actions
	async move(id: string, targetId: string | null) {
		await this._init;
		const { error } = await this.#moveSource.move(id, targetId);

		if (!error) {
			// TODO: Be aware about this responsibility.
			this._treeStore!.updateItem(id, { parentId: targetId });
			// only update the target if its not the root
			if (targetId) {
				this._treeStore!.updateItem(targetId, { hasChildren: true });
			}

			const notification = { data: { message: `Data type moved` } };
			this._notificationContext!.peek('positive', notification);
		}

		return { error };
	}

	async copy(id: string, targetId: string | null) {
		await this._init;
		const { data: dataTypeCopyId, error } = await this.#copySource.copy(id, targetId);
		if (error) return { error };

		if (dataTypeCopyId) {
			const { data: dataTypeCopy } = await this.requestById(dataTypeCopyId);
			if (!dataTypeCopy) throw new Error('Could not find copied data type');

			// TODO: Be aware about this responsibility.
			this._treeStore!.appendItems([dataTypeCopy]);
			// only update the target if its not the root
			if (targetId) {
				this._treeStore!.updateItem(targetId, { hasChildren: true });
			}

			const notification = { data: { message: `Data type copied` } };
			this._notificationContext!.peek('positive', notification);
		}

		return { data: dataTypeCopyId };
	}
}

export const createTreeItem = (item: CreateDataTypeRequestModel): FolderTreeItemResponseModel => {
	if (!item) throw new Error('item is null or undefined');
	if (!item.id) throw new Error('item.id is null or undefined');

	return {
		type: 'data-type',
		parentId: item.parentId,
		name: item.name,
		id: item.id,
		isFolder: false,
		isContainer: false,
		hasChildren: false,
	};
};

export const createFolderTreeItem = (item: CreateFolderRequestModel): FolderTreeItemResponseModel => {
	if (!item) throw new Error('item is null or undefined');
	if (!item.id) throw new Error('item.id is null or undefined');

	return {
		...createTreeItem(item),
		isFolder: true,
	};
};
