import type { UmbWebhookDeliveryCollectionFilterModel } from '../types.js';
import type { UmbWebhookDeliveryDetailModel } from '../../../types.js';
import { UmbWebhookDeliveryCollectionServerDataSource } from './webhook-delivery-collection.server.data-source.js';
import type { UmbWebhookDeliveryCollectionDataSource } from './types.js';

import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_WEBHOOK_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/webhook';

export class UmbWebhookDeliveryCollectionRepository
	extends UmbRepositoryBase
	implements UmbCollectionRepository<UmbWebhookDeliveryDetailModel, UmbWebhookDeliveryCollectionFilterModel>
{
	#collectionSource: UmbWebhookDeliveryCollectionDataSource;

	#webhookUnique?: string;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#collectionSource = new UmbWebhookDeliveryCollectionServerDataSource(host);

		this.consumeContext(UMB_WEBHOOK_WORKSPACE_CONTEXT, (instance) => {
			this.observe(instance.data, (webhook) => {
				this.#webhookUnique = webhook?.unique;
			});
		});
	}

	async requestCollection(filter: UmbWebhookDeliveryCollectionFilterModel) {
		const filterWithId = {
			...filter,
			webhook: { unique: this.#webhookUnique! },
		};

		return this.#collectionSource.getCollection(filterWithId);
	}
}

export default UmbWebhookDeliveryCollectionRepository;
