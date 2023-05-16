import { FileSystemTreeItemPresentationModel, StylesheetResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for the Stylesheet tree that fetches data from the server
 * @export
 * @class UmbStylesheetTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbStylesheetTreeServerDataSource implements UmbTreeDataSource<FileSystemTreeItemPresentationModel> {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbStylesheetTreeServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbStylesheetTreeServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the stylesheet tree root items from the server
	 * @return {*}
	 * @memberof UmbStylesheetTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, StylesheetResource.getTreeStylesheetRoot({}));
	}

	/**
	 * Fetches the children of a given stylesheet path from the server
	 * @param {(string | null)} path
	 * @return {*}
	 * @memberof UmbStylesheetTreeServerDataSource
	 */
	async getChildrenOf(path: string | null) {
		if (path === undefined) throw new Error('Path is missing');

		/* TODO: should we make getRootItems() internal 
		so it only is a server concern that there are two endpoints? */
		if (path === null) {
			return this.getRootItems();
		} else {
			return tryExecuteAndNotify(
				this.#host,
				StylesheetResource.getTreeStylesheetChildren({
					path,
				})
			);
		}
	}

	/**
	 * Fetches stylesheet items from the server
	 * @param {(string | undefined)} path
	 * @return {*}
	 * @memberof UmbStylesheetTreeServerDataSource
	 */
	async getItems(path: Array<string>) {
		return tryExecuteAndNotify(
			this.#host,
			StylesheetResource.getStylesheetItem({
				path,
			})
		);
	}
}
