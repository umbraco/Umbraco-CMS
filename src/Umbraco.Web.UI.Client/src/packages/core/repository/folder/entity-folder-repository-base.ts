import { UmbRepositoryBase } from '../repository-base.js';
import { type UmbEntityFolderRepository } from './entity-folder-repository.interface.js';
import type {
	UmbEntityFolderDataSource,
	UmbEntityFolderDataSourceConstructor,
} from './entity-folder-data-source.interface.js';
import { UmbCreateFolderModel, UmbUpdateFolderModel } from './types.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbTreeStore } from '@umbraco-cms/backoffice/tree';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbId } from '@umbraco-cms/backoffice/id';

export type UmbFolderToTreeItemMapper<FolderTreeItemType> = (item: UmbCreateFolderModel) => FolderTreeItemType;

export class UmbFolderRepositoryBase extends UmbRepositoryBase implements UmbEntityFolderRepository {
	protected _init: Promise<unknown>;
	protected _treeStore?: UmbTreeStore;
	#folderDataSource: UmbEntityFolderDataSource;
	#folderToTreeItemMapper: UmbFolderToTreeItemMapper<any>;

	constructor(
		host: UmbControllerHost,
		folderDataSource: UmbEntityFolderDataSourceConstructor,
		treeStoreContextAlias: string | UmbContextToken<any>,
		folderToTreeItemMapper: UmbFolderToTreeItemMapper<any>,
	) {
		super(host);
		this.#folderDataSource = new folderDataSource(this);
		this.#folderToTreeItemMapper = folderToTreeItemMapper;

		this._init = this.consumeContext(treeStoreContextAlias, (instance) => {
			this._treeStore = instance as UmbTreeStore;
		}).asPromise();
	}

	/**
	 * Creates a Data Type folder with the given id from the server
	 * @param {string} parentId
	 * @return {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	/* TODO: revisit this method. 
	Id is currently not used everywhere, but the method is currently overwritten for file system repos. */
	async createFolderScaffold(parentUnique: string | null) {
		if (parentUnique === undefined) throw new Error('Parent unique is missing');

		const scaffold = {
			name: '',
			unique: UmbId.new(),
			parentUnique,
		};

		return { data: scaffold };
	}

	async createFolder(args: UmbCreateFolderModel) {
		if (args.parentUnique === undefined) throw new Error('Parent unique is missing');
		if (!args.name) throw new Error('Name is missing');
		await this._init;

		const { error } = await this.#folderDataSource.insert(args);

		if (!error) {
			const folderTreeItem = this.#folderToTreeItemMapper(args);
			this._treeStore!.appendItems([folderTreeItem]);
		}

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
	async updateFolder(args: UmbUpdateFolderModel) {
		if (!args.unique) throw new Error('Unique is missing');
		if (!args.name) throw new Error('Folder name is missing');
		await this._init;

		const { error } = await this.#folderDataSource.update(args);

		if (!error) {
			this._treeStore!.updateItem(args.unique, { name: args.name });
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
