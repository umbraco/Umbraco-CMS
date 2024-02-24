import type { UmbDocumentCollectionFilterModel } from '../types.js';
import { UmbDocumentCollectionServerDataSource } from './document-collection.server.data-source.js';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentCollectionRepository implements UmbCollectionRepository {
	#collectionSource: UmbDocumentCollectionServerDataSource;

	constructor(host: UmbControllerHost) {
		this.#collectionSource = new UmbDocumentCollectionServerDataSource(host);
	}

	async requestCollection(query: UmbDocumentCollectionFilterModel) {
		return this.#collectionSource.getCollection(query);
	}

	destroy(): void {}
}

export default UmbDocumentCollectionRepository;
