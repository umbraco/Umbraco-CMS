import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/tree';
import { EntityTreeItemResponseModel, MemberGroupResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the MemberGroup tree that fetches data from the server
 * @export
 * @class UmbMemberGroupTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbMemberGroupTreeServerDataSource implements UmbTreeDataSource<EntityTreeItemResponseModel> {
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
	 * @param {(string)} parentId
	 * @return {*}
	 * @memberof UmbMemberGroupTreeServerDataSource
	 */
	async getChildrenOf(parentId: string | null): Promise<any> {
		/* TODO: should we make getRootItems() internal
		so it only is a server concern that there are two endpoints? */
		if (parentId === null) {
			return this.getRootItems();
		} else {
			alert('not implemented');
			/*
			return tryExecuteAndNotify(
				this.#host,
				MemberGroupResource.getTreeMemberGroupChildren({
					parentId,
				}),
			);
			*/
		}
	}

	// TODO: remove when interface is cleaned up
	async getItems(unique: Array<string>): Promise<any> {
		throw new Error('Dot not use this method. Use the item source instead');
	}
}
