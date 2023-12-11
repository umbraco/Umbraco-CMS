import { TemplateQueryExecuteModel, TemplateResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Template Query Builder that fetches data from the server
 * @export
 * @class UmbTemplateQueryBuilderServerDataSource
 */
export class UmbTemplateQueryBuilderServerDataSource {
	#host: UmbControllerHost;

	// TODO: When we map the server models to our own models, we need to have a localization property.
	// For example, the OperatorModel.NOT_EQUALS need to use the localization key "template_doesNotEqual"

	/**
	 * Creates an instance of UmbTemplateQueryBuilderServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbTemplateQueryBuilderServerDataSource
	 */
	constructor(host: UmbControllerHost) {
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
	/**
	 * Executes a query builder query on the server
	 *
	 * @param {{ requestBody?: TemplateQueryExecuteModel }} { requestBody }
	 * @return {*}
	 * @memberof UmbTemplateQueryBuilderServerDataSource
	 */
	async postTemplateQueryExecute({ requestBody }: { requestBody?: TemplateQueryExecuteModel }) {
		return tryExecuteAndNotify(this.#host, TemplateResource.postTemplateQueryExecute({ requestBody }));
	}
}
