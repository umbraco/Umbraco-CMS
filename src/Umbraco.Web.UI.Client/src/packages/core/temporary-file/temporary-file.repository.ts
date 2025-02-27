import { UmbTemporaryFileServerDataSource } from './temporary-file.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

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
	 * @param {string} id
	 * @param {File} file
	 * @returns {*}
	 * @memberof UmbTemporaryFileRepository
	 */
	upload(id: string, file: File, onProgress?: (progress: ProgressEvent) => void) {
		return this.#source.create(id, file, onProgress);
	}

	/**
	 * Deletes a temporary file
	 * @param {string} id
	 * @returns {*}
	 * @memberof UmbTemporaryFileRepository
	 */
	delete(id: string) {
		return this.#source.delete(id);
	}

	/**
	 * Gets a temporary file
	 * @param {string} id
	 * @returns {*}
	 * @memberof UmbTemporaryFileRepository
	 */
	requestById(id: string) {
		return this.#source.read(id);
	}
}
