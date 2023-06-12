import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';
import { TemplateItemResponseModel, TemplateResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Data Type items that fetches data from the server
 * @export
 * @class UmbTemplateItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbTemplateItemServerDataSource implements UmbItemDataSource<TemplateItemResponseModel> {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbTemplateItemServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbTemplateItemServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbTemplateItemServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids) throw new Error('Ids are missing');
		return tryExecuteAndNotify(
			this.#host,
			TemplateResource.getTemplateItem({
				id: ids,
			})
		);
	}
}
