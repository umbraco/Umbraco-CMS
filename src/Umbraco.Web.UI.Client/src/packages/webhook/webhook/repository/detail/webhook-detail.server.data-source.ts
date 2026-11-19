import type { UmbWebhookDetailModel } from '../../../types.js';
import { UMB_WEBHOOK_ENTITY_TYPE } from '../../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	UmbDataSourceErrorResponse,
	UmbDataSourceResponse,
	UmbDetailDataSource,
} from '@umbraco-cms/backoffice/repository';
import type {
	CreateWebhookRequestModel,
	UpdateWebhookRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { WebhookService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Webhook that fetches data from the server
 * @class UmbWebhookDetailServerDataSource
 * @implements {UmbDetailDataSource<UmbWebhookDetailModel>}
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
	 * @param {Partial<UmbWebhookDetailModel>} [preset] Initial values to seed the scaffold with
	 * @returns { CreateWebhookRequestModel } The scaffolded Webhook
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
			name: '',
			description: '',
			contentTypes: [],
			...preset,
		};

		return { data };
	}

	/**
	 * Fetches a Webhook with the given id from the server
	 * @param {string} unique The unique id of the Webhook
	 * @returns {Promise<UmbDataSourceResponse<UmbWebhookDetailModel>>} The Webhook
	 * @memberof UmbWebhookDetailServerDataSource
	 */
	async read(unique: string): Promise<UmbDataSourceResponse<UmbWebhookDetailModel>> {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(this.#host, WebhookService.getWebhookById({ path: { id: unique } }));

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
			name: data.name ?? '',
			description: data.description,
			contentTypes: data.contentTypeKeys,
		};

		return { data: dataType };
	}

	/**
	 * Inserts a new Webhook on the server
	 * @param {UmbWebhookDetailModel} model The Webhook to create
	 * @returns {Promise<UmbDataSourceResponse<UmbWebhookDetailModel>>} The created Webhook
	 * @memberof UmbWebhookDetailServerDataSource
	 */
	async create(model: UmbWebhookDetailModel): Promise<UmbDataSourceResponse<UmbWebhookDetailModel>> {
		if (!model) throw new Error('Webhook is missing');

		// TODO: make data mapper to prevent errors
		const body: CreateWebhookRequestModel = {
			id: model.unique,
			headers: model.headers,
			events: model.events.map((event) => event.alias),
			enabled: model.enabled,
			url: model.url,
			name: model.name,
			description: model.description,
			contentTypeKeys: model.contentTypes,
		};

		const { data, error } = await tryExecute(
			this.#host,
			WebhookService.postWebhook({
				body,
			}),
		);

		if (data) {
			return this.read(data as never);
		}

		return { error };
	}

	/**
	 * Updates a Webhook on the server
	 * @param {UmbWebhookDetailModel} model The Webhook to update
	 * @returns {Promise<UmbDataSourceResponse<UmbWebhookDetailModel>>} The updated Webhook
	 * @memberof UmbWebhookDetailServerDataSource
	 */
	async update(model: UmbWebhookDetailModel): Promise<UmbDataSourceResponse<UmbWebhookDetailModel>> {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const body: UpdateWebhookRequestModel = {
			headers: model.headers,
			events: model.events.map((event) => event.alias),
			enabled: model.enabled,
			url: model.url,
			name: model.name,
			description: model.description,
			contentTypeKeys: model.contentTypes,
		};

		const { error } = await tryExecute(
			this.#host,
			WebhookService.putWebhookById({
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
	 * Deletes a Webhook on the server
	 * @param {string} unique The unique id of the Webhook
	 * @returns {Promise<UmbDataSourceErrorResponse>} The result of the deletion
	 * @memberof UmbWebhookDetailServerDataSource
	 */
	async delete(unique: string): Promise<UmbDataSourceErrorResponse> {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(
			this.#host,
			WebhookService.deleteWebhookById({
				path: { id: unique },
			}),
		);
	}
}
