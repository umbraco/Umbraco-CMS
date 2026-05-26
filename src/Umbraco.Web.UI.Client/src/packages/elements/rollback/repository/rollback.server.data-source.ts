import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { ElementVersionService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the element rollback feature that fetches data from the server.
 * @class UmbElementRollbackServerDataSource
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
	 * Get a list of versions for an element.
	 * @param {string} id - The unique ID of the element
	 * @param {string} [culture] - Optional culture to filter versions by
	 * @returns {*} The list of versions
	 * @memberof UmbElementRollbackServerDataSource
	 */
	getVersionsByElementId(id: string, culture?: string) {
		return tryExecute(this.#host, ElementVersionService.getElementVersion({ query: { elementId: id, culture } }));
	}

	/**
	 * Get a specific version by id.
	 * @param {string} versionId - The unique ID of the version
	 * @returns {*} The version data
	 * @memberof UmbElementRollbackServerDataSource
	 */
	getVersionById(versionId: string) {
		return tryExecute(this.#host, ElementVersionService.getElementVersionById({ path: { id: versionId } }));
	}

	/**
	 * Toggle whether a specific version is excluded from automatic content version cleanup.
	 * @param {string} versionId - The unique ID of the version
	 * @param {boolean} preventCleanup - `true` to prevent cleanup, `false` to allow it
	 * @returns {*} The result of the operation
	 * @memberof UmbElementRollbackServerDataSource
	 */
	setPreventCleanup(versionId: string, preventCleanup: boolean) {
		return tryExecute(
			this.#host,
			ElementVersionService.putElementVersionByIdPreventCleanup({
				path: { id: versionId },
				query: { preventCleanup },
			}),
		);
	}

	/**
	 * Roll the element back to a specific version.
	 * @param {string} versionId - The unique ID of the version to roll back to
	 * @param {string} [culture] - Optional culture to roll back
	 * @returns {*} The result of the operation
	 * @memberof UmbElementRollbackServerDataSource
	 */
	rollback(versionId: string, culture?: string) {
		return tryExecute(
			this.#host,
			ElementVersionService.postElementVersionByIdRollback({ path: { id: versionId }, query: { culture } }),
		);
	}
}
