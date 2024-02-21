import type { UmbMediaCollectionFilterModel } from '../types.js';
import { UmbMediaCollectionServerDataSource } from './media-collection.server.data-source.js';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaCollectionRepository implements UmbCollectionRepository {
	#collectionSource: UmbMediaCollectionServerDataSource;

	constructor(host: UmbControllerHost) {
		this.#collectionSource = new UmbMediaCollectionServerDataSource(host);
	}

	async requestCollection(query: UmbMediaCollectionFilterModel) {
		return this.#collectionSource.getCollection(query);
	}

	destroy(): void {}
}

export default UmbMediaCollectionRepository;
