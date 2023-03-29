import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';
import { ProblemDetailsModel, DataTypeResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document tree that fetches data from the server
 * @export
 * @class DocumentTreeServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class DataTypeTreeServerDataSource implements UmbTreeDataSource {
	#host: UmbControllerHostElement;

	// TODO: how do we handle trashed items?
	async trashItems(keys: Array<string>) {
		if (!keys) {
			const error: ProblemDetailsModel = { title: 'DataType keys is missing' };
			return { error };
		}

		// TODO: use resources when end point is ready:
		/*
		return tryExecuteAndNotify<DataType>(
			this.#host,
			DataTypeResource.deleteDataTypeByKey({
				key: keys,
			})
		);
		*/
		return Promise.resolve({ error: null, data: null });
	}

	async moveItems(keys: Array<string>, destination: string) {
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch('/umbraco/management/api/v1/data-type/move', {
				method: 'POST',
				body: JSON.stringify({ keys, destination }),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		);
	}

	/**
	 * Creates an instance of DocumentTreeServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof DocumentTreeServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof DocumentTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, DataTypeResource.getTreeDataTypeRoot({}));
	}

	/**
	 * Fetches the children of a given parent key from the server
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof DocumentTreeServerDataSource
	 */
	async getChildrenOf(parentKey: string | null) {
		if (!parentKey) {
			const error: ProblemDetailsModel = { title: 'Parent key is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.getTreeDataTypeChildren({
				parentKey,
			})
		);
	}

	/**
	 * Fetches the items for the given keys from the server
	 * @param {Array<string>} keys
	 * @return {*}
	 * @memberof DocumentTreeServerDataSource
	 */
	async getItems(keys: Array<string>) {
		if (keys) {
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.getTreeDataTypeItem({
				key: keys,
			})
		);
	}
}
