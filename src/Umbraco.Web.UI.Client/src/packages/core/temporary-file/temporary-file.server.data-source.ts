import type { UmbDataSourceResponse } from '../repository/index.js';
import { TemporaryFileService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute, tryXhrRequest } from '@umbraco-cms/backoffice/resources';

/**
 * A data source to upload temporary files to the server
 * @class UmbTemporaryFileServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbTemporaryFileServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbTemporaryFileServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbTemporaryFileServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Uploads a temporary file to the server
	 * @param {string} id
	 * @param {File} file
	 * @returns {*}
	 * @memberof UmbTemporaryFileServerDataSource
	 */
	async create(
		id: string,
		file: File,
		onProgress?: (progress: ProgressEvent) => void,
		abortSignal?: AbortSignal,
	): Promise<UmbDataSourceResponse<unknown>> {
		const body = new FormData();
		body.append('Id', id);
		body.append('File', file);
		const xhrRequest = tryXhrRequest<unknown>(this.#host, {
			url: '/umbraco/management/api/v1/temporary-file',
			method: 'POST',
			responseHeader: 'Umb-Generated-Resource',
			disableNotifications: true,
			body,
			onProgress,
			abortSignal,
		});
		return xhrRequest;
	}

	/**
	 * Gets a temporary file from the server
	 * @param {string} id
	 * @returns {*}
	 * @memberof UmbTemporaryFileServerDataSource
	 */
	read(id: string) {
		if (!id) throw new Error('Id is missing');
		return tryExecute(this.#host, TemporaryFileService.getTemporaryFileById({ path: { id } }));
	}

	/**
	 * Deletes a temporary file from the server
	 * @param {string} id
	 * @returns {*}
	 * @memberof UmbTemporaryFileServerDataSource
	 */
	delete(id: string) {
		if (!id) throw new Error('Id is missing');
		return tryExecute(this.#host, TemporaryFileService.deleteTemporaryFileById({ path: { id } }));
	}
}
