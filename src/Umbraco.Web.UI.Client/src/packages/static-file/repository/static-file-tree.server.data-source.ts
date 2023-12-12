import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/tree';
import { type StaticFileItemResponseModel, StaticFileResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Static File tree that fetches data from the server
 * @export
 * @class UmbStaticFileTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbStaticFileTreeServerDataSource implements UmbTreeDataSource<StaticFileItemResponseModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbStaticFileTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbStaticFileTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbStaticFileTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, StaticFileResource.getTreeStaticFileRoot({}));
	}

	/**
	 * Fetches the children of a given parent id from the server
	 * @param {(string)} parentId
	 * @return {*}
	 * @memberof UmbStaticFileTreeServerDataSource
	 */
	async getChildrenOf(path: string | null): Promise<any> {
		/* TODO: should we make getRootItems() internal
		so it only is a server concern that there are two endpoints? */
		if (path === null) {
			return this.getRootItems();
		} else {
			return tryExecuteAndNotify(
				this.#host,
				StaticFileResource.getTreeStaticFileChildren({
					path,
				}),
			);
		}
	}

	// TODO: remove when interface is cleaned up
	async getItems(unique: Array<string>): Promise<any> {
		throw new Error('Dot not use this method. Use the item source instead');
	}
}
