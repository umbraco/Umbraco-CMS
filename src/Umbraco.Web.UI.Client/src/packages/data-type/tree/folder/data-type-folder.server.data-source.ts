import type { UmbCreateFolderModel, UmbFolderDataSource, UmbUpdateFolderModel } from '@umbraco-cms/backoffice/tree';
import { DataTypeResource } from '@umbraco-cms/backoffice/external/backend-api';
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

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			DataTypeResource.getDataTypeFolderById({
				id: unique,
			}),
		);

		if (data) {
			const mappedData = {
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
	 * @return {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async create(args: UmbCreateFolderModel) {
		if (args.parentUnique === undefined) throw new Error('Parent unique is missing');
		if (!args.name) throw new Error('Name is missing');

		const requestBody = {
			id: args.unique,
			parentId: args.parentUnique,
			name: args.name,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			DataTypeResource.postDataTypeFolder({
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
	 * @return {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async update(args: UmbUpdateFolderModel) {
		if (!args.unique) throw new Error('Unique is missing');
		if (!args.name) throw new Error('Folder name is missing');

		const { error } = await tryExecuteAndNotify(
			this.#host,
			DataTypeResource.putDataTypeFolderById({
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
