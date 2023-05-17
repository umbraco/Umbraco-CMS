import { MemberTreeDataSource } from '.';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Member tree that fetches data from the server
 * @export
 * @class UmbMemberTreeServerDataSource
 * @implements {MemberTreeDataSource}
 */
export class UmbMemberTreeServerDataSource implements MemberTreeDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbMemberTreeServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMemberTreeServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbMemberTreeServerDataSource
	 */
	async getRootItems() {
		const response = await fetch('/umbraco/management/api/v1/tree/member/root');
		const data = await response.json();

		return { data, error: undefined };
		//return tryExecuteAndNotify(this.#host, MemberResource.getTreeMemberRoot({}));
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbMemberTreeServerDataSource
	 */
	async getItems(ids: Array<string>) {
		const response = await fetch('/umbraco/management/api/v1/tree/member/item');
		const data = await response.json();

		return { data, error: undefined };

		// if (ids) {
		// 	const error: ProblemDetailsModel = { title: 'Ids are missing' };
		// 	return { error };
		// }

		// return tryExecuteAndNotify(
		// 	this.#host,
		// 	MemberResource.getTreeMemberItem({
		// 		id: ids,
		// 	})
		// );
	}
}
