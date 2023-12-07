import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/tree';
import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Member tree that fetches data from the server
 * @export
 * @class UmbMemberTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbMemberTreeServerDataSource implements UmbTreeDataSource<EntityTreeItemResponseModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbMemberTreeServerDataSource
	 */
	async getRootItems() {
		alert('not implemented');
		//return tryExecuteAndNotify(this.#host, MemberResource.getTreeMemberRoot({}));
	}

	/**
	 * Fetches the children of a given parent id from the server
	 * @param {(string)} parentId
	 * @return {*}
	 * @memberof UmbMemberTreeServerDataSource
	 */
	async getChildrenOf(parentId: string | null) {
		alert('not implemented');
		/* TODO: should we make getRootItems() internal
		so it only is a server concern that there are two endpoints? */
		/*
		if (parentId === null) {
			return this.getRootItems();
		} else {
			return tryExecuteAndNotify(
				this.#host,
				MemberResource.getTreeMemberChildren({
					parentId,
				}),
			);
		}
		*/
	}

	// TODO: remove when interface is cleaned up
	async getItems(unique: Array<string>): Promise<any> {
		throw new Error('Dot not use this method. Use the item source instead');
	}
}
