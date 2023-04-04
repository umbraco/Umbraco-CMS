import { MemberTreeDataSource } from '.';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

/**
 * A data source for the Member tree that fetches data from the server
 * @export
 * @class MemberTreeServerDataSource
 * @implements {MemberTreeDataSource}
 */
export class MemberTreeServerDataSource implements MemberTreeDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of MemberTreeServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof MemberTreeServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof MemberTreeServerDataSource
	 */
	async getRootItems() {
		const response = await fetch('/umbraco/management/api/v1/tree/member/root');
		const data = await response.json();

		return { data, error: undefined };
		//return tryExecuteAndNotify(this.#host, MemberResource.getTreeMemberRoot({}));
	}

	/**
	 * Fetches the items for the given keys from the server
	 * @param {Array<string>} keys
	 * @return {*}
	 * @memberof MemberTreeServerDataSource
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
