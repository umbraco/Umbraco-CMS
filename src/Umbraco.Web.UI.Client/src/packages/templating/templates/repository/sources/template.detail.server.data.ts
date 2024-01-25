import type {
	TemplateResponseModel,
	CreateTemplateRequestModel,
	UpdateTemplateRequestModel,
	TemplateScaffoldResponseModel} from '@umbraco-cms/backoffice/backend-api';
import {
	TemplateResource
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for the Template detail that fetches data from the server
 * @export
 * @class UmbTemplateDetailServerDataSource
 * @implements {TemplateDetailDataSource}
 */
export class UmbTemplateDetailServerDataSource
	implements
		UmbDataSource<
			CreateTemplateRequestModel,
			string,
			UpdateTemplateRequestModel,
			TemplateResponseModel,
			string,
			TemplateScaffoldResponseModel
		>
{
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbTemplateDetailServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches a Template with the given id from the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	read(id: string) {
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
		return await tryExecuteAndNotify(this.#host, TemplateResource.getTemplateItem({ id: id }));
	}

	/**
	 * Creates a new Template scaffold
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async createScaffold(masterTemplateId: string | null) {
		return await tryExecuteAndNotify(
			this.#host,
			TemplateResource.getTemplateScaffold({ masterTemplateId: masterTemplateId ?? undefined }),
		);
	}

	/**
	 * Creates a new Template on the server
	 * @param {Template} template
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async create(template: CreateTemplateRequestModel) {
		if (!template) throw new Error('Template is missing');
		return await tryExecuteAndNotify(this.#host, TemplateResource.postTemplate({ requestBody: template }));
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
		return await tryExecuteAndNotify(this.#host, TemplateResource.putTemplateById({ id, requestBody: template }));
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
