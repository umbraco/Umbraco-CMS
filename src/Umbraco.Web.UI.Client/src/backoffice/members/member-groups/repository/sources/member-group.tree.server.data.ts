import { MemberGroupResource, ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Member Group tree that fetches data from the server
 * @export
 * @class MemberGroupTreeServerDataSource
 * @implements {MemberGroupTreeDataSource}
 */
export class MemberGroupTreeServerDataSource implements UmbTreeDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of MemberGroupTreeServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof MemberGroupTreeServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof MemberGroupTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, MemberGroupResource.getTreeMemberGroupRoot({}));
	}

	/**
	 * Fetches the children of a given parent key from the server
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof MemberGroupTreeServerDataSource
	 */
	async getChildrenOf(parentKey: string | null) {
		// Not implemented for this tree
		return {};
	}

	/**
	 * Fetches the items for the given keys from the server
	 * @param {Array<string>} keys
	 * @return {*}
	 * @memberof MemberGroupTreeServerDataSource
	 */
	async getItems(keys: Array<string>) {
		if (!keys || keys.length === 0) {
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			MemberGroupResource.getTreeMemberGroupItem({
				key: keys,
			})
		);
	}
}
