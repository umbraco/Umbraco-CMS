import { UmbMediaTypeTreeItemModel } from './types.js';
import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/tree';
import { MediaTypeResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Media Type tree that fetches data from the server
 * @export
 * @class UmbMediaTypeTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbMediaTypeTreeServerDataSource implements UmbTreeDataSource<UmbMediaTypeTreeItemModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMediaTypeTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaTypeTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbMediaTypeTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, MediaTypeResource.getTreeMediaTypeRoot({}));
	}

	/**
	 * Fetches the children of a given parent id from the server
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbMediaTypeTreeServerDataSource
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
				MediaTypeResource.getTreeMediaTypeChildren({
					parentId,
				}),
			);
		}
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbMediaTypeTreeServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (ids) {
			throw new Error('Ids are missing');
		}

		return tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.getMediaTypeItem({
				id: ids,
			}),
		);
	}
}
