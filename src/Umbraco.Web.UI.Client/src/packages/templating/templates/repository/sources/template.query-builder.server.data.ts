import { TemplateQueryExecuteModel, TemplateResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Template Query Builder that fetches data from the server
 * @export
 * @class UmbTemplateQueryBuilderServerDataSource
 */
export class UmbTemplateQueryBuilderServerDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbTemplateQueryBuilderServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbTemplateQueryBuilderServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}
	/**
	 * Fetches the query builder settings from the server
	 *
	 * @return {*}
	 * @memberof UmbTemplateQueryBuilderServerDataSource
	 */
	async getTemplateQuerySettings() {
		return tryExecuteAndNotify(this.#host, TemplateResource.getTemplateQuerySettings());
	}

	async postTemplateQueryExecute({ requestBody }: { requestBody?: TemplateQueryExecuteModel }) {
		return tryExecuteAndNotify(this.#host, TemplateResource.postTemplateQueryExecute({ requestBody }));
	}
}
