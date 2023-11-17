import { UmbRepositoryBase } from '../repository-base.js';
import { type IUmbFolderRepository } from './folder-repository.interface.js';
import type { UmbFolderDataSource, UmbFolderDataSourceConstructor } from './folder-data-source.interface.js';
import { UmbCreateFolderModel, UmbFolderModel, UmbUpdateFolderModel } from './types.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbTreeStore } from '@umbraco-cms/backoffice/tree';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export type UmbFolderToTreeItemMapper<FolderTreeItemType> = (item: UmbFolderModel) => FolderTreeItemType;

export class UmbFolderRepositoryBase extends UmbRepositoryBase implements IUmbFolderRepository {
	protected _init: Promise<unknown>;
	protected _treeStore?: UmbTreeStore;
	#folderDataSource: UmbFolderDataSource;
	#folderToTreeItemMapper: UmbFolderToTreeItemMapper<any>;

	constructor(
		host: UmbControllerHost,
		folderDataSource: UmbFolderDataSourceConstructor,
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

	async createScaffold(parentUnique: string | null) {
		if (parentUnique === undefined) throw new Error('Parent unique is missing');

		const scaffold = {
			name: '',
			parentUnique,
		};

		return { data: scaffold };
	}

	/**
	 * Creates a folder
	 * @param {UmbCreateFolderModel} args
	 * @return {*}
	 * @memberof UmbFolderRepositoryBase
	 */
	async create(args: UmbCreateFolderModel) {
		if (args.parentUnique === undefined) throw new Error('Parent unique is missing');
		if (!args.name) throw new Error('Name is missing');
		await this._init;

		const { data, error } = await this.#folderDataSource.create(args);

		if (data) {
			const folderTreeItem = this.#folderToTreeItemMapper(data);
			this._treeStore!.appendItems([folderTreeItem]);
		}

		return { error };
	}

	/**
	 * Updates a folder
	 * @param {UmbUpdateFolderModel} args
	 * @return {*}
	 * @memberof UmbFolderRepositoryBase
	 */
	async update(args: UmbUpdateFolderModel) {
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
	 * Deletes a folder
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbFolderRepositoryBase
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		await this._init;

		const { error } = await this.#folderDataSource.delete(unique);

		if (!error) {
			this._treeStore!.removeItem(unique);
		}

		return { error };
	}

	/**
	 * Request a folder
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbFolderRepositoryBase
	 */
	async request(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		await this._init;
		return await this.#folderDataSource.read(unique);
	}
}
