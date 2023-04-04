import { MemberTypeResource, ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the MemberType tree that fetches data from the server
 * @export
 * @class MemberTypeTreeServerDataSource
 * @implements {MemberTypeTreeDataSource}
 */
export class MemberTypeTreeServerDataSource implements UmbTreeDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of MemberTypeTreeDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof MemberTypeTreeDataSource
	 */
	constructor(host: UmbControllerHostElement) {
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
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof MemberTypeTreeServerDataSource
	 */
	async getChildrenOf(parentId: string | null) {
		const error: ProblemDetailsModel = { title: 'Not implemented for Member Type' };
		return { error };
	}

	/**
	 * Fetches the items for the given keys from the server
	 * @param {Array<string>} keys
	 * @return {*}
	 * @memberof MemberTypeTreeServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids || ids.length === 0) {
			const error: ProblemDetailsModel = { title: 'Ids are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			MemberTypeResource.getTreeMemberTypeItem({
				id: ids,
			})
		);
	}
}
