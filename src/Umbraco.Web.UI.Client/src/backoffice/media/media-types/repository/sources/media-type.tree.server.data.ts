import { MediaTypeResource, ProblemDetailsModel } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { RepositoryTreeDataSource } from '@umbraco-cms/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

/**
 * A data source for the MediaType tree that fetches data from the server
 * @export
 * @class MediaTypeTreeServerDataSource
 * @implements {MediaTypeTreeDataSource}
 */
export class MediaTypeTreeServerDataSource implements RepositoryTreeDataSource {
	#host: UmbControllerHostInterface;

	/**
	 * Creates an instance of MediaTypeTreeDataSource.
	 * @param {UmbControllerHostInterface} host
	 * @memberof MediaTypeTreeDataSource
	 */
	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof MediaTypeTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, MediaTypeResource.getTreeMediaTypeRoot({}));
	}

	/**
	 * Fetches the children of a given parent key from the server
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof MediaTypeTreeServerDataSource
	 */
	async getChildrenOf(parentKey: string | null) {
		if (!parentKey) {
			const error: ProblemDetailsModel = { title: 'Parent key is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.getTreeMediaTypeChildren({
				parentKey,
			})
		);
	}

	/**
	 * Fetches the items for the given keys from the server
	 * @param {Array<string>} keys
	 * @return {*}
	 * @memberof MediaTypeTreeServerDataSource
	 */
	async getItems(keys: Array<string>) {
		if (!keys || keys.length === 0) {
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.getTreeMediaTypeItem({
				key: keys,
			})
		);
	}
}
