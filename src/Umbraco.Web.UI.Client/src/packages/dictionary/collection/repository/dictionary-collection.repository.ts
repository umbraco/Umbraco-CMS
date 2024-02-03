import type { UmbDictionaryCollectionFilterModel } from '../types.js';
import { UmbDictionaryCollectionServerDataSource } from './dictionary-collection.server.data-source.js';
import type { UmbDictionaryCollectionDataSource } from './types.js';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDictionaryCollectionRepository implements UmbCollectionRepository {
	#collectionSource: UmbDictionaryCollectionDataSource;

	constructor(host: UmbControllerHost) {
		this.#collectionSource = new UmbDictionaryCollectionServerDataSource(host);
	}

	async requestCollection(filter: UmbDictionaryCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}

	destroy(): void {}
}

export default UmbDictionaryCollectionRepository;
