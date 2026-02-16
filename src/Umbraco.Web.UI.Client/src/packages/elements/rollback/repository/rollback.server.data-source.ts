import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { ElementVersionService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Rollback that fetches data from the server
 * @class UmbElementRollbackServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbElementRollbackServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbElementRollbackServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbElementRollbackServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get a list of versions for a element
	 * @param id
	 * @param culture
	 * @returns {*}
	 * @memberof UmbElementRollbackServerDataSource
	 */
	getVersionsByElementId(id: string, culture?: string) {
		return tryExecute(this.#host, ElementVersionService.getElementVersion({ query: { elementId: id, culture } }));
	}

	/**
	 * Get a specific version by id
	 * @param versionId
	 * @returns {*}
	 * @memberof UmbElementRollbackServerDataSource
	 */
	getVersionById(versionId: string) {
		return tryExecute(this.#host, ElementVersionService.getElementVersionById({ path: { id: versionId } }));
	}

	setPreventCleanup(versionId: string, preventCleanup: boolean) {
		return tryExecute(
			this.#host,
			ElementVersionService.putElementVersionByIdPreventCleanup({
				path: { id: versionId },
				query: { preventCleanup },
			}),
		);
	}

	rollback(versionId: string, culture?: string) {
		return tryExecute(
			this.#host,
			ElementVersionService.postElementVersionByIdRollback({ path: { id: versionId }, query: { culture } }),
		);
	}
}
