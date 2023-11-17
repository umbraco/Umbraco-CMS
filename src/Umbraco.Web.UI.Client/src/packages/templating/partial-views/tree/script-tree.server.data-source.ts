import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/tree';
import { FileSystemTreeItemPresentationModel, PartialViewResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the PartialView tree that fetches data from the server
 * @export
 * @class UmbPartialViewTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbPartialViewTreeServerDataSource implements UmbTreeDataSource<FileSystemTreeItemPresentationModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbPartialViewTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbPartialViewTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbPartialViewTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, PartialViewResource.getTreePartialViewRoot({}));
	}

	/**
	 * Fetches the children of a given parent path from the server
	 * @param {(string)} parentPath
	 * @return {*}
	 * @memberof UmbPartialViewTreeServerDataSource
	 */
	async getChildrenOf(parentPath: string | null) {
		/* TODO: should we make getRootItems() internal
		so it only is a server concern that there are two endpoints? */
		if (parentPath === null) {
			return this.getRootItems();
		} else {
			return tryExecuteAndNotify(
				this.#host,
				PartialViewResource.getTreePartialViewChildren({
					path: parentPath,
				}),
			);
		}
	}

	// TODO: remove when interface is cleaned up
	async getItems(unique: Array<string>): Promise<any> {
		throw new Error('Dot not use this method. Use the item source instead');
	}
}
