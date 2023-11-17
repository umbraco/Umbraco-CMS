import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/tree';
import { FileSystemTreeItemPresentationModel, ScriptResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Script tree that fetches data from the server
 * @export
 * @class UmbScriptTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbScriptTreeServerDataSource implements UmbTreeDataSource<FileSystemTreeItemPresentationModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbScriptTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbScriptTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbScriptTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, ScriptResource.getTreeScriptRoot({}));
	}

	/**
	 * Fetches the children of a given parent path from the server
	 * @param {(string)} parentPath
	 * @return {*}
	 * @memberof UmbScriptTreeServerDataSource
	 */
	async getChildrenOf(parentPath: string | null) {
		/* TODO: should we make getRootItems() internal
		so it only is a server concern that there are two endpoints? */
		if (parentPath === null) {
			return this.getRootItems();
		} else {
			return tryExecuteAndNotify(
				this.#host,
				ScriptResource.getTreeScriptChildren({
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
