import { v4 as uuid } from 'uuid';
import { ProblemDetailsModel, TemplateResponseModel, TemplateResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface TemplateDetailDataSource {
	createScaffold(): Promise<DataSourceResponse<TemplateResponseModel>>;
	get(id: string): Promise<DataSourceResponse<TemplateResponseModel>>;
	insert(template: TemplateResponseModel): Promise<DataSourceResponse>;
	update(template: TemplateResponseModel): Promise<DataSourceResponse>;
	delete(id: string): Promise<DataSourceResponse>;
}

/**
 * A data source for the Template detail that fetches data from the server
 * @export
 * @class UmbTemplateDetailServerDataSource
 * @implements {TemplateDetailDataSource}
 */
export class UmbTemplateDetailServerDataSource implements TemplateDetailDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbTemplateDetailServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches a Template with the given id from the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	get(id: string) {
		return tryExecuteAndNotify(this.#host, TemplateResource.getTemplateById({ id }));
	}

	/**
	 * Creates a new Template scaffold
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async createScaffold() {
		const error = undefined;
		const data: TemplateResponseModel = {
			$type: '',
			id: uuid(),
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
	async insert(template: TemplateResponseModel) {
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
	async update(template: TemplateResponseModel) {
		if (!template.id) {
			const error: ProblemDetailsModel = { title: 'Template id is missing' };
			return { error };
		}

		const payload = { id: template.id, requestBody: template };
		return tryExecuteAndNotify(this.#host, TemplateResource.putTemplateById(payload));
	}

	/**
	 * Deletes a Template on the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async delete(id: string) {
		if (!id) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		return await tryExecuteAndNotify(this.#host, TemplateResource.deleteTemplateById({ id }));
	}
}
