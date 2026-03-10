import { UmbPropertyEditorDataSourceCollectionExtensionRegistryDataSource } from './collection.extension-registry.data-source.js';
import type { UmbPropertyEditorDataSourceCollectionFilterModel } from './types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';

export class UmbPropertyEditorDataSourceCollectionRepository
	extends UmbRepositoryBase
	implements UmbCollectionRepository
{
	#collectionSource = new UmbPropertyEditorDataSourceCollectionExtensionRegistryDataSource(this);

	async requestCollection(filter: UmbPropertyEditorDataSourceCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}
}

export { UmbPropertyEditorDataSourceCollectionRepository as api };
