import { FileSystemTreeItemPresentationModel, StylesheetResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for the Stylesheet tree that fetches data from the server
 * @export
 * @class UmbStylesheetTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbStylesheetTreeServerDataSource implements UmbTreeDataSource<FileSystemTreeItemPresentationModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbStylesheetTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbStylesheetTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the children of a given stylesheet path from the server
	 * @param {(string | null)} path
	 * @return {*}
	 * @memberof UmbStylesheetTreeServerDataSource
	 */
	async getChildrenOf(path: string | null) {
		if (path === undefined) throw new Error('Path is missing');

		if (path === null) {
			return tryExecuteAndNotify(this.#host, StylesheetResource.getTreeStylesheetRoot({}));
		} else {
			return tryExecuteAndNotify(
				this.#host,
				StylesheetResource.getTreeStylesheetChildren({
					path,
				}),
			);
		}
	}
}
