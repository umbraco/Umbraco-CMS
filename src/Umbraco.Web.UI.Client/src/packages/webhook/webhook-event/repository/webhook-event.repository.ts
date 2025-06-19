import { UmbWebhookEventServerDataSource } from './webhook-event.server.data-source.js';
import type { UmbWebhookEventStore } from './webhook-event.store.js';
import { UMB_WEBHOOK_EVENT_STORE_CONTEXT } from './webhook-event.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbWebhookEventRepository extends UmbRepositoryBase implements UmbApi {
	#init: Promise<unknown>;

	#store?: UmbWebhookEventStore;
	#source: UmbWebhookEventServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#source = new UmbWebhookEventServerDataSource(host);

		this.#init = this.consumeContext(UMB_WEBHOOK_EVENT_STORE_CONTEXT, (instance) => {
			if (instance) {
				this.#store = instance;
			}
		})
			.asPromise({ preventTimeout: true })
			// Ignore the error, we can assume that the flow was stopped (asPromise failed), but it does not mean that the consumption was not successful.
			.catch(() => undefined);
	}

	async requestEvents() {
		await this.#init;

		const { data, error } = await this.#source.getAll();

		if (!this.#store) {
			return {};
		}

		if (data) {
			this.#store.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#store!.all() };
	}
}

export default UmbWebhookEventRepository;
