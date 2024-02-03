import type { UmbTemplateDetailModel } from '../../types.js';
import { UMB_TEMPLATE_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type { CreateTemplateRequestModel, UpdateTemplateRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { TemplateResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Template that fetches data from the server
 * @export
 * @class UmbTemplateServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbTemplateServerDataSource implements UmbDetailDataSource<UmbTemplateDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbTemplateServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbTemplateServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Template scaffold
	 * @param {(string | null)} parentUnique
	 * @return { CreateTemplateRequestModel }
	 * @memberof UmbTemplateServerDataSource
	 */
	async createScaffold(parentUnique: string | null) {
		const data: UmbTemplateDetailModel = {
			entityType: UMB_TEMPLATE_ENTITY_TYPE,
			unique: UmbId.new(),
			parentUnique,
			name: '',
			alias: '',
			content: '',
			masterTemplateId: null,
		};

		return { data };
	}

	/**
	 * Fetches a Template with the given id from the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbTemplateServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecuteAndNotify(this.#host, TemplateResource.getTemplateById({ id: unique }));

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const template: UmbTemplateDetailModel = {
			entityType: UMB_TEMPLATE_ENTITY_TYPE,
			unique: data.id,
			parentUnique: null,
			name: data.name,
			content: data.content || null,
			alias: data.alias,
			masterTemplateId: data.masterTemplateId || null,
		};

		return { data: template };
	}

	/**
	 * Inserts a new Template on the server
	 * @param {UmbTemplateDetailModel} model
	 * @return {*}
	 * @memberof UmbTemplateServerDataSource
	 */
	async create(model: UmbTemplateDetailModel) {
		if (!model) throw new Error('Template is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateTemplateRequestModel = {
			key: model.unique,
			name: model.name,
			content: model.content,
			alias: model.alias,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			TemplateResource.postTemplate({
				requestBody,
			}),
		);

		if (data) {
			return this.read(data);
		}

		return { error };
	}

	/**
	 * Updates a Template on the server
	 * @param {UmbTemplateDetailModel} Template
	 * @return {*}
	 * @memberof UmbTemplateServerDataSource
	 */
	async update(model: UmbTemplateDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: UpdateTemplateRequestModel = {
			name: model.name,
			content: model.content,
			alias: model.alias,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			TemplateResource.putTemplateById({
				id: model.unique,
				requestBody,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a Template on the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbTemplateServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			TemplateResource.deleteTemplateById({
				id: unique,
			}),
		);
	}
}
