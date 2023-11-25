import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/tree';
import { EntityTreeItemResponseModel, MediaResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Media tree that fetches data from the server
 * @export
 * @class UmbMediaTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbMediaTreeServerDataSource implements UmbTreeDataSource<EntityTreeItemResponseModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMediaTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbMediaTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, MediaResource.getTreeMediaRoot({}));
	}

	/**
	 * Fetches the children of a given parent id from the server
	 * @param {(string)} parentId
	 * @return {*}
	 * @memberof UmbMediaTreeServerDataSource
	 */
	async getChildrenOf(parentId: string | null): Promise<any> {
		/* TODO: should we make getRootItems() internal
		so it only is a server concern that there are two endpoints? */
		if (parentId === null) {
			return this.getRootItems();
		} else {
			return tryExecuteAndNotify(
				this.#host,
				MediaResource.getTreeMediaChildren({
					parentId,
				}),
			);
		}
	}

	// TODO: remove when interface is cleaned up
	async getItems(unique: Array<string>): Promise<any> {
		throw new Error('Dot not use this method. Use the item source instead');
	}
}
