import { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';
import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	UmbDataSourceErrorResponse,
	UmbDataSourceResponse,
	UmbDetailDataSource,
} from '@umbraco-cms/backoffice/repository';

/**
 * A data source for a Data Type folder that fetches data from the server
 * @class UmbDataTypeFolderServerDataSource
 * @implements {UmbDetailDataSource<UmbFolderModel>}
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
	 * @param {Partial<UmbFolderModel>} [preset] - The preset data to populate the scaffold with.
	 * @returns {Promise<UmbDataSourceResponse<UmbFolderModel>>} The data type folder scaffold.
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async createScaffold(preset?: Partial<UmbFolderModel>): Promise<UmbDataSourceResponse<UmbFolderModel>> {
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
	 * @param {string} unique - The unique identifier of the folder to fetch.
	 * @returns {Promise<UmbDataSourceResponse<UmbFolderModel>>} The data type folder.
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async read(unique: string): Promise<UmbDataSourceResponse<UmbFolderModel>> {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(
			this.#host,
			DataTypeService.getDataTypeFolderById({
				path: { id: unique },
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
	 * @param {UmbFolderModel} model - The data type folder to create.
	 * @returns {Promise<UmbDataSourceResponse<UmbFolderModel>>} The created data type folder.
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async create(model: UmbFolderModel, parentUnique: string | null): Promise<UmbDataSourceResponse<UmbFolderModel>> {
		if (!model) throw new Error('Data is missing');
		if (!model.unique) throw new Error('Unique is missing');
		if (!model.name) throw new Error('Name is missing');

		const body = {
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			name: model.name,
		};

		const { error } = await tryExecute(
			this.#host,
			DataTypeService.postDataTypeFolder({
				body,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Updates a Data Type folder on the server
	 * @param {UmbFolderModel} model - The data type folder to update.
	 * @returns {Promise<UmbDataSourceResponse<UmbFolderModel>>} The updated data type folder.
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async update(model: UmbFolderModel): Promise<UmbDataSourceResponse<UmbFolderModel>> {
		if (!model) throw new Error('Data is missing');
		if (!model.unique) throw new Error('Unique is missing');
		if (!model.name) throw new Error('Folder name is missing');

		const { error } = await tryExecute(
			this.#host,
			DataTypeService.putDataTypeFolderById({
				path: { id: model.unique },
				body: { name: model.name },
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a Data Type folder on the server
	 * @param {string} unique - The unique identifier of the folder to delete.
	 * @returns {Promise<UmbDataSourceErrorResponse>} The result of the delete operation.
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async delete(unique: string): Promise<UmbDataSourceErrorResponse> {
		if (!unique) throw new Error('Unique is missing');
		return tryExecute(
			this.#host,
			DataTypeService.deleteDataTypeFolderById({
				path: { id: unique },
			}),
		);
	}
}
