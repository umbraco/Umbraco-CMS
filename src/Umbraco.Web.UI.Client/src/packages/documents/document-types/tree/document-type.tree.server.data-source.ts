import { UmbDocumentTypeTreeItemModel } from './types.js';
import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/tree';
import { DocumentTypeResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document Type tree that fetches data from the server
 * @export
 * @class UmbDocumentTypeTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbDocumentTypeTreeServerDataSource implements UmbTreeDataSource<UmbDocumentTypeTreeItemModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentTypeTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentTypeTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbDocumentTypeTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, DocumentTypeResource.getTreeDocumentTypeRoot({}));
	}

	/**
	 * Fetches the children of a given parent id from the server
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbDocumentTypeTreeServerDataSource
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
				DocumentTypeResource.getTreeDocumentTypeChildren({
					parentId,
				}),
			);
		}
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbDocumentTypeTreeServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (ids) {
			throw new Error('Ids are missing');
		}

		return tryExecuteAndNotify(
			this.#host,
			DocumentTypeResource.getDocumentTypeItem({
				id: ids,
			}),
		);
	}
}
