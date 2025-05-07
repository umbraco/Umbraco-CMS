import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * @class UmbUserGroupCollectionServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentPermissionServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentPermissionServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentPermissionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	requestPermissions(id: string) {
		return tryExecute(
			this.#host,
			fetch(`/umbraco/management/api/v1/document/${id}/permissions`, {
				method: 'GET',
				headers: {
					'Content-Type': 'application/json',
				},
			}).then((res) => res.json()),
		);
	}
}
