import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbMoveDataSource, UmbMoveToRequestArgs } from '@umbraco-cms/backoffice/tree';

/**
 * Move Data Type Folder Server Data Source
 * @class UmbMoveDataTypeFolderServerDataSource
 */
export class UmbMoveDataTypeFolderServerDataSource implements UmbMoveDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMoveDataTypeFolderServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMoveDataTypeFolderServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Move an item for the given id to the target unique
	 * @param {UmbMoveToRequestArgs} args - The move to request arguments
	 * @returns {Promise} The result of the move operation
	 * @memberof UmbMoveDataTypeFolderServerDataSource
	 */
	async moveTo(args: UmbMoveToRequestArgs) {
		if (!args.unique) throw new Error('Unique is missing');
		if (args.destination.unique === undefined) throw new Error('Destination unique is missing');

		return tryExecute(
			this.#host,
			DataTypeService.putDataTypeFolderByIdMove({
				path: { id: args.unique },
				body: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
		);
	}
}
