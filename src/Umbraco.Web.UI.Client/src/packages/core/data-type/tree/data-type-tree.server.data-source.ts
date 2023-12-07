import { UmbDataTypeTreeItemModel } from './types.js';
import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/tree';
import { DataTypeResource, DataTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document tree that fetches data from the server
 * @export
 * @class UmbDataTypeTreeServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDataTypeTreeServerDataSource implements UmbTreeDataSource<UmbDataTypeTreeItemModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDataTypeTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDataTypeTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbDataTypeTreeServerDataSource
	 */
	async getRootItems() {
		const { data, error } = await tryExecuteAndNotify(this.#host, DataTypeResource.getTreeDataTypeRoot({}));

		if (data) {
			const items = data?.items.map((item) => mapper(item));
			return { data: { total: data.total, items } };
		}

		return { error };
	}

	/**
	 * Fetches the children of a given parent unique from the server
	 * @param {(string)} parentUnique
	 * @return {*}
	 * @memberof UmbDataTypeTreeServerDataSource
	 */
	async getChildrenOf(parentUnique: string | null) {
		if (parentUnique === undefined) throw new Error('Parent unique is missing');

		/* TODO: should we make getRootItems() internal
		so it only is a server concern that there are two endpoints? */

		if (parentUnique === null) {
			return this.getRootItems();
		}

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			DataTypeResource.getTreeDataTypeChildren({
				parentId: parentUnique,
			}),
		);

		if (data) {
			const items = data?.items.map((item) => mapper(item));
			return { data: { total: data.total, items } };
		}

		return { error };
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} uniques
	 * @return {*}
	 * @memberof UmbDataTypeTreeServerDataSource
	 */
	async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');
		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.getDataTypeItem({
				id: uniques,
			}),
		);
	}
}

const mapper = (item: DataTypeTreeItemResponseModel): UmbDataTypeTreeItemModel => {
	return {
		unique: item.id!,
		parentUnique: item.parentId || null,
		name: item.name!,
		type: item.isFolder ? 'data-type-folder' : 'data-type',
		isFolder: item.isFolder!,
		isContainer: item.isContainer!,
		hasChildren: item.hasChildren!,
	};
};
