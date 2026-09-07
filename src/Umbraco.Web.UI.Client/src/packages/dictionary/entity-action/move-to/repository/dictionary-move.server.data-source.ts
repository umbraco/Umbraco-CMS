import { DictionaryService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbMoveDataSource, UmbMoveToRequestArgs } from '@umbraco-cms/backoffice/tree';
import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

/**
 * Move Dictionary Server Data Source
 * @class UmbMoveDictionaryServerDataSource
 */
export class UmbMoveDictionaryServerDataSource implements UmbMoveDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMoveDictionaryServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMoveDictionaryServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Move an item for the given id to the target unique
	 * @param {UmbMoveToRequestArgs} args - The arguments for the move operation.
	 * @returns {Promise<UmbDataSourceErrorResponse>} Undefined if the operation succeeded, otherwise an error.
	 * @memberof UmbMoveDictionaryServerDataSource
	 */
	async moveTo(args: UmbMoveToRequestArgs): Promise<UmbDataSourceErrorResponse> {
		if (!args.unique) throw new Error('Unique is missing');
		if (args.destination.unique === undefined) throw new Error('Destination unique is missing');

		return tryExecute(
			this.#host,
			DictionaryService.putDictionaryByIdMove({
				path: { id: args.unique },
				body: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
		);
	}
}
