import type { UmbWebhookDeliveryCollectionFilterModel } from '../types.js';
import type { UmbWebhookDeliveryDetailModel } from '../../types.js';
import { UmbWebhookDeliveryCollectionServerDataSource } from './webhook-delivery-collection.server.data-source.js';
import type { UmbWebhookDeliveryCollectionDataSource } from './types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbWebhookDeliveryCollectionRepository
	extends UmbRepositoryBase
	implements UmbCollectionRepository<UmbWebhookDeliveryDetailModel, UmbWebhookDeliveryCollectionFilterModel>
{
	#collectionSource: UmbWebhookDeliveryCollectionDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#collectionSource = new UmbWebhookDeliveryCollectionServerDataSource(host);
	}

	async requestCollection(filter: UmbWebhookDeliveryCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}
}

export default UmbWebhookDeliveryCollectionRepository;
