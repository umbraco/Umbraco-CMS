import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbMoveDataSource, UmbMoveToRequestArgs } from '@umbraco-cms/backoffice/tree';

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
	 * @param {string} unique
	 * @param {(string | null)} targetUnique
	 * @param args
	 * @returns {*}
	 * @memberof UmbMoveDataTypeServerDataSource
	 */
	async moveTo(args: UmbMoveToRequestArgs) {
		if (!args.unique) throw new Error('Unique is missing');
		if (args.destination.unique === undefined) throw new Error('Destination unique is missing');

		return tryExecute(
			this.#host,
			DataTypeService.putDataTypeByIdMove({
				id: args.unique,
				body: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
		);
	}
}
