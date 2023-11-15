import { UmbRepositoryBase } from '../repository-base.js';
import { type UmbFolderRepository } from './folder-repository.interface.js';
import type { UmbFolderDataSource, UmbFolderDataSourceConstructor } from './folder-data-source.interface.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbTreeStore } from '@umbraco-cms/backoffice/tree';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbFolderRepositoryBase extends UmbRepositoryBase implements UmbFolderRepository {
	protected _init: Promise<unknown>;
	protected _treeStore?: UmbTreeStore;
	#folderDataSource: UmbFolderDataSource;

	constructor(
		host: UmbControllerHost,
		folderDataSource: UmbFolderDataSourceConstructor,
		treeStoreContextAlias: string | UmbContextToken<any, any>,
	) {
		super(host);
		this.#folderDataSource = new folderDataSource(this);

		this._init = this.consumeContext(treeStoreContextAlias, (instance) => {
			this._treeStore = instance as UmbTreeStore;
		}).asPromise();
	}

	async createFolderScaffold(parentUnique: string | null) {
		if (parentUnique === undefined) throw new Error('Parent unique is missing');
		await this._init;
		return this.#folderDataSource.createScaffold(parentUnique);
	}

	async createFolder(unique: string, parentUnique: string | null, name: string) {
		if (!unique) throw new Error('Unique is missing');
		if (parentUnique === undefined) throw new Error('Parent unique is missing');
		if (!name) throw new Error('Name is missing');
		await this._init;

		const { error } = await this.#folderDataSource.insert(unique, parentUnique, name);

		/*
		if (!error) {
			// TODO: We need to push a new item to the tree store to update the tree. How do we want to create the tree items?
			const folderTreeItem = createFolderTreeItem(folderRequest);
			this._treeStore!.appendItems([folderTreeItem]);
		}
		*/

		return { error };
	}

	async deleteFolder(id: string) {
		if (!id) throw new Error('Key is missing');
		await this._init;

		const { error } = await this.#folderDataSource.delete(id);

		if (!error) {
			this._treeStore!.removeItem(id);
		}

		return { error };
	}

	/**
	 * Request a folder by a unique
	 * @param {string} id
	 * @param {FolderModelBaseModel} folder
	 * @return {*}
	 * @memberof UmbFolderRepositoryBase
	 */
	async updateFolder(unique: string, name: string) {
		if (!unique) throw new Error('Unique is missing');
		if (!name) throw new Error('Folder name is missing');
		await this._init;

		const { error } = await this.#folderDataSource.update(unique, name);

		if (!error) {
			this._treeStore!.updateItem(unique, { name });
		}

		return { error };
	}

	/**
	 * Request a folder by a unique
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbFolderRepositoryBase
	 */
	async requestFolder(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		await this._init;
		return await this.#folderDataSource.get(unique);
	}
}
