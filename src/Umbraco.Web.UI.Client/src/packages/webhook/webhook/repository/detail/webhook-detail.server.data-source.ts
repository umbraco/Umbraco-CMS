import type { UmbWebhookDetailModel } from '../../types.js';
import { UMB_WEBHOOK_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateWebhookRequestModel,
	UpdateWebhookRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { WebhookService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Webhook that fetches data from the server
 * @class UmbWebhookDetailServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbWebhookDetailServerDataSource implements UmbDetailDataSource<UmbWebhookDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbWebhookDetailServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbWebhookDetailServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Webhook scaffold
	 * @param {Partial<UmbWebhookDetailModel>} [preset]
	 * @returns { CreateWebhookRequestModel }
	 * @memberof UmbWebhookDetailServerDataSource
	 */
	async createScaffold(preset: Partial<UmbWebhookDetailModel> = {}) {
		const data: UmbWebhookDetailModel = {
			entityType: UMB_WEBHOOK_ENTITY_TYPE,
			unique: UmbId.new(),
			headers: {},
			events: [],
			enabled: true,
			url: '',
			contentTypes: [],
			...preset,
		};

		return { data };
	}

	/**
	 * Fetches a Webhook with the given id from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbWebhookDetailServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecuteAndNotify(this.#host, WebhookService.getWebhookById({ id: unique }));

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const dataType: UmbWebhookDetailModel = {
			entityType: UMB_WEBHOOK_ENTITY_TYPE,
			unique: data.id,
			headers: data.headers,
			events: data.events,
			enabled: data.enabled,
			url: data.url,
			contentTypes: data.contentTypeKeys,
		};

		return { data: dataType };
	}

	/**
	 * Inserts a new Webhook on the server
	 * @param {UmbWebhookDetailModel} model
	 * @returns {*}
	 * @memberof UmbWebhookDetailServerDataSource
	 */
	async create(model: UmbWebhookDetailModel) {
		if (!model) throw new Error('Webhook is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateWebhookRequestModel = {
			id: model.unique,
			headers: model.headers,
			events: model.events.map((event) => event.alias),
			enabled: model.enabled,
			url: model.url,
			contentTypeKeys: model.contentTypes,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			WebhookService.postWebhook({
				requestBody,
			}),
		);

		if (data) {
			return this.read(data);
		}

		return { error };
	}

	/**
	 * Updates a Webhook on the server
	 * @param {UmbWebhookDetailModel} Webhook
	 * @param model
	 * @returns {*}
	 * @memberof UmbWebhookDetailServerDataSource
	 */
	async update(model: UmbWebhookDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: UpdateWebhookRequestModel = {
			headers: model.headers,
			events: model.events.map((event) => event.alias),
			enabled: model.enabled,
			url: model.url,
			contentTypeKeys: model.contentTypes,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			WebhookService.putWebhookById({
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
	 * Deletes a Webhook on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbWebhookDetailServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			WebhookService.deleteWebhookById({
				id: unique,
			}),
		);
	}
}
