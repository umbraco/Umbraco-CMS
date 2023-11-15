import { MemberGroupResource } from '@umbraco-cms/backoffice/backend-api';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { type UmbTreeDataSource } from '@umbraco-cms/backoffice/tree';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Member Group tree that fetches data from the server
 * @export
 * @class UmbMemberGroupTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbMemberGroupTreeServerDataSource implements UmbTreeDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberGroupTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberGroupTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbMemberGroupTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, MemberGroupResource.getTreeMemberGroupRoot({}));
	}

	/**
	 * Fetches the children of a given parent id from the server
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbMemberGroupTreeServerDataSource
	 */
	async getChildrenOf(parentId: string | null) {
		// Not implemented for this tree
		return {};
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbMemberGroupTreeServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids || ids.length === 0) {
			throw new Error('Ids are missing');
		}

		return tryExecuteAndNotify(
			this.#host,
			MemberGroupResource.getMemberGroupItem({
				id: ids,
			}),
		);
	}
}
