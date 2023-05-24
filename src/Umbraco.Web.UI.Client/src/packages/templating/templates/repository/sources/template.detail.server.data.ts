import {
	TemplateResponseModel,
	TemplateResource,
	CreateTemplateRequestModel,
	UpdateTemplateRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for the Template detail that fetches data from the server
 * @export
 * @class UmbTemplateDetailServerDataSource
 * @implements {TemplateDetailDataSource}
 */
export class UmbTemplateDetailServerDataSource
	implements UmbDataSource<CreateTemplateRequestModel, string, UpdateTemplateRequestModel, TemplateResponseModel>
{
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
		if (!id) throw new Error('Id is missing');
		return tryExecuteAndNotify(this.#host, TemplateResource.getTemplateById({ id }));
	}

	/**
	 * Gets template item - you can get name and id from that
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async getItem(id: string[]) {
		if (!id) throw new Error('Id is missing');
		return await tryExecuteAndNotify(this.#host, TemplateResource.getTemplateItem({ id }));
	}

	/**
	 * Creates a new Template scaffold
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async createScaffold() {
		return tryExecuteAndNotify(this.#host, TemplateResource.getTemplateScaffold());
	}

	/**
	 * Creates a new Template on the server
	 * @param {Template} template
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async insert(template: CreateTemplateRequestModel) {
		if (!template) throw new Error('Template is missing');
		return tryExecuteAndNotify(this.#host, TemplateResource.postTemplate({ requestBody: template }));
	}

	/**
	 * Updates a Template on the server
	 * @param {Template} template
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async update(id: string, template: UpdateTemplateRequestModel) {
		if (!id) throw new Error('You need to pass template id to update it');
		if (!template) throw new Error('Template is missing');
		return tryExecuteAndNotify(this.#host, TemplateResource.putTemplateById({ id, requestBody: template }));
	}

	/**
	 * Deletes a Template on the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async delete(id: string) {
		if (!id) throw new Error('You need to pass template id to delete it');
		return await tryExecuteAndNotify(this.#host, TemplateResource.deleteTemplateById({ id }));
	}
}
