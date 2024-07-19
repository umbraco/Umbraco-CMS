import type { UmbWebhookCollectionFilterModel } from '../types.js';
import { UmbWebhookCollectionServerDataSource } from './webhook-collection.server.data-source.js';
import type { UmbWebhookCollectionDataSource } from './types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbWebhookCollectionRepository extends UmbRepositoryBase implements UmbCollectionRepository {
	#collectionSource: UmbWebhookCollectionDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#collectionSource = new UmbWebhookCollectionServerDataSource(host);
	}

	async requestCollection(filter: UmbWebhookCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}
}

export default UmbWebhookCollectionRepository;
