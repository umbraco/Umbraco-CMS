import { MediaTypeResource, ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the MediaType tree that fetches data from the server
 * @export
 * @class UmbMediaTypeTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbMediaTypeTreeServerDataSource implements UmbTreeDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of MediaTypeTreeDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof MediaTypeTreeDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbMediaTypeTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, MediaTypeResource.getTreeMediaTypeRoot({}));
	}

	/**
	 * Fetches the children of a given parent id from the server
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbMediaTypeTreeServerDataSource
	 */
	async getChildrenOf(parentId: string | null) {
		if (!parentId) {
			const error: ProblemDetailsModel = { title: 'Parent id is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.getTreeMediaTypeChildren({
				parentId,
			})
		);
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbMediaTypeTreeServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids || ids.length === 0) {
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.getMediaTypeItem({
				id: ids,
			})
		);
	}
}
