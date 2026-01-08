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

		this.#init = Promise.all([
			this.consumeContext(UMB_WEBHOOK_EVENT_STORE_CONTEXT, (instance) => {
				this.#store = instance;
			}).asPromise(),
		]);
	}

	async requestEvents() {
		await this.#init;

		const { data, error } = await this.#source.getAll();

		if (data) {
			this.#store!.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#store!.all() };
	}
}

export default UmbWebhookEventRepository;
