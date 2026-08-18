import type { UmbDataSourceErrorResponse, UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { TemporaryFileService } from '@umbraco-cms/backoffice/external/backend-api';
import type { TemporaryFileResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute, tryXhrRequest } from '@umbraco-cms/backoffice/resources';

/**
 * A data source to upload temporary files to the server
 * @class UmbTemporaryFileServerDataSource
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
	 * @param {string} id - The unique identifier of the temporary file
	 * @param {File} file - The file to upload
	 * @param onProgress
	 * @param abortSignal
	 * @returns {Promise<UmbDataSourceResponse<unknown>>} The upload response
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
	 * @param {string} id - The unique identifier of the temporary file
	 * @returns {Promise<UmbDataSourceResponse<TemporaryFileResponseModel>>} The temporary file
	 * @memberof UmbTemporaryFileServerDataSource
	 */
	read(id: string): Promise<UmbDataSourceResponse<TemporaryFileResponseModel>> {
		if (!id) throw new Error('Id is missing');
		return tryExecute(this.#host, TemporaryFileService.getTemporaryFileById({ path: { id } }));
	}

	/**
	 * Deletes a temporary file from the server
	 * @param {string} id - The unique identifier of the temporary file
	 * @returns {Promise<UmbDataSourceErrorResponse>} The delete response
	 * @memberof UmbTemporaryFileServerDataSource
	 */
	delete(id: string): Promise<UmbDataSourceErrorResponse> {
		if (!id) throw new Error('Id is missing');
		return tryExecute(this.#host, TemporaryFileService.deleteTemporaryFileById({ path: { id } }));
	}
}
