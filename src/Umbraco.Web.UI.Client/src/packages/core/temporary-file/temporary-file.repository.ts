import { UmbTemporaryFileServerDataSource } from './temporary-file.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbRepositoryErrorResponse, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { TemporaryFileResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * A repository for uploading temporary files
 * @class UmbTemporaryFileRepository
 * @augments {UmbRepositoryBase}
 */
export class UmbTemporaryFileRepository extends UmbRepositoryBase {
	#source: UmbTemporaryFileServerDataSource;

	/**
	 * Creates an instance of UmbTemporaryFileRepository.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbTemporaryFileRepository
	 */
	constructor(host: UmbControllerHost) {
		super(host);
		this.#source = new UmbTemporaryFileServerDataSource(host);
	}

	/**
	 * Uploads a temporary file
	 * @param {string} id - The unique identifier of the temporary file
	 * @param {File} file - The file to upload
	 * @param {(progress: ProgressEvent) => void} [onProgress] - Callback invoked with upload progress
	 * @param {AbortSignal} [abortSignal] - Signal to abort the upload
	 * @returns {Promise<UmbRepositoryResponse<unknown>>} The upload response
	 * @memberof UmbTemporaryFileRepository
	 */
	upload(
		id: string,
		file: File,
		onProgress?: (progress: ProgressEvent) => void,
		abortSignal?: AbortSignal,
	): Promise<UmbRepositoryResponse<unknown>> {
		return this.#source.create(id, file, onProgress, abortSignal);
	}

	/**
	 * Deletes a temporary file
	 * @param {string} id - The unique identifier of the temporary file
	 * @returns {Promise<UmbRepositoryErrorResponse>} The delete response
	 * @memberof UmbTemporaryFileRepository
	 */
	delete(id: string): Promise<UmbRepositoryErrorResponse> {
		return this.#source.delete(id);
	}

	/**
	 * Gets a temporary file
	 * @param {string} id - The unique identifier of the temporary file
	 * @returns {Promise<UmbRepositoryResponse<TemporaryFileResponseModel>>} The temporary file
	 * @memberof UmbTemporaryFileRepository
	 */
	requestById(id: string): Promise<UmbRepositoryResponse<TemporaryFileResponseModel>> {
		return this.#source.read(id);
	}
}
