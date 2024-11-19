import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionDataSource, UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbClipboardCollectionFilterModel } from '../types';
import { UmbClipboardCollectionLocalStorageDataSource } from './clipboard-collection.local-storage.data-source';

export class UmbClipboardCollectionRepository extends UmbRepositoryBase implements UmbCollectionRepository {
	#collectionSource: UmbCollectionDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#collectionSource = new UmbClipboardCollectionLocalStorageDataSource(host);
	}

	async requestCollection(filter: UmbClipboardCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}
}

export { UmbClipboardCollectionRepository as api };
