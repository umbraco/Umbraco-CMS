import { documentItemCache } from '../../item/repository/document-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	DocumentVersionService,
	type DocumentVersionResponseModel,
	type PagedDocumentVersionItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for the Rollback that fetches data from the server
 * @class UmbRollbackServerDataSource
 */
export class UmbRollbackServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbRollbackServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbRollbackServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get a list of versions for a document
	 * @param {string} id - The document unique identifier.
	 * @param {string} [culture] - The culture to get versions for.
	 * @returns {Promise<UmbDataSourceResponse<PagedDocumentVersionItemResponseModel>>} The versions for the document.
	 * @memberof UmbRollbackServerDataSource
	 */
	getVersionsByDocumentId(
		id: string,
		culture?: string,
	): Promise<UmbDataSourceResponse<PagedDocumentVersionItemResponseModel>> {
		return tryExecute(this.#host, DocumentVersionService.getDocumentVersion({ query: { documentId: id, culture } }));
	}

	/**
	 * Get a specific version by id
	 * @param {string} versionId - The version unique identifier.
	 * @returns {Promise<UmbDataSourceResponse<DocumentVersionResponseModel>>} The requested version.
	 * @memberof UmbRollbackServerDataSource
	 */
	getVersionById(versionId: string): Promise<UmbDataSourceResponse<DocumentVersionResponseModel>> {
		return tryExecute(this.#host, DocumentVersionService.getDocumentVersionById({ path: { id: versionId } }));
	}

	setPreventCleanup(versionId: string, preventCleanup: boolean) {
		return tryExecute(
			this.#host,
			DocumentVersionService.putDocumentVersionByIdPreventCleanup({
				path: { id: versionId },
				query: { preventCleanup },
			}),
		);
	}

	async rollback(versionId: string, culture?: string) {
		const result = await tryExecute(
			this.#host,
			DocumentVersionService.postDocumentVersionByIdRollback({ path: { id: versionId }, query: { culture } }),
		);
		if (!result.error) {
			// The server event that invalidates the cached item can arrive after the response. The rollback is
			// addressed by version, so the version is read to find the document it belongs to.
			const { data: version } = await this.getVersionById(versionId);
			if (version?.document) {
				documentItemCache.delete(version.document.id);
			}
		}
		return result;
	}
}
