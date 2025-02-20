import type { UmbClipboardCollectionFilterModel } from '../types.js';
import { UmbClipboardCollectionLocalStorageDataSource } from './clipboard-collection.local-storage.data-source.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';

export class UmbClipboardCollectionRepository extends UmbRepositoryBase implements UmbCollectionRepository {
	#collectionSource = new UmbClipboardCollectionLocalStorageDataSource(this);

	async requestCollection(filter: UmbClipboardCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}
}

export { UmbClipboardCollectionRepository as api };
