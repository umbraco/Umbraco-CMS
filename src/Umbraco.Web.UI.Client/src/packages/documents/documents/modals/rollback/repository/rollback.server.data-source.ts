import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DocumentVersionResource } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Rollback that fetches data from the server
 * @export
 * @class UmbRollbackServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbRollbackServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbRollbackServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbRollbackServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get a list of versions for a document
	 * @return {*}
	 * @memberof UmbRollbackServerDataSource
	 */
	getVersionsByDocumentId(id: string, culture: string) {
		return tryExecuteAndNotify(this.#host, DocumentVersionResource.getDocumentVersion({ documentId: id, culture }));
	}

	/**
	 * Get a specific version by id
	 * @return {*}
	 * @memberof UmbRollbackServerDataSource
	 */
	getVersionById(versionId: string) {
		return tryExecuteAndNotify(this.#host, DocumentVersionResource.getDocumentVersionById({ id: versionId }));
	}

	setPreventCleanup(versionId: string, preventCleanup: boolean) {
		return tryExecuteAndNotify(
			this.#host,
			DocumentVersionResource.putDocumentVersionByIdPreventCleanup({ id: versionId, preventCleanup }),
		);
	}

	rollback(versionId: string, culture?: string) {
		return tryExecuteAndNotify(
			this.#host,
			DocumentVersionResource.postDocumentVersionByIdRollback({ id: versionId, culture }),
		);
	}
}
