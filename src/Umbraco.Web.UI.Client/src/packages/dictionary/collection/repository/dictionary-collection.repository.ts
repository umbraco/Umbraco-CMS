import type { UmbDictionaryCollectionFilterModel } from '../types.js';
import { UmbDictionaryCollectionServerDataSource } from './dictionary-collection.server.data-source.js';
import type { UmbDictionaryCollectionDataSource } from './types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDictionaryCollectionRepository extends UmbRepositoryBase implements UmbCollectionRepository {
	#collectionSource: UmbDictionaryCollectionDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#collectionSource = new UmbDictionaryCollectionServerDataSource(host);
	}

	async requestCollection(filter: UmbDictionaryCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}
}

export default UmbDictionaryCollectionRepository;
