import type { UmbWebhookDeliveryCollectionFilterModel } from '../types.js';
import { UmbWebhookDeliveryCollectionServerDataSource } from './webhook-delivery-collection.server.data-source.js';
import type { UmbWebhookDeliveryCollectionDataSource } from './types.js';

import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_WEBHOOK_WORKSPACE_CONTEXT } from 'src/packages/webhook/constants.js';

export class UmbWebhookDeliveryCollectionRepository extends UmbRepositoryBase implements UmbCollectionRepository {
	#collectionSource: UmbWebhookDeliveryCollectionDataSource;

	#webhookId: string = "";

	constructor(host: UmbControllerHost) {
		super(host);

		this.#collectionSource = new UmbWebhookDeliveryCollectionServerDataSource(host);

		this.consumeContext(UMB_WEBHOOK_WORKSPACE_CONTEXT, (instance) => {
			this.observe(instance.data, (webhook) => {
				this.#webhookId = webhook?.unique ?? "";
			});
		});
	}

	async requestCollection(filter: UmbWebhookDeliveryCollectionFilterModel) {
		const filterWithId = { ...filter, id: this.#webhookId };
		return this.#collectionSource.getCollection(filterWithId);
	}
}

export default UmbWebhookDeliveryCollectionRepository;
