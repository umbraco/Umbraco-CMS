import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/tree';
import { EntityTreeItemResponseModel, RelationTypeResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Relation Type tree that fetches data from the server
 * @export
 * @class UmbRelationTypeTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbRelationTypeTreeServerDataSource implements UmbTreeDataSource<EntityTreeItemResponseModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbRelationTypeTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbRelationTypeTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbRelationTypeTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, RelationTypeResource.getTreeRelationTypeRoot({}));
	}

	/**
	 * Fetches the children of a given parent id from the server
	 * @param {(string)} parentId
	 * @return {*}
	 * @memberof UmbRelationTypeTreeServerDataSource
	 */
	async getChildrenOf(parentId: string | null) {
		/* TODO: should we make getRootItems() internal
		so it only is a server concern that there are two endpoints? */
		if (parentId === null) {
			return this.getRootItems();
		} else {
			throw new Error('Not supported for the relation type tree');
		}
	}

	// TODO: remove when interface is cleaned up
	async getItems(unique: Array<string>): Promise<any> {
		throw new Error('Dot not use this method. Use the item source instead');
	}
}
