import { UmbCreateFolderModel, UmbFolderDataSource, UmbUpdateFolderModel } from '@umbraco-cms/backoffice/repository';
import { DataTypeResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for a Data Type folder that fetches data from the server
 * @export
 * @class UmbDataTypeFolderServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDataTypeFolderServerDataSource implements UmbFolderDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDataTypeFolderServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches a Data Type folder from the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.getDataTypeFolderById({
				id: unique,
			}),
		);
	}

	/**
	 * Creates a Data Type folder on the server
	 * @param {UmbCreateFolderModel} args
	 * @return {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async create(args: UmbCreateFolderModel) {
		if (args.parentUnique === undefined) throw new Error('Parent unique is missing');
		if (!args.name) throw new Error('Name is missing');
		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.postDataTypeFolder({
				requestBody: { parentId: args.parentUnique, name: args.name },
			}),
		);
	}

	/**
	 * Updates a Data Type folder on the server
	 * @param {UmbUpdateFolderModel} args
	 * @return {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async update(args: UmbUpdateFolderModel) {
		if (!args.unique) throw new Error('Unique is missing');
		if (!args.name) throw new Error('Folder name is missing');
		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.putDataTypeFolderById({
				id: args.unique,
				requestBody: { name: args.name },
			}),
		);
	}

	/**
	 * Deletes a Data Type folder on the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.deleteDataTypeFolderById({
				id: unique,
			}),
		);
	}
}
