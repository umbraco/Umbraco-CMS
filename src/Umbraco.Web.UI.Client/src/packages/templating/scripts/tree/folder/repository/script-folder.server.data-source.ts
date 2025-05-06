import { UMB_SCRIPT_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';
import type { CreateScriptFolderRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { ScriptService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for Script folders that fetches data from the server
 * @class UmbScriptFolderServerDataSource
 * @implements {UmbDetailDataSource<UmbFolderModel>}
 */
export class UmbScriptFolderServerDataSource implements UmbDetailDataSource<UmbFolderModel> {
	#host: UmbControllerHost;
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();

	/**
	 * Creates an instance of UmbScriptFolderServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbScriptFolderServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a scaffold for a Script folder
	 * @param {Partial<UmbFolderModel>} [preset]
	 * @returns {*}
	 * @memberof UmbScriptFolderServerDataSource
	 */
	async createScaffold(preset?: Partial<UmbFolderModel>) {
		const scaffold: UmbFolderModel = {
			entityType: UMB_SCRIPT_FOLDER_ENTITY_TYPE,
			unique: UmbId.new(),
			name: '',
			...preset,
		};

		return { data: scaffold };
	}

	/**
	 * Fetches a Script folder from the server
	 * @param {string} unique
	 * @returns {UmbDataSourceResponse<UmbFolderModel>}
	 * @memberof UmbScriptFolderServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const path = this.#serverFilePathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Cannot read script folder without a path');

		const { data, error } = await tryExecute(
			this.#host,
			ScriptService.getScriptFolderByPath({
				path: { path: encodeURIComponent(path) },
			}),
		);

		if (data) {
			const mappedData = {
				entityType: UMB_SCRIPT_FOLDER_ENTITY_TYPE,
				unique: this.#serverFilePathUniqueSerializer.toUnique(data.path),
				parentUnique: data.parent ? this.#serverFilePathUniqueSerializer.toUnique(data.parent.path) : null,
				name: data.name,
			};

			return { data: mappedData };
		}

		return { error };
	}

	/**
	 * Creates a Script folder on the server
	 * @param {UmbFolderModel} model
	 * @returns {UmbDataSourceResponse<UmbFolderModel>}
	 * @memberof UmbScriptFolderServerDataSource
	 */
	async create(model: UmbFolderModel, parentUnique: string | null) {
		if (!model) throw new Error('Data is missing');
		if (!model.name) throw new Error('Name is missing');

		const parentPath = new UmbServerFilePathUniqueSerializer().toServerPath(parentUnique);

		const body: CreateScriptFolderRequestModel = {
			parent: parentPath ? { path: parentPath } : null,
			name: model.name,
		};

		const { data, error } = await tryExecute(
			this.#host,
			ScriptService.postScriptFolder({
				body,
			}),
		);

		if (data && typeof data === 'string') {
			const newPath = decodeURIComponent(data);
			const newPathUnique = this.#serverFilePathUniqueSerializer.toUnique(newPath);
			return this.read(newPathUnique);
		}

		return { error };
	}

	/**
	 * Deletes a Script folder on the server
	 * @param {string} unique
	 * @returns {UmbDataSourceErrorResponse}
	 * @memberof UmbScriptServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const path = this.#serverFilePathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Cannot delete script folder without a path');

		return tryExecute(
			this.#host,
			ScriptService.deleteScriptFolderByPath({
				path: { path: encodeURIComponent(path) },
			}),
		);
	}

	async update(): Promise<any> {
		throw new Error('Updating is not supported');
	}
}
