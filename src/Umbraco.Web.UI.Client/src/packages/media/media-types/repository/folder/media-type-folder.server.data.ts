import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbFolderDataSource } from '@umbraco-cms/backoffice/tree';
import {
	MediaTypeResource,
	FolderResponseModel,
	CreateFolderRequestModel,
	FolderModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for a Data Type folder that fetches data from the server
 * @export
 * @class UmbMediaTypeFolderServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMediaTypeFolderServerDataSource implements UmbFolderDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMediaTypeFolderServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaTypeFolderServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a Data Type folder with the given id from the server
	 * @param {string} parentId
	 * @return {*}
	 * @memberof UmbMediaTypeFolderServerDataSource
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
	 * @memberof UmbMediaTypeFolderServerDataSource
	 */
	async read(id: string) {
		if (!id) throw new Error('Key is missing');
		return tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.getMediaTypeFolderById({
				id: id,
			}),
		);
	}

	/**
	 * Inserts a new Data Type folder on the server
	 * @param {folder} folder
	 * @return {*}
	 * @memberof UmbMediaTypeFolderServerDataSource
	 */
	async create(folder: CreateFolderRequestModel) {
		if (!folder) throw new Error('Folder is missing');
		return tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.postMediaTypeFolder({
				requestBody: folder,
			}),
		);
	}

	/**
	 * Updates a Data Type folder on the server
	 * @param {folder} folder
	 * @return {*}
	 * @memberof UmbMediaTypeFolderServerDataSource
	 */
	async update(id: string, folder: FolderModelBaseModel) {
		if (!id) throw new Error('Key is missing');
		if (!id) throw new Error('Folder data is missing');
		return tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.putMediaTypeFolderById({
				id: id,
				requestBody: folder,
			}),
		);
	}

	/**
	 * Deletes a Data Type folder with the given id on the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbMediaTypeServerDataSource
	 */
	async delete(id: string) {
		if (!id) throw new Error('Key is missing');
		return tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.deleteMediaTypeFolderById({
				id: id,
			}),
		);
	}
}
