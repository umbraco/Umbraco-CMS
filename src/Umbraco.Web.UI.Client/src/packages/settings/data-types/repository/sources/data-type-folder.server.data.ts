import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbFolderDataSource } from '@umbraco-cms/backoffice/repository';
import {
	DataTypeResource,
	FolderReponseModel,
	CreateFolderRequestModel,
	FolderModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
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
	 * Creates a Data Type folder with the given id from the server
	 * @param {string} parentId
	 * @return {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async createScaffold(parentId: string | null) {
		const scaffold: FolderReponseModel = {
			name: '',
			id: UmbId.new(),
			parentId,
		};

		return { data: scaffold };
	}

	/**
	 * Fetches a Data Type folder with the given id from the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async get(id: string) {
		if (!id) throw new Error('Key is missing');
		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.getDataTypeFolderById({
				id: id,
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
	async update(id: string, folder: FolderModelBaseModel) {
		if (!id) throw new Error('Key is missing');
		if (!id) throw new Error('Folder data is missing');
		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.putDataTypeFolderById({
				id: id,
				requestBody: folder,
			})
		);
	}

	/**
	 * Deletes a Data Type folder with the given id on the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async delete(id: string) {
		if (!id) throw new Error('Key is missing');
		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.deleteDataTypeFolderById({
				id: id,
			})
		);
	}
}
