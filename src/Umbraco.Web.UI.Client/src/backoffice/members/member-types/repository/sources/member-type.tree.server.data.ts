import { MemberTypeResource, ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the MemberType tree that fetches data from the server
 * @export
 * @class UmbMemberTypeTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbMemberTypeTreeServerDataSource implements UmbTreeDataSource {
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
	 * @memberof UmbMemberTypeTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, MemberTypeResource.getTreeMemberTypeRoot({}));
	}

	/**
	 * Fetches the children of a given parent id from the server
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbMemberTypeTreeServerDataSource
	 */
	async getChildrenOf(parentId: string | null) {
		const error: ProblemDetailsModel = { title: 'Not implemented for Member Type' };
		return { error };
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbMemberTypeTreeServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids || ids.length === 0) {
			const error: ProblemDetailsModel = { title: 'Ids are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			MemberTypeResource.getMemberTypeItem({
				id: ids,
			})
		);
	}
}
