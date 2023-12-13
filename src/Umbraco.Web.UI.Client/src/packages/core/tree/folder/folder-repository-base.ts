import { UmbRepositoryBase } from '../../repository/repository-base.js';
import { type UmbFolderRepository } from './folder-repository.interface.js';
import type { UmbFolderDataSource, UmbFolderDataSourceConstructor } from './folder-data-source.interface.js';
import { UmbCreateFolderModel, UmbFolderModel, UmbUpdateFolderModel } from './types.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbTreeItemModelBase, UmbTreeStore } from '@umbraco-cms/backoffice/tree';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbId } from '@umbraco-cms/backoffice/id';

export type UmbFolderToTreeItemMapper<FolderTreeItemType extends UmbTreeItemModelBase> = (
	item: UmbFolderModel,
) => FolderTreeItemType;

export abstract class UmbFolderRepositoryBase<FolderTreeItemType extends UmbTreeItemModelBase>
	extends UmbRepositoryBase
	implements UmbFolderRepository
{
	protected _init: Promise<unknown>;
	protected _treeStore?: UmbTreeStore<FolderTreeItemType>;
	#folderDataSource: UmbFolderDataSource;
	#folderToTreeItemMapper: UmbFolderToTreeItemMapper<FolderTreeItemType>;

	constructor(
		host: UmbControllerHost,
		folderDataSource: UmbFolderDataSourceConstructor,
		treeStoreContextAlias: string | UmbContextToken<any>,
		folderToTreeItemMapper: UmbFolderToTreeItemMapper<FolderTreeItemType>,
	) {
		super(host);
		this.#folderDataSource = new folderDataSource(this);
		this.#folderToTreeItemMapper = folderToTreeItemMapper;

		this._init = this.consumeContext(treeStoreContextAlias, (instance) => {
			this._treeStore = instance;
		}).asPromise();
	}

	async createScaffold(parentUnique: string | null) {
		if (parentUnique === undefined) throw new Error('Parent unique is missing');

		const scaffold = {
			unique: UmbId.new(),
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
			this._treeStore!.append(folderTreeItem);
			return { data };
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

		const { data, error } = await this.#folderDataSource.update(args);

		if (data) {
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			// TODO: I don't know why typescript is complaining about the name prop
			this._treeStore!.updateItem(args.unique, { name: data.name });
		}

		return { data, error };
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
