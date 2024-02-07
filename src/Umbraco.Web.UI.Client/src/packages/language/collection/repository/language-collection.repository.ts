import type { UmbLanguageCollectionFilterModel } from '../types.js';
import { UmbLanguageCollectionServerDataSource } from './language-collection.server.data-source.js';
import type { UmbLanguageCollectionDataSource } from './types.js';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbLanguageCollectionRepository implements UmbCollectionRepository {
	#collectionSource: UmbLanguageCollectionDataSource;

	constructor(host: UmbControllerHost) {
		this.#collectionSource = new UmbLanguageCollectionServerDataSource(host);
	}

	async requestCollection(filter: UmbLanguageCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}

	destroy(): void {}
}

export default UmbLanguageCollectionRepository;
