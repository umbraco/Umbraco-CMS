import { UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';
import { MediaTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	UmbDataSourceErrorResponse,
	UmbDataSourceResponse,
	UmbDetailDataSource,
} from '@umbraco-cms/backoffice/repository';

/**
 * A data source for a Media Type folder that fetches data from the server
 * @class UmbMediaTypeFolderServerDataSource
 * @implements {UmbDetailDataSource<UmbFolderModel>}
 */
export class UmbMediaTypeFolderServerDataSource implements UmbDetailDataSource<UmbFolderModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMediaTypeFolderServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMediaTypeFolderServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a scaffold for a Media Type folder
	 * @param {Partial<UmbFolderModel>} [preset] - The preset to use for the scaffold
	 * @returns {Promise<UmbDataSourceResponse<UmbFolderModel>>} The scaffolded Media Type folder
	 * @memberof UmbMediaTypeFolderServerDataSource
	 */
	async createScaffold(preset?: Partial<UmbFolderModel>): Promise<UmbDataSourceResponse<UmbFolderModel>> {
		const scaffold: UmbFolderModel = {
			entityType: UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE,
			unique: UmbId.new(),
			name: '',
			...preset,
		};

		return { data: scaffold };
	}

	/**
	 * Fetches a Media Type folder from the server
	 * @param {string} unique - The unique ID of the Media Type folder
	 * @returns {Promise<UmbDataSourceResponse<UmbFolderModel>>} The Media Type folder
	 * @memberof UmbMediaTypeFolderServerDataSource
	 */
	async read(unique: string): Promise<UmbDataSourceResponse<UmbFolderModel>> {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(
			this.#host,
			MediaTypeService.getMediaTypeFolderById({
				path: { id: unique },
			}),
		);

		if (data) {
			const mappedData = {
				entityType: UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE,
				unique: data.id,
				name: data.name,
			};

			return { data: mappedData };
		}

		return { error };
	}

	/**
	 * Creates a Media Type folder on the server
	 * @param {UmbFolderModel} model - The Media Type folder to create
	 * @returns {Promise<UmbDataSourceResponse<UmbFolderModel>>} The created Media Type folder
	 * @memberof UmbMediaTypeFolderServerDataSource
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
			MediaTypeService.postMediaTypeFolder({
				body,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Updates a Media Type folder on the server
	 * @param {UmbFolderModel} model - The Media Type folder to update
	 * @returns {Promise<UmbDataSourceResponse<UmbFolderModel>>} The updated Media Type folder
	 * @memberof UmbMediaTypeFolderServerDataSource
	 */
	async update(model: UmbFolderModel): Promise<UmbDataSourceResponse<UmbFolderModel>> {
		if (!model) throw new Error('Data is missing');
		if (!model.unique) throw new Error('Unique is missing');
		if (!model.name) throw new Error('Folder name is missing');

		const { error } = await tryExecute(
			this.#host,
			MediaTypeService.putMediaTypeFolderById({
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
	 * Deletes a Media Type folder on the server
	 * @param {string} unique - The unique ID of the Media Type folder
	 * @returns {Promise<UmbDataSourceErrorResponse>} A promise that resolves once the Media Type folder has been deleted
	 * @memberof UmbMediaTypeFolderServerDataSource
	 */
	async delete(unique: string): Promise<UmbDataSourceErrorResponse> {
		if (!unique) throw new Error('Unique is missing');
		return tryExecute(
			this.#host,
			MediaTypeService.deleteMediaTypeFolderById({
				path: { id: unique },
			}),
		);
	}
}
