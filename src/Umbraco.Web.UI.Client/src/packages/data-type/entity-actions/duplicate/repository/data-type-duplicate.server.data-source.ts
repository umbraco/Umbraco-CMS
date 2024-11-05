import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbDuplicateToDataSource, UmbDuplicateToRequestArgs } from '@umbraco-cms/backoffice/tree';

/**
 * Duplicate Document Server Data Source
 * @class UmbDuplicateDataTypeServerDataSource
 */
export class UmbDuplicateDataTypeServerDataSource implements UmbDuplicateToDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDuplicateDataTypeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDuplicateDataTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Duplicate an item for the given unique to the destination unique
	 * @param {UmbDuplicateToRequestArgs} args
	 * @returns {*}
	 * @memberof UmbDuplicateDataTypeServerDataSource
	 */
	async duplicateTo(args: UmbDuplicateToRequestArgs) {
		if (!args.unique) throw new Error('Unique is missing');
		if (args.destination.unique === undefined) throw new Error('Destination unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			DataTypeService.postDataTypeByIdCopy({
				id: args.unique,
				requestBody: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
		);
	}
}
