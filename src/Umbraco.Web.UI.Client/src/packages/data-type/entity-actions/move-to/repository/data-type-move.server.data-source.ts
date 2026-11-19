import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbMoveDataSource, UmbMoveToRequestArgs } from '@umbraco-cms/backoffice/tree';
import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

/**
 * Move Data Type Server Data Source
 * @class UmbMoveDataTypeServerDataSource
 */
export class UmbMoveDataTypeServerDataSource implements UmbMoveDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMoveDataTypeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMoveDataTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Move an item for the given id to the target unique
	 * @param {UmbMoveToRequestArgs} args - The unique and destination to move to.
	 * @returns {Promise<UmbDataSourceErrorResponse>} The result of the move request.
	 * @memberof UmbMoveDataTypeServerDataSource
	 */
	async moveTo(args: UmbMoveToRequestArgs): Promise<UmbDataSourceErrorResponse> {
		if (!args.unique) throw new Error('Unique is missing');
		if (args.destination.unique === undefined) throw new Error('Destination unique is missing');

		return tryExecute(
			this.#host,
			DataTypeService.putDataTypeByIdMove({
				path: { id: args.unique },
				body: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
		);
	}
}
