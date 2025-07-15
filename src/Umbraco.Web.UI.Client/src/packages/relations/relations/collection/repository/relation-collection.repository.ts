import type { UmbRelationCollectionFilterModel } from '../types.js';
import { UmbRelationCollectionServerDataSource } from './relation-collection.server.data-source.js';
import type { UmbRelationCollectionDataSource } from './types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbRelationCollectionRepository extends UmbRepositoryBase implements UmbCollectionRepository {
	#collectionSource: UmbRelationCollectionDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#collectionSource = new UmbRelationCollectionServerDataSource(host);
	}

	async requestCollection(filter: UmbRelationCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}
}

export default UmbRelationCollectionRepository;
