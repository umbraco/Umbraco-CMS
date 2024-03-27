import { DocumentResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * @export
 * @class UmbUserGroupCollectionServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentTrackedReferenceServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentTrackedReferenceServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentTrackedReferenceServerDataSource
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
		return await tryExecuteAndNotify(this.#host, DocumentResource.getDocumentByIdReferencedBy({ id, skip, take }));
	}

	/**
	 * Fetches the item descendant for the given id from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbDocumentTrackedReferenceServerDataSource
	 */
	async getTrackedReferenceDescendantsByParentId(parentId: string, skip = 0, take = 20) {
		return await tryExecuteAndNotify(
			this.#host,
			DocumentResource.getDocumentByIdReferencedDescendants({
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
	 * @memberof UmbDocumentTrackedReferenceServerDataSource
	 */
	async getTrackedReferenceItem(id: string[], skip = 0, take = 20) {
		return await tryExecuteAndNotify(
			this.#host,
			DocumentResource.getDocumentAreReferenced({
				id,
				skip,
				take,
			}),
		);
	}
}
