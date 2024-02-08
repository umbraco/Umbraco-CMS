import { DataTypeResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbMoveDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for Data Type items that fetches data from the server
 * @export
 * @class UmbDataTypeMoveServerDataSource
 */
export class UmbDataTypeMoveServerDataSource implements UmbMoveDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDataTypeMoveServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDataTypeMoveServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Move an item for the given id to the target unique
	 * @param {string} unique
	 * @param {(string | null)} targetUnique
	 * @return {*}
	 * @memberof UmbDataTypeMoveServerDataSource
	 */
	async move(unique: string, targetUnique: string | null) {
		if (!unique) throw new Error('Unique is missing');
		if (targetUnique === undefined) throw new Error('Target unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.putDataTypeByIdMove({
				id: unique,
				requestBody: {
					target: targetUnique ? { id: targetUnique } : null,
				},
			}),
		);
	}
}
