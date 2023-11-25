import { UmbDataTypeRepositoryBase } from '../data-type-repository-base.js';
import { createFolderTreeItem } from '../utils.js';
import { UmbDataTypeFolderServerDataSource } from './data-type-folder.server.data.js';
import type { UmbFolderRepository, UmbFolderDataSource } from '@umbraco-cms/backoffice/repository';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { CreateFolderRequestModel, FolderModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
export class UmbDataTypeFolderRepository extends UmbDataTypeRepositoryBase implements UmbFolderRepository {
	#folderSource: UmbFolderDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#folderSource = new UmbDataTypeFolderServerDataSource(this);
	}

	async createFolderScaffold(parentId: string | null) {
		if (parentId === undefined) throw new Error('Parent id is missing');
		await this._init;
		return this.#folderSource.createScaffold(parentId);
	}

	// TODO: temp create type until backend is ready. Remove the id addition when new types are generated.
	async createFolder(folderRequest: CreateFolderRequestModel & { id?: string | undefined }) {
		if (!folderRequest) throw new Error('folder request is missing');
		await this._init;

		const { error } = await this.#folderSource.create(folderRequest);

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
		return await this.#folderSource.read(id);
	}
}
