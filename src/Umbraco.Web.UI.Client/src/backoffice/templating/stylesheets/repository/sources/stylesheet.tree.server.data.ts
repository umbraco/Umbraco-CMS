import {
	FileSystemTreeItemPresentationModel,
	PagedFileSystemTreeItemPresentationModel,
	StylesheetResource,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for the Stylesheet tree that fetches data from the server
 * @export
 * @class UmbStylesheetTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbStylesheetTreeServerDataSource
	implements UmbTreeDataSource<PagedFileSystemTreeItemPresentationModel, FileSystemTreeItemPresentationModel>
{
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
	 * @param {(string | undefined)} path
	 * @return {*}
	 * @memberof UmbStylesheetTreeServerDataSource
	 */
	async getChildrenOf(path: string | undefined) {
		return tryExecuteAndNotify(
			this.#host,
			StylesheetResource.getTreeStylesheetChildren({
				path,
			})
		);
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
			StylesheetResource.getTreeStylesheetItem({
				path,
			})
		);
	}
}
