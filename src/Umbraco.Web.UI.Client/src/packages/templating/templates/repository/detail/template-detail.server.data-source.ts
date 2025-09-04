import type { UmbTemplateDetailModel } from '../../types.js';
import { UMB_TEMPLATE_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateTemplateRequestModel,
	UpdateTemplateRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { TemplateService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Template that fetches data from the server
 * @class UmbTemplateServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbTemplateServerDataSource implements UmbDetailDataSource<UmbTemplateDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbTemplateServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbTemplateServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Template scaffold
	 * @param {Partial<UmbTemplateDetailModel>} [preset]
	 * @returns { CreateTemplateRequestModel }
	 * @memberof UmbTemplateServerDataSource
	 */
	async createScaffold(preset: Partial<UmbTemplateDetailModel> = {}) {
		const scaffold =
			'@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n@{\n\tLayout = null;\n}';

		const data: UmbTemplateDetailModel = {
			entityType: UMB_TEMPLATE_ENTITY_TYPE,
			unique: UmbId.new(),
			name: '',
			alias: '',
			content: scaffold,
			masterTemplate: preset.masterTemplate ?? null,
			...preset,
		};

		return { data };
	}

	/**
	 * Fetches a Template with the given id from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbTemplateServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(this.#host, TemplateService.getTemplateById({ path: { id: unique } }));

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const template: UmbTemplateDetailModel = {
			entityType: UMB_TEMPLATE_ENTITY_TYPE,
			unique: data.id,
			name: data.name,
			content: data.content || null,
			alias: data.alias,
			masterTemplate: data.masterTemplate ? { unique: data.masterTemplate.id } : null,
		};

		return { data: template };
	}

	/**
	 * Inserts a new Template on the server
	 * @param {UmbTemplateDetailModel} model
	 * @returns {*}
	 * @memberof UmbTemplateServerDataSource
	 */
	async create(model: UmbTemplateDetailModel) {
		if (!model) throw new Error('Template is missing');

		// TODO: make data mapper to prevent errors
		const body: CreateTemplateRequestModel = {
			id: model.unique,
			name: model.name,
			content: model.content,
			alias: model.alias,
		};

		const { data, error } = await tryExecute(
			this.#host,
			TemplateService.postTemplate({
				body,
			}),
		);

		if (data) {
			return this.read(data as never);
		}

		return { error };
	}

	/**
	 * Updates a Template on the server
	 * @param {UmbTemplateDetailModel} Template
	 * @param model
	 * @returns {*}
	 * @memberof UmbTemplateServerDataSource
	 */
	async update(model: UmbTemplateDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const body: UpdateTemplateRequestModel = {
			name: model.name,
			content: model.content,
			alias: model.alias,
		};

		const { error } = await tryExecute(
			this.#host,
			TemplateService.putTemplateById({
				path: { id: model.unique },
				body,
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
	 * @returns {*}
	 * @memberof UmbTemplateServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(
			this.#host,
			TemplateService.deleteTemplateById({
				path: { id: unique },
			}),
		);
	}
}
