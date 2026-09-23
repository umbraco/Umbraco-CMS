import type { UmbElementTransferFromBlockRequestArgs } from './types.js';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { ElementService } from '@umbraco-cms/backoffice/external/backend-api';
import type { CreateElementFromBlockRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * A server data source for transferring a block into the Element Library
 * @class UmbElementTransferServerDataSource
 */
export class UmbElementTransferServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbElementTransferServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbElementTransferServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Transfers a block held in a content item's property into the Element Library
	 * @param {UmbElementTransferFromBlockRequestArgs} args - Which block to transfer, and where to put it
	 * @returns {Promise<UmbDataSourceResponse<string>>} The unique of the new element
	 * @memberof UmbElementTransferServerDataSource
	 */
	async transferFromBlock(args: UmbElementTransferFromBlockRequestArgs): Promise<UmbDataSourceResponse<string>> {
		const body: CreateElementFromBlockRequestModel = {
			owner: { id: args.owner },
			block: { id: args.block },
			parent: args.parent ? { id: args.parent } : null,
			name: args.name,
		};

		// the new element's unique is the server's to assign. It comes back as the response body, which the
		// Umb-Generated-Resource interceptor fills in from the header a 201 Created carries.
		const { data, error } = await tryExecute(this.#host, ElementService.postElementFromBlock({ body }));

		return error ? { error } : { data: data as string };
	}
}
