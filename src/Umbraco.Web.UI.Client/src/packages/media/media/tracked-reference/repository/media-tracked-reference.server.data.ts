import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { MediaResource } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * @export
 * @class UmbUserGroupCollectionServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMediaTrackedReferenceServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMediaTrackedReferenceServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaTrackedReferenceServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the item for the given id from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbDataTypeItemServerDataSource
	 */
	async getTrackedReferenceById(id: string, skip = 0, take = 20) {
		return await tryExecuteAndNotify(this.#host, MediaResource.getMediaByIdReferencedBy({ id, skip, take }));
	}

	/**
	 * Fetches the item descendant for the given id from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbMediaTrackedReferenceServerDataSource
	 */
	async getTrackedReferenceDescendantsByParentId(parentId: string, skip = 0, take = 20) {
		return await tryExecuteAndNotify(
			this.#host,
			MediaResource.getMediaByIdReferencedDescendants({
				id: parentId,
				skip,
				take,
			}),
		);
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbMediaTrackedReferenceServerDataSource
	 */
	async getTrackedReferenceItem(id: string[], skip = 0, take = 20) {
		return await tryExecuteAndNotify(
			this.#host,
			MediaResource.getMediaAreReferenced({
				id,
				skip,
				take,
			}),
		);
	}
}
