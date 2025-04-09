import { DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDuplicateToDataSource, UmbDuplicateToRequestArgs } from '@umbraco-cms/backoffice/tree';

/**
 * Duplicate Document Server Data Source
 * @class UmbDuplicateDocumentTypeServerDataSource
 */
export class UmbDuplicateDocumentTypeServerDataSource implements UmbDuplicateToDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDuplicateDocumentTypeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDuplicateDocumentTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Duplicate an item for the given id to the destination unique
	 * @param {UmbDuplicateToRequestArgs} args
	 * @returns {*}
	 * @memberof UmbDuplicateDocumentTypeServerDataSource
	 */
	async duplicateTo(args: UmbDuplicateToRequestArgs) {
		if (!args.unique) throw new Error('Unique is missing');
		if (args.destination.unique === undefined) throw new Error('Destination unique is missing');

		return tryExecute(
			this.#host,
			DocumentTypeService.postDocumentTypeByIdCopy({
				id: args.unique,
				body: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
		);
	}
}
