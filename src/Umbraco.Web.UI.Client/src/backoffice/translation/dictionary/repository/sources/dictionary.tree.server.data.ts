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
	 * Fetches the children of a given parent id from the server
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof DictionaryTreeServerDataSource
	 */
	async getChildrenOf(parentId: string | null) {
		if (!parentId) {
			const error: ProblemDetailsModel = { title: 'Parent id is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			DictionaryResource.getTreeDictionaryChildren({
				parentId,
			})
		);
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof DictionaryTreeServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids || ids.length === 0) {
			const error: ProblemDetailsModel = { title: 'Ids are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			DictionaryResource.getDictionaryItem({
				id: ids,
			})
		);
	}
}
