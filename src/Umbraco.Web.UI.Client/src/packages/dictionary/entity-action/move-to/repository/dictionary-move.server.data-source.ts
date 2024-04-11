import { DictionaryService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbMoveToDataSource, UmbMoveToRequestArgs } from '@umbraco-cms/backoffice/repository';

/**
 * Move Dictionary Server Data Source
 * @export
 * @class UmbDictionaryMoveServerDataSource
 */
export class UmbDictionaryMoveServerDataSource implements UmbMoveToDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDictionaryMoveServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDictionaryMoveServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Move an item for the given id to the target unique
	 * @param {string} unique
	 * @param {(string | null)} targetUnique
	 * @return {*}
	 * @memberof UmbDictionaryMoveServerDataSource
	 */
	async move(args: UmbMoveToRequestArgs) {
		if (!args.unique) throw new Error('Unique is missing');
		if (args.destination.unique === undefined) throw new Error('Destination unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			DictionaryService.putDictionaryByIdMove({
				id: args.unique,
				requestBody: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
		);
	}
}
