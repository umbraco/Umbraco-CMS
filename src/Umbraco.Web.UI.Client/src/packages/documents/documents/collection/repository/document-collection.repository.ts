import type { UmbDocumentCollectionFilterModel } from '../types.js';
import { UmbDocumentCollectionServerDataSource } from './document-collection.server.data-source.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentCollectionRepository extends UmbRepositoryBase implements UmbCollectionRepository {
	#collectionSource: UmbDocumentCollectionServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#collectionSource = new UmbDocumentCollectionServerDataSource(host);
	}

	async requestCollection(query: UmbDocumentCollectionFilterModel) {
		return this.#collectionSource.getCollection(query);
	}
}

export default UmbDocumentCollectionRepository;
