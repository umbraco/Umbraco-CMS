import { TemplateTreeDataSource } from '.';
import { ProblemDetailsModel, TemplateResource } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

/**
 * A data source for the Template tree that fetches data from the server
 * @export
 * @class TemplateTreeServerDataSource
 * @implements {TemplateTreeDataSource}
 */
export class TemplateTreeServerDataSource implements TemplateTreeDataSource {
	#host: UmbControllerHostInterface;

	/**
	 * Creates an instance of TemplateTreeServerDataSource.
	 * @param {UmbControllerHostInterface} host
	 * @memberof TemplateTreeServerDataSource
	 */
	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof TemplateTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, TemplateResource.getTreeTemplateRoot({}));
	}

	/**
	 * Fetches the children of a given parent key from the server
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof TemplateTreeServerDataSource
	 */
	async getChildrenOf(parentKey: string | null) {
		if (!parentKey) {
			const error: ProblemDetailsModel = { title: 'Parent key is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			TemplateResource.getTreeTemplateChildren({
				parentKey,
			})
		);
	}

	/**
	 * Fetches the items for the given keys from the server
	 * @param {Array<string>} keys
	 * @return {*}
	 * @memberof TemplateTreeServerDataSource
	 */
	async getItems(keys: Array<string>) {
		if (!keys) {
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			TemplateResource.getTreeTemplateItem({
				key: keys,
			})
		);
	}
}
