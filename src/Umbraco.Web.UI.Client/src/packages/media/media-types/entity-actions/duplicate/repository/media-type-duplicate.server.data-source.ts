import { MediaTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDuplicateToDataSource, UmbDuplicateToRequestArgs } from '@umbraco-cms/backoffice/tree';

/**
 * Duplicate Document Server Data Source
 * @class UmbDuplicateMediaTypeServerDataSource
 */
export class UmbDuplicateMediaTypeServerDataSource implements UmbDuplicateToDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDuplicateMediaTypeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDuplicateMediaTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Duplicate an item for the given unique to the destination unique
	 * @param {UmbDuplicateToRequestArgs} args
	 * @returns {*}
	 * @memberof UmbDuplicateMediaTypeServerDataSource
	 */
	async duplicateTo(args: UmbDuplicateToRequestArgs) {
		if (!args.unique) throw new Error('Unique is missing');
		if (args.destination.unique === undefined) throw new Error('Destination unique is missing');

		return tryExecute(
			this.#host,
			MediaTypeService.postMediaTypeByIdCopy({
				id: args.unique,
				body: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
		);
	}
}
