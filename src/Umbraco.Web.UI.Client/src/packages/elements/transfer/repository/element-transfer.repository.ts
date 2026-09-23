import type { UmbElementTransferFromBlockRequestArgs } from './types.js';
import { UmbElementTransferServerDataSource } from './element-transfer.server.data-source.js';
import { UmbRepositoryBase, type UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export class UmbElementTransferRepository extends UmbRepositoryBase {
	#transferDataSource = new UmbElementTransferServerDataSource(this);

	/**
	 * Transfers a block held in a content item's property into the Element Library, reproducing the cultures
	 * it is currently live in.
	 * @param {UmbElementTransferFromBlockRequestArgs} args - Which block to transfer, and where to put it
	 * @returns {Promise<UmbDataSourceResponse<string>>} The unique of the new element
	 * @memberof UmbElementTransferRepository
	 */
	async transferFromBlock(args: UmbElementTransferFromBlockRequestArgs): Promise<UmbDataSourceResponse<string>> {
		if (!args.owner) throw new Error('Owner is missing');
		if (!args.block) throw new Error('Block is missing');
		if (!args.name) throw new Error('Name is missing');

		return this.#transferDataSource.transferFromBlock(args);
	}
}

export { UmbElementTransferRepository as api };
