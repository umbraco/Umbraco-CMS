import { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { UmbCreateFolderModel, UmbFolderDataSource, UmbUpdateFolderModel } from '@umbraco-cms/backoffice/tree';
import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for a Data Type folder that fetches data from the server
 * @class UmbDataTypeFolderServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDataTypeFolderServerDataSource implements UmbFolderDataSource {
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
	 * Fetches a Data Type folder from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecuteAndNotify(
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
	 * @param {UmbCreateFolderModel} args
	 * @returns {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async create(args: UmbCreateFolderModel) {
		if (args.parentUnique === undefined) throw new Error('Parent unique is missing');
		if (!args.name) throw new Error('Name is missing');

		const requestBody = {
			id: args.unique,
			parent: args.parentUnique ? { id: args.parentUnique } : null,
			name: args.name,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			DataTypeService.postDataTypeFolder({
				requestBody,
			}),
		);

		if (!error) {
			return this.read(args.unique);
		}

		return { error };
	}

	/**
	 * Updates a Data Type folder on the server
	 * @param {UmbUpdateFolderModel} args
	 * @returns {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async update(args: UmbUpdateFolderModel) {
		if (!args.unique) throw new Error('Unique is missing');
		if (!args.name) throw new Error('Folder name is missing');

		const { error } = await tryExecuteAndNotify(
			this.#host,
			DataTypeService.putDataTypeFolderById({
				id: args.unique,
				requestBody: { name: args.name },
			}),
		);

		if (!error) {
			return this.read(args.unique);
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
		return tryExecuteAndNotify(
			this.#host,
			DataTypeService.deleteDataTypeFolderById({
				id: unique,
			}),
		);
	}
}
