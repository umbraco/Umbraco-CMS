import type { UmbDuplicateDocumentRequestArgs } from './types.js';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * Duplicate Document Server Data Source
 * @class UmbDuplicateDocumentServerDataSource
 */
export class UmbDuplicateDocumentServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDuplicateDocumentServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDuplicateDocumentServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Duplicate an item for the given id to the destination unique
	 * @param {UmbDuplicateDocumentRequestArgs} args
	 * @returns {*}
	 * @memberof UmbDuplicateDocumentServerDataSource
	 */
	async duplicate(args: UmbDuplicateDocumentRequestArgs) {
		if (!args.unique) throw new Error('Unique is missing');
		if (args.destination.unique === undefined) throw new Error('Destination unique is missing');

		return tryExecute(
			this.#host,
			DocumentService.postDocumentByIdCopy({
				path: { id: args.unique },
				body: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
					relateToOriginal: args.relateToOriginal,
					includeDescendants: args.includeDescendants,
				},
			}),
		);
	}
}
