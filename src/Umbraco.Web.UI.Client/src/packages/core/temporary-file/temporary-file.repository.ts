import { UmbTemporaryFileServerDataSource } from './temporary-file.server.data-source.js';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

/**
 * A repository for uploading temporary files
 * @export
 * @class UmbTemporaryFileRepository
 * @extends {UmbRepositoryBase}
 */
export class UmbTemporaryFileRepository extends UmbRepositoryBase {
	#source: UmbTemporaryFileServerDataSource;

	/**
	 * Creates an instance of UmbTemporaryFileRepository.
	 * @param {UmbControllerHost} host
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
	 * @return {*}
	 * @memberof UmbTemporaryFileRepository
	 */
	upload(id: string, file: File) {
		return this.#source.create('id', file);
	}

	/**
	 * Deletes a temporary file
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbTemporaryFileRepository
	 */
	delete(id: string) {
		return this.#source.delete(id);
	}

	/**
	 * Gets a temporary file
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbTemporaryFileRepository
	 */
	requestById(id: string) {
		return this.#source.read(id);
	}
}
