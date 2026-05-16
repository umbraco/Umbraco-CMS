import type { UmbWebhookEventCollectionFilterModel } from '../types.js';
import { UmbWebhookEventCollectionServerDataSource } from './webhook-event-collection.server.data-source.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';

export class UmbWebhookEventCollectionRepository extends UmbRepositoryBase implements UmbCollectionRepository {
	#collectionSource = new UmbWebhookEventCollectionServerDataSource(this);

	async requestCollection(filter: UmbWebhookEventCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}
}

export default UmbWebhookEventCollectionRepository;
