import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbClipboardCollectionFilterModel } from '../types';
import { UmbClipboardCollectionLocalStorageDataSource } from './clipboard-collection.local-storage.data-source';

export class UmbClipboardCollectionRepository extends UmbRepositoryBase implements UmbCollectionRepository {
	#collectionSource = new UmbClipboardCollectionLocalStorageDataSource();

	async requestCollection(filter: UmbClipboardCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}
}

export { UmbClipboardCollectionRepository as api };
