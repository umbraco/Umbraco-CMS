import { ExampleDynamicFacetCollectionDataSource } from './collection.data-source.js';
import type { ExampleDynamicFacetCollectionFilterModel, ExampleProductCollectionItemModel } from './types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';

export class ExampleDynamicFacetCollectionRepository
	extends UmbRepositoryBase
	implements UmbCollectionRepository<ExampleProductCollectionItemModel, ExampleDynamicFacetCollectionFilterModel>
{
	#dataSource = new ExampleDynamicFacetCollectionDataSource();

	async requestCollection(args: ExampleDynamicFacetCollectionFilterModel) {
		return this.#dataSource.getCollection(args);
	}
}

export { ExampleDynamicFacetCollectionRepository as api };
