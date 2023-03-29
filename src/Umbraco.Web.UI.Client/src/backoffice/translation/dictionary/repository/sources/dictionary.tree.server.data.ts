import { DictionaryResource, ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Dictionary tree that fetches data from the server
 * @export
 * @class DictionaryTreeServerDataSource
 * @implements {DictionaryTreeDataSource}
 */
export class DictionaryTreeServerDataSource implements UmbTreeDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of DictionaryTreeDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof DictionaryTreeDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof DictionaryTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, DictionaryResource.getTreeDictionaryRoot({}));
	}

	/**
	 * Fetches the children of a given parent key from the server
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof DictionaryTreeServerDataSource
	 */
	async getChildrenOf(parentKey: string | null) {
		if (!parentKey) {
			const error: ProblemDetailsModel = { title: 'Parent key is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			DictionaryResource.getTreeDictionaryChildren({
				parentKey,
			})
		);
	}

	/**
	 * Fetches the items for the given keys from the server
	 * @param {Array<string>} keys
	 * @return {*}
	 * @memberof DictionaryTreeServerDataSource
	 */
	async getItems(keys: Array<string>) {
		if (!keys || keys.length === 0) {
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			DictionaryResource.getTreeDictionaryItem({
				key: keys,
			})
		);
	}
}
