import type { UmbRelationTypeCollectionFilterModel } from '../types.js';
import { UmbRelationTypeCollectionServerDataSource } from './relation-type-collection.server.data-source.js';
import type { UmbRelationTypeCollectionDataSource } from './types.js';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbRelationTypeCollectionRepository implements UmbCollectionRepository {
	#collectionSource: UmbRelationTypeCollectionDataSource;

	constructor(host: UmbControllerHost) {
		this.#collectionSource = new UmbRelationTypeCollectionServerDataSource(host);
	}

	async requestCollection(filter: UmbRelationTypeCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}

	destroy(): void {}
}

export default UmbRelationTypeCollectionRepository;
