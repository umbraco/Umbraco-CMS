import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/tree';
import { DictionaryResource, EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Dictionary tree that fetches data from the server
 * @export
 * @class UmbDictionaryTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbDictionaryTreeServerDataSource implements UmbTreeDataSource<EntityTreeItemResponseModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDictionaryTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDictionaryTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbDictionaryTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, DictionaryResource.getTreeDictionaryRoot({}));
	}

	/**
	 * Fetches the children of a given parent id from the server
	 * @param {(string)} parentId
	 * @return {*}
	 * @memberof UmbDictionaryTreeServerDataSource
	 */
	async getChildrenOf(parentId: string | null) {
		if (parentId === undefined) throw new Error('Parent id is missing');

		/* TODO: should we make getRootItems() internal
		so it only is a server concern that there are two endpoints? */
		if (parentId === null) {
			return this.getRootItems();
		} else {
			return tryExecuteAndNotify(
				this.#host,
				DictionaryResource.getTreeDictionaryChildren({
					parentId,
				}),
			);
		}
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbDictionaryTreeServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids) throw new Error('Ids are missing');
		return tryExecuteAndNotify(
			this.#host,
			DictionaryResource.getDictionaryItem({
				id: ids,
			}),
		);
	}
}
