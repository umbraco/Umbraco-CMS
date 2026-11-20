import { documentItemCache } from '../../../item/repository/document-item.server.cache.js';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';
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
	 * @param {UmbMoveToRequestArgs} args - The move request arguments
	 * @returns {Promise<UmbDataSourceErrorResponse>} The result of the move request
	 * @memberof UmbMoveDocumentServerDataSource
	 */
	async moveTo(args: UmbMoveToRequestArgs): Promise<UmbDataSourceErrorResponse> {
		if (!args.unique) throw new Error('Unique is missing');
		if (args.destination.unique === undefined) throw new Error('Destination unique is missing');

		const previousParentUnique = documentItemCache.get(args.unique)?.parent?.id;

		const { error } = await tryExecute(
			this.#host,
			DocumentService.putDocumentByIdMove({
				path: { id: args.unique },
				body: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
		);

		if (!error) {
			// The server event that invalidates the cached item can arrive after the response. Both parents'
			// children have changed, though the previous parent is only known when the item was cached.
			documentItemCache.delete(args.unique);
			if (previousParentUnique) {
				documentItemCache.delete(previousParentUnique);
			}
			if (args.destination.unique) {
				documentItemCache.delete(args.destination.unique);
			}
		}

		return { error };
	}
}
