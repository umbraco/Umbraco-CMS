import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { UmbElementFolderModel } from '../types.js';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { ElementService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

/**
 * A data source for a Element folder that fetches data from the server
 * @class UmbElementFolderServerDataSource
 * @implements {UmbDetailDataSource}
 */
export class UmbElementFolderServerDataSource implements UmbDetailDataSource<UmbElementFolderModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbElementFolderServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbElementFolderServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a scaffold for a Element folder
	 * @returns {*}
	 * @memberof UmbElementFolderServerDataSource
	 */
	async createScaffold() {
		const scaffold: UmbElementFolderModel = {
			entityType: UMB_ELEMENT_FOLDER_ENTITY_TYPE,
			unique: UmbId.new(),
			name: '',
		};

		return { data: scaffold };
	}

	/**
	 * Fetches a Element folder from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbElementFolderServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(
			this.#host,
			ElementService.getElementFolderById({
				path: { id: unique },
			}),
		);

		if (data) {
			const mappedData = {
				entityType: UMB_ELEMENT_FOLDER_ENTITY_TYPE,
				unique: data.id,
				name: data.name,
				// TODO: [LK] We need to have `isTrashed` returned from the server, see endpoint "/umbraco/management/api/v1/element/folder/{id}".
				isTrashed: false,
			};

			return { data: mappedData };
		}

		return { error };
	}

	/**
	 * Creates a Element folder on the server
	 * @param {UmbCreateFolderModel} model
	 * @returns {*}
	 * @memberof UmbElementFolderServerDataSource
	 */
	async create(model: UmbFolderModel, parentUnique: string | null) {
		if (!model) throw new Error('Model is missing');
		if (!model.unique) throw new Error('Unique is missing');
		if (!model.name) throw new Error('Name is missing');

		const body = {
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			name: model.name,
		};

		const { error } = await tryExecute(
			this.#host,
			ElementService.postElementFolder({
				body,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Updates a Element folder on the server
	 * @param {UmbFolderModel} model
	 * @returns {*}
	 * @memberof UmbElementFolderServerDataSource
	 */
	async update(model: UmbFolderModel) {
		if (!model.unique) throw new Error('Unique is missing');
		if (!model.name) throw new Error('Folder name is missing');

		const { error } = await tryExecute(
			this.#host,
			ElementService.putElementFolderById({
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
	 * Deletes a Element folder on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbElementFolderServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return tryExecute(
			this.#host,
			ElementService.deleteElementFolderById({
				path: { id: unique },
			}),
		);
	}
}
