import { MediaService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbMoveDataSource, UmbMoveToRequestArgs } from '@umbraco-cms/backoffice/tree';

/**
 * Move Media Server Data Source
 * @class UmbMoveMediaServerDataSource
 */
export class UmbMoveMediaServerDataSource implements UmbMoveDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMoveMediaServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMoveMediaServerDataSource
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
	 * @memberof UmbMoveMediaServerDataSource
	 */
	async moveTo(args: UmbMoveToRequestArgs) {
		if (!args.unique) throw new Error('Unique is missing');
		if (args.destination.unique === undefined) throw new Error('Destination unique is missing');

		return tryExecute(
			this.#host,
			MediaService.putMediaByIdMove({
				id: args.unique,
				requestBody: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
		);
	}
}
