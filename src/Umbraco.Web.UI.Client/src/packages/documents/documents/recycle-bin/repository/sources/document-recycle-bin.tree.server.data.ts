import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';
import { DocumentResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document Recycle Bin tree that fetches data from the server
 * @export
 * @class UmbDocumentRecycleBinTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbDocumentRecycleBinTreeServerDataSource implements UmbTreeDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbDocumentRecycleBinTreeServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentTreeServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @memberof UmbDocumentRecycleBinTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, DocumentResource.getRecycleBinDocumentRoot({}));
	}

	/**
	 * Fetches the children of a given parent id from the server
	 * @param {(string | null)} parentId
	 * @memberof UmbDocumentTreeServerDataSource
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
				DocumentResource.getRecycleBinDocumentChildren({
					parentId,
				}),
			);
		}
	}
}
