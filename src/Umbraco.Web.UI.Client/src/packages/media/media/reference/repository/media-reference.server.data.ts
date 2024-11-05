import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { MediaService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * @class UmbMediaReferenceServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMediaReferenceServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMediaReferenceServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMediaReferenceServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the item for the given id from the server
	 * @param {Array<string>} ids
	 * @returns {*}
	 * @memberof UmbMediaReferenceServerDataSource
	 */
	async getReferencedBy(id: string, skip = 0, take = 20) {
		return await tryExecuteAndNotify(this.#host, MediaService.getMediaByIdReferencedBy({ id, skip, take }));
	}
}
