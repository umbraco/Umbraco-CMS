import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';
import { DataTypeResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document tree that fetches data from the server
 * @export
 * @class UmbDataTypeTreeServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDataTypeTreeServerDataSource implements UmbTreeDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbDataTypeTreeServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDataTypeTreeServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbDataTypeTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, DataTypeResource.getTreeDataTypeRoot({}));
	}

	/**
	 * Fetches the children of a given parent id from the server
	 * @param {(string)} parentId
	 * @return {*}
	 * @memberof UmbDataTypeTreeServerDataSource
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
				DataTypeResource.getTreeDataTypeChildren({
					parentId,
				})
			);
		}
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbDataTypeTreeServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids) throw new Error('Ids are missing');
		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.getDataTypeItem({
				id: ids,
			})
		);
	}
}
