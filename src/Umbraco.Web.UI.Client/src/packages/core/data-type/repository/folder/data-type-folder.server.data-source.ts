import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbFolderDataSource } from '@umbraco-cms/backoffice/repository';
import {
	DataTypeResource,
	FolderResponseModel,
	CreateFolderRequestModel,
	FolderModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
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
	 * Creates a Data Type folder with the given id from the server
	 * @param {string} parentId
	 * @return {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async createScaffold(parentId: string | null) {
		const scaffold: FolderResponseModel = {
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
		if (!id) throw new Error('Id is missing');
		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.getDataTypeFolderById({
				id: id,
			}),
		);
	}

	/**
	 * Inserts a new Data Type folder on the server
	 * @param {folder} folder
	 * @return {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async insert(id: string, parentId: string | null, name: string) {
		if (!id) throw new Error('Unique is missing');
		if (parentId === undefined) throw new Error('Parent unique is missing');
		if (!name) throw new Error('Name is missing');

		const requestBody: CreateFolderRequestModel = {
			id: id,
			parentId: parentId,
			name,
		};

		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.postDataTypeFolder({
				requestBody,
			}),
		);
	}

	/**
	 * Updates a Data Type folder on the server
	 * @param {folder} folder
	 * @return {*}
	 * @memberof UmbDataTypeFolderServerDataSource
	 */
	async update(id: string, name: string) {
		if (!id) throw new Error('Key is missing');
		if (!name) throw new Error('Folder name is missing');

		const requestBody: FolderModelBaseModel = {
			name,
		};

		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.putDataTypeFolderById({
				id,
				requestBody,
			}),
		);
	}

	/**
	 * Deletes a Data Type folder with the given id on the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async delete(id: string) {
		if (!id) throw new Error('Id is missing');

		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.deleteDataTypeFolderById({
				id: id,
			}),
		);
	}
}
