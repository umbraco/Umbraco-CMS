import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbMoveDataSource, UmbMoveToRequestArgs } from '@umbraco-cms/backoffice/tree';

/**
 * Move Document Server Data Source
 * @class UmbMoveDocumentServerDataSource
 */
export class UmbMoveDocumentServerDataSource implements UmbMoveDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMoveDocumentServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMoveDocumentServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Move an item for the given id to the target unique
	 * @param {string} unique - The unique identifier of the item to move
	 * @param {(string | null)} targetUnique - The unique identifier of the target destination
	 * @param {UmbMoveToRequestArgs} args - The move request arguments
	 * @returns {*} The result of the move request
	 * @memberof UmbMoveDocumentServerDataSource
	 */
	async moveTo(args: UmbMoveToRequestArgs) {
		if (!args.unique) throw new Error('Unique is missing');
		if (args.destination.unique === undefined) throw new Error('Destination unique is missing');

		return tryExecute(
			this.#host,
			DocumentService.putDocumentByIdMove({
				path: { id: args.unique },
				body: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
			{ disableNotifications: true },
		);
	}
}
