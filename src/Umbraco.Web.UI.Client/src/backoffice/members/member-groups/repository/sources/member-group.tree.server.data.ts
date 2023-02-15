import { MemberGroupResource, ProblemDetailsModel } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { RepositoryTreeDataSource } from '@umbraco-cms/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

/**
 * A data source for the Member Group tree that fetches data from the server
 * @export
 * @class MemberGroupTreeServerDataSource
 * @implements {MemberGroupTreeDataSource}
 */
export class MemberGroupTreeServerDataSource implements RepositoryTreeDataSource {
	#host: UmbControllerHostInterface;

	/**
	 * Creates an instance of MemberGroupTreeServerDataSource.
	 * @param {UmbControllerHostInterface} host
	 * @memberof MemberGroupTreeServerDataSource
	 */
	constructor(host: UmbControllerHostInterface) {
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
