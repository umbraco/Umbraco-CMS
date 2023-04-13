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
	 * Fetches the children of a given parent id from the server
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof TemplateTreeServerDataSource
	 */
	async getChildrenOf(parentId: string | null) {
		if (!parentId) {
			const error: ProblemDetailsModel = { title: 'Parent id is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			TemplateResource.getTreeTemplateChildren({
				parentId,
			})
		);
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} id
	 * @return {*}
	 * @memberof TemplateTreeServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids) {
			const error: ProblemDetailsModel = { title: 'Ids are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			TemplateResource.getTemplateItem({
				id: ids,
			})
		);
	}
}
