import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbMoveToDataSource, UmbMoveToRequestArgs } from '@umbraco-cms/backoffice/repository';

/**
 * Move Data Type Server Data Source
 * @export
 * @class UmbMoveDataTypeServerDataSource
 */
export class UmbMoveDataTypeServerDataSource implements UmbMoveToDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMoveDataTypeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMoveDataTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Move an item for the given id to the target unique
	 * @param {string} unique
	 * @param {(string | null)} targetUnique
	 * @return {*}
	 * @memberof UmbMoveDataTypeServerDataSource
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
