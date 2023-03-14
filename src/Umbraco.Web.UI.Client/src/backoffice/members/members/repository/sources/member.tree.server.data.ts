import { MemberTreeDataSource } from '.';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

/**
 * A data source for the Member tree that fetches data from the server
 * @export
 * @class MemberTreeServerDataSource
 * @implements {MemberTreeDataSource}
 */
export class MemberTreeServerDataSource implements MemberTreeDataSource {
	#host: UmbControllerHostInterface;

	/**
	 * Creates an instance of MemberTreeServerDataSource.
	 * @param {UmbControllerHostInterface} host
	 * @memberof MemberTreeServerDataSource
	 */
	constructor(host: UmbControllerHostInterface) {
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
	async getItems(keys: Array<string>) {
		const response = await fetch('/umbraco/management/api/v1/tree/member/item');
		const data = await response.json();

		return { data, error: undefined };

		// if (keys) {
		// 	const error: ProblemDetailsModel = { title: 'Keys are missing' };
		// 	return { error };
		// }

		// return tryExecuteAndNotify(
		// 	this.#host,
		// 	MemberResource.getTreeMemberItem({
		// 		key: keys,
		// 	})
		// );
	}
}
