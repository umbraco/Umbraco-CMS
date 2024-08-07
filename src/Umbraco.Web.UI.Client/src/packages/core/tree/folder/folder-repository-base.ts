import { UmbRepositoryBase } from '../../repository/repository-base.js';
import type { UmbFolderRepository } from './folder-repository.interface.js';
import type { UmbFolderDataSource, UmbFolderDataSourceConstructor } from './folder-data-source.interface.js';
import type { UmbCreateFolderModel, UmbUpdateFolderModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';

export abstract class UmbFolderRepositoryBase extends UmbRepositoryBase implements UmbFolderRepository {
	protected _init: Promise<unknown>;
	#folderDataSource: UmbFolderDataSource;
	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost, folderDataSource: UmbFolderDataSourceConstructor) {
		super(host);
		this.#folderDataSource = new folderDataSource(this);

		this._init = Promise.all([
			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	/**
	 * Creates a scaffold for a folder
	 * @returns {*}
	 * @memberof UmbFolderRepositoryBase
	 */
	async createScaffold() {
		const scaffold = {
			unique: UmbId.new(),
			name: '',
		};

		return { data: scaffold };
	}

	/**
	 * Creates a folder
	 * @param {UmbCreateFolderModel} args
	 * @returns {*}
	 * @memberof UmbFolderRepositoryBase
	 */
	async create(args: UmbCreateFolderModel) {
		if (args.parentUnique === undefined) throw new Error('Parent unique is missing');
		if (!args.name) throw new Error('Name is missing');
		await this._init;

		const { data, error } = await this.#folderDataSource.create(args);

		if (data) {
			const notification = { data: { message: `Folder created` } };
			this.#notificationContext!.peek('positive', notification);

			return { data };
		}

		return { error };
	}

	/**
	 * Request a folder
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbFolderRepositoryBase
	 */
	async request(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		await this._init;
		return await this.#folderDataSource.read(unique);
	}

	/**
	 * Updates a folder
	 * @param {UmbUpdateFolderModel} args
	 * @returns {*}
	 * @memberof UmbFolderRepositoryBase
	 */
	async update(args: UmbUpdateFolderModel) {
		if (!args.unique) throw new Error('Unique is missing');
		if (!args.name) throw new Error('Folder name is missing');
		await this._init;

		const { data, error } = await this.#folderDataSource.update(args);

		if (data) {
			const notification = { data: { message: `Folder updated` } };
			this.#notificationContext!.peek('positive', notification);
		}

		return { data, error };
	}

	/**
	 * Deletes a folder
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbFolderRepositoryBase
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		await this._init;

		const { error } = await this.#folderDataSource.delete(unique);

		if (!error) {
			const notification = { data: { message: `Folder deleted` } };
			this.#notificationContext!.peek('positive', notification);
		}

		return { error };
	}
}
