import type { UmbWebhookCollectionFilterModel } from '../types.js';
import { UmbWebhookCollectionServerDataSource } from './webhook-collection.server.data-source.js';
import type { UmbWebhookCollectionDataSource } from './types.js';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbWebhookCollectionRepository implements UmbCollectionRepository {
	#collectionSource: UmbWebhookCollectionDataSource;

	constructor(host: UmbControllerHost) {
		this.#collectionSource = new UmbWebhookCollectionServerDataSource(host);
	}

	async requestCollection(filter: UmbWebhookCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}

	destroy(): void {}
}

export default UmbWebhookCollectionRepository;
