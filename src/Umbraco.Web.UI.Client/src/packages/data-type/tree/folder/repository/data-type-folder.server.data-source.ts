import { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';
import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for a Data Type folder that fetches data from the server
 * @class UmbDataTypeFolderServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDataTypeFolderServerDataSource implements UmbDetailDataSource<UmbFolderModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDataTypeFolderServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a scaffold for a Data Type folder
	 * @param {Partial<UmbFolderModel>} [preset]
	 * @returns {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async createScaffold(preset?: Partial<UmbFolderModel>) {
		const scaffold: UmbFolderModel = {
			entityType: UMB_DATA_TYPE_FOLDER_ENTITY_TYPE,
			unique: UmbId.new(),
			name: '',
			...preset,
		};

		return { data: scaffold };
	}

	/**
	 * Fetches a Data Type folder from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(
			this.#host,
			DataTypeService.getDataTypeFolderById({
				id: unique,
			}),
		);

		if (data) {
			const mappedData = {
				entityType: UMB_DATA_TYPE_FOLDER_ENTITY_TYPE,
				unique: data.id,
				name: data.name,
			};

			return { data: mappedData };
		}

		return { error };
	}

	/**
	 * Creates a Data Type folder on the server
	 * @param {UmbFolderModel} model
	 * @returns {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async create(model: UmbFolderModel, parentUnique: string | null) {
		if (!model) throw new Error('Data is missing');
		if (!model.unique) throw new Error('Unique is missing');
		if (!model.name) throw new Error('Name is missing');

		const requestBody = {
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			name: model.name,
		};

		const { error } = await tryExecute(
			this.#host,
			DataTypeService.postDataTypeFolder({
				requestBody,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Updates a Data Type folder on the server
	 * @param {UmbFolderModel} model
	 * @returns {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async update(model: UmbFolderModel) {
		if (!model) throw new Error('Data is missing');
		if (!model.unique) throw new Error('Unique is missing');
		if (!model.name) throw new Error('Folder name is missing');

		const { error } = await tryExecute(
			this.#host,
			DataTypeService.putDataTypeFolderById({
				id: model.unique,
				requestBody: { name: model.name },
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a Data Type folder on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return tryExecute(
			this.#host,
			DataTypeService.deleteDataTypeFolderById({
				id: unique,
			}),
		);
	}
}
