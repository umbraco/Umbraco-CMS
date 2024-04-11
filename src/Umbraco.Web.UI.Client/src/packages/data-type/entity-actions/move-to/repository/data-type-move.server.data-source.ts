import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbMoveToDataSource, UmbMoveToRequestArgs } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for Data Type items that fetches data from the server
 * @export
 * @class UmbDataTypeMoveServerDataSource
 */
export class UmbDataTypeMoveServerDataSource implements UmbMoveToDataSource {
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
	async move(args: UmbMoveToRequestArgs) {
		if (!args.unique) throw new Error('Unique is missing');
		if (args.destination.unique === undefined) throw new Error('Destination unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			DataTypeService.putDataTypeByIdMove({
				id: args.unique,
				requestBody: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
		);
	}
}
