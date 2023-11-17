import { UmbCreateFolderModel, UmbFolderDataSource, UmbUpdateFolderModel } from '@umbraco-cms/backoffice/repository';
import { ScriptResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Script folders that fetches data from the server
 * @export
 * @class UmbScriptFolderServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbScriptFolderServerDataSource implements UmbFolderDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbScriptFolderServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbScriptFolderServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches a Script folder from the server
	 * @param {string} unique
	 * @return {UmbDataSourceResponse<UmbFolderModel>}
	 * @memberof UmbScriptFolderServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return tryExecuteAndNotify(
			this.#host,
			ScriptResource.getScriptFolder({
				path: unique,
			}),
		);
	}

	/**
	 * Creates a Script folder on the server
	 * @param {UmbCreateFolderModel} args
	 * @return {UmbDataSourceResponse<UmbFolderModel>}
	 * @memberof UmbScriptFolderServerDataSource
	 */
	async create(args: UmbCreateFolderModel) {
		if (args.parentUnique === undefined) throw new Error('Parent unique is missing');
		if (!args.name) throw new Error('Name is missing');

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			ScriptResource.postScriptFolder({
				requestBody: { parentPath: args.parentUnique, name: args.name },
			}),
		);

		if (data) {
			const folderData = { unique: data.path, parentUnique: data.parentPath || null, name: data.name };
			return { data: folderData };
		}

		return { error };
	}

	/**
	 * Updates a Script folder on the server
	 * @param {UmbUpdateFolderModel} args
	 * @return {UmbDataSourceErrorResponse}
	 * @memberof UmbScriptFolderServerDataSource
	 */
	async update(args: UmbUpdateFolderModel): Promise<any> {
		throw new Error('Not implemented. Missing server endpoint');
		/*
		if (!args.unique) throw new Error('Unique is missing');
		if (!args.name) throw new Error('Folder name is missing');
		return tryExecuteAndNotify(
			this.#host,
			ScriptResource.putScriptFolder({
				id: args.unique,
				requestBody: { name: args.name },
			}),
		);
		*/
	}

	/**
	 * Deletes a Script folder on the server
	 * @param {string} unique
	 * @return {UmbDataSourceErrorResponse}
	 * @memberof UmbScriptServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return tryExecuteAndNotify(
			this.#host,
			ScriptResource.deleteScriptFolder({
				path: unique,
			}),
		);
	}
}
