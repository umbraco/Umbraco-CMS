import { MemberTypeResource, ProblemDetailsModel } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { RepositoryTreeDataSource } from '@umbraco-cms/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

/**
 * A data source for the MemberType tree that fetches data from the server
 * @export
 * @class MemberTypeTreeServerDataSource
 * @implements {MemberTypeTreeDataSource}
 */
export class MemberTypeTreeServerDataSource implements RepositoryTreeDataSource {
	#host: UmbControllerHostInterface;

	/**
	 * Creates an instance of MemberTypeTreeDataSource.
	 * @param {UmbControllerHostInterface} host
	 * @memberof MemberTypeTreeDataSource
	 */
	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof MemberTypeTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, MemberTypeResource.getTreeMemberTypeRoot({}));
	}

	/**
	 * Fetches the children of a given parent key from the server
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof MemberTypeTreeServerDataSource
	 */
	async getChildrenOf(parentKey: string | null) {
		const error: ProblemDetailsModel = { title: 'Not implemented for Member Type' };
		return { error };
	}

	/**
	 * Fetches the items for the given keys from the server
	 * @param {Array<string>} keys
	 * @return {*}
	 * @memberof MemberTypeTreeServerDataSource
	 */
	async getItems(keys: Array<string>) {
		if (!keys || keys.length === 0) {
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			MemberTypeResource.getTreeMemberTypeItem({
				key: keys,
			})
		);
	}
}
