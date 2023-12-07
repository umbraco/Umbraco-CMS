import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/tree';
import { EntityTreeItemResponseModel, DocumentResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document tree that fetches data from the server
 * @export
 * @class UmbDocumentTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbDocumentTreeServerDataSource implements UmbTreeDataSource<EntityTreeItemResponseModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbDocumentTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, DocumentResource.getTreeDocumentRoot({}));
	}

	/**
	 * Fetches the children of a given parent id from the server
	 * @param {(string)} parentId
	 * @return {*}
	 * @memberof UmbDocumentTreeServerDataSource
	 */
	async getChildrenOf(parentId: string | null) {
		/* TODO: should we make getRootItems() internal
		so it only is a server concern that there are two endpoints? */
		if (parentId === null) {
			return this.getRootItems();
		} else {
			return tryExecuteAndNotify(
				this.#host,
				DocumentResource.getTreeDocumentChildren({
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
