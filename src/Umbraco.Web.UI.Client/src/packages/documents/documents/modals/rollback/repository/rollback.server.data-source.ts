import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DocumentVersionService } from '@umbraco-cms/backoffice/external/backend-api';
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
	 * @returns {*}
	 * @memberof UmbRollbackServerDataSource
	 */
	getVersionsByDocumentId(id: string, culture?: string) {
		return tryExecuteAndNotify(this.#host, DocumentVersionService.getDocumentVersion({ documentId: id, culture }));
	}

	/**
	 * Get a specific version by id
	 * @returns {*}
	 * @memberof UmbRollbackServerDataSource
	 */
	getVersionById(versionId: string) {
		return tryExecuteAndNotify(this.#host, DocumentVersionService.getDocumentVersionById({ id: versionId }));
	}

	setPreventCleanup(versionId: string, preventCleanup: boolean) {
		return tryExecuteAndNotify(
			this.#host,
			DocumentVersionService.putDocumentVersionByIdPreventCleanup({ id: versionId, preventCleanup }),
		);
	}

	rollback(versionId: string, culture?: string) {
		return tryExecuteAndNotify(
			this.#host,
			DocumentVersionService.postDocumentVersionByIdRollback({ id: versionId, culture }),
		);
	}
}
