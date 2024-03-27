import type { UmbRelationCollectionFilterModel } from '../types.js';
import { UmbRelationCollectionServerDataSource } from './relation-collection.server.data-source.js';
import type { UmbRelationCollectionDataSource } from './types.js';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbRelationCollectionRepository implements UmbCollectionRepository {
	#collectionSource: UmbRelationCollectionDataSource;

	constructor(host: UmbControllerHost) {
		this.#collectionSource = new UmbRelationCollectionServerDataSource(host);
	}

	async requestCollection(filter: UmbRelationCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}

	destroy(): void {}
}

export default UmbRelationCollectionRepository;
