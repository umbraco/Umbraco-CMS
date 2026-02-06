import { UmbExtensionCollectionExtensionRegistryDataSource } from './collection.extension-registry.data-source.js';
import type { UmbExtensionCollectionFilterModel } from './types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';

export class UmbExtensionCollectionRepository extends UmbRepositoryBase implements UmbCollectionRepository {
	#collectionSource = new UmbExtensionCollectionExtensionRegistryDataSource(this);

	async requestCollection(filter: UmbExtensionCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}
}

export { UmbExtensionCollectionRepository as api };
