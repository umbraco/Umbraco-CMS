import { v4 as uuidv4 } from 'uuid';
import { UmbFolderDataSource } from '@umbraco-cms/backoffice/repository';
import {
	DataTypeResource,
	FolderReponseModel,
	CreateFolderRequestModel,
	FolderModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for a Data Type folder that fetches data from the server
 * @export
 * @class UmbDataTypeFolderServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDataTypeFolderServerDataSource implements UmbFolderDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbDataTypeFolderServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Creates a Data Type folder with the given key from the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async createScaffold(parentId: string | null) {
		const scaffold: FolderReponseModel = {
			$type: 'FolderReponseModel',
			name: '',
			key: uuidv4(),
			parentId,
		};

		return { data: scaffold };
	}

	/**
	 * Fetches a Data Type folder with the given key from the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async get(key: string) {
		if (!key) throw new Error('Key is missing');
		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.getDataTypeFolderByKey({
				key,
			})
		);
	}

	/**
	 * Inserts a new Data Type folder on the server
	 * @param {folder} folder
	 * @return {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async insert(folder: CreateFolderRequestModel) {
		if (!folder) throw new Error('Folder is missing');
		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.postDataTypeFolder({
				requestBody: folder,
			})
		);
	}

	/**
	 * Updates a Data Type folder on the server
	 * @param {folder} folder
	 * @return {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async update(key: string, folder: FolderModelBaseModel) {
		if (!key) throw new Error('Key is missing');
		if (!key) throw new Error('Folder data is missing');
		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.putDataTypeFolderByKey({
				key,
				requestBody: folder,
			})
		);
	}

	/**
	 * Deletes a Data Type folder with the given key on the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async delete(key: string) {
		if (!key) throw new Error('Key is missing');
		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.deleteDataTypeFolderByKey({
				key,
			})
		);
	}
}
