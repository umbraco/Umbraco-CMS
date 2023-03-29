import { TemplateTreeDataSource } from '.';
import { ProblemDetailsModel, TemplateResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Template tree that fetches data from the server
 * @export
 * @class TemplateTreeServerDataSource
 * @implements {TemplateTreeDataSource}
 */
export class TemplateTreeServerDataSource implements TemplateTreeDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of TemplateTreeServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof TemplateTreeServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
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
