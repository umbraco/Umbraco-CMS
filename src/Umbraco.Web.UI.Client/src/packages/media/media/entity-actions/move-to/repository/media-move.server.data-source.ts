import { mediaItemCache } from '../../../repository/item/media-item.server.cache.js';
import { MediaService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbMoveDataSource, UmbMoveToRequestArgs } from '@umbraco-cms/backoffice/tree';
import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

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
	 * @param {UmbMoveToRequestArgs} args - The move request arguments
	 * @returns {UmbDataSourceErrorResponse} A promise that resolves once the media has been moved
	 * @memberof UmbMoveMediaServerDataSource
	 */
	async moveTo(args: UmbMoveToRequestArgs): Promise<UmbDataSourceErrorResponse> {
		if (!args.unique) throw new Error('Unique is missing');
		if (args.destination.unique === undefined) throw new Error('Destination unique is missing');

		const previousParentUnique = mediaItemCache.get(args.unique)?.parent?.id;

		const { error } = await tryExecute(
			this.#host,
			MediaService.putMediaByIdMove({
				path: { id: args.unique },
				body: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
		);

		if (!error) {
			// The server event that invalidates the cached item can arrive after the response. Both parents'
			// children have changed, though the previous parent is only known when the item was cached.
			mediaItemCache.delete(args.unique);
			if (previousParentUnique) {
				mediaItemCache.delete(previousParentUnique);
			}
			if (args.destination.unique) {
				mediaItemCache.delete(args.destination.unique);
			}
		}

		return { error };
	}
}
