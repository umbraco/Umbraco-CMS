import { RelationResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Relation that fetches data from the server
 * @export
 * @class UmbRelationServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbRelationServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbRelationServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbRelationServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches relations by the given id from the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbRelationServerDataSource
	 */
	async read(id: string) {
		if (!id) {
			throw new Error('Id is missing');
		}

		return tryExecuteAndNotify(
			this.#host,
			RelationResource.getRelationTypeById({
				id,
			}),
		);
	}
}
