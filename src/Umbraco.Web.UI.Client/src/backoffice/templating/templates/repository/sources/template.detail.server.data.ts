import { v4 as uuid } from 'uuid';
import { ProblemDetailsModel, TemplateModel, TemplateResource } from '@umbraco-cms/backend-api';
import type { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import type { DataSourceResponse } from '@umbraco-cms/models';

export interface TemplateDetailDataSource {
	createScaffold(): Promise<DataSourceResponse<TemplateModel>>;
	get(key: string): Promise<DataSourceResponse<TemplateModel>>;
	insert(template: TemplateModel): Promise<DataSourceResponse>;
	update(template: TemplateModel): Promise<DataSourceResponse>;
	delete(key: string): Promise<DataSourceResponse>;
}

/**
 * A data source for the Template detail that fetches data from the server
 * @export
 * @class UmbTemplateDetailServerDataSource
 * @implements {TemplateDetailDataSource}
 */
export class UmbTemplateDetailServerDataSource implements TemplateDetailDataSource {
	#host: UmbControllerHostInterface;

	/**
	 * Creates an instance of UmbTemplateDetailServerDataSource.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
	}

	/**
	 * Fetches a Template with the given key from the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	get(key: string) {
		return tryExecuteAndNotify(this.#host, TemplateResource.getTemplateByKey({ key }));
	}

	/**
	 * Creates a new Template scaffold
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async createScaffold() {
		const error = undefined;
		const data: TemplateModel = {
			$type: '',
			key: uuid(),
			name: '',
			alias: '',
			content: '',
		};

		// TODO: update when backend is updated so we don't have to do two calls
		/*
		// TODO: Revisit template models, masterTemplateAlias is not here anymore?
		const { data: scaffoldData, error: scaffoldError } = await tryExecuteAndNotify(
			this.#host,
			TemplateResource.getTemplateScaffold()
		);
		*/

		//error = scaffoldError;
		//data.content = scaffoldData?.content || '';

		return { data, error };
	}

	/**
	 * Inserts a new Template on the server
	 * @param {Template} template
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async insert(template: TemplateModel) {
		const payload = { requestBody: template };
		// TODO: fix type mismatch
		return tryExecuteAndNotify(
			this.#host,
			tryExecuteAndNotify(this.#host, TemplateResource.postTemplate(payload)) as any
		) as any;
	}

	/**
	 * Updates a Template on the server
	 * @param {Template} template
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async update(template: TemplateModel) {
		if (!template.key) {
			const error: ProblemDetailsModel = { title: 'Template key is missing' };
			return { error };
		}

		const payload = { key: template.key, requestBody: template };
		return tryExecuteAndNotify(this.#host, TemplateResource.putTemplateByKey(payload));
	}

	/**
	 * Deletes a Template on the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async delete(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		return await tryExecuteAndNotify(this.#host, TemplateResource.deleteTemplateByKey({ key }));
	}
}
