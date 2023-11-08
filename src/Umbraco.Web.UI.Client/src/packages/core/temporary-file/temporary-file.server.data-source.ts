import { TemporaryFileResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source to upload temporary files to the server
 * @export
 * @class UmbTemporaryFileServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbTemporaryFileServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbTemporaryFileServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbTemporaryFileServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Uploads a temporary file to the server
	 * @param {string} id
	 * @param {File} file
	 * @return {*}
	 * @memberof UmbTemporaryFileServerDataSource
	 */
	create(id: string, file: File) {
		debugger;
		return tryExecuteAndNotify(
			this.#host,
			TemporaryFileResource.postTemporaryfile({
				requestBody: {
					id,
					file,
				},
			}),
		);
	}

	/**
	 * Gets a temporary file from the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbTemporaryFileServerDataSource
	 */
	read(id: string) {
		if (!id) throw new Error('Id is missing');
		return tryExecuteAndNotify(this.#host, TemporaryFileResource.getTemporaryfileById({ id }));
	}

	/**
	 * Deletes a temporary file from the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbTemporaryFileServerDataSource
	 */
	delete(id: string) {
		if (!id) throw new Error('Id is missing');
		return tryExecuteAndNotify(this.#host, TemporaryFileResource.deleteTemporaryfileById({ id }));
	}
}
