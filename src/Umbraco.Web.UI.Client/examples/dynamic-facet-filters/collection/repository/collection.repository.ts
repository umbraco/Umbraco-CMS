import type { ExampleProductModel } from '../../data/types.js';
import { ExampleDynamicFacetCollectionDataSource } from './collection.data-source.js';
import type { ExampleDynamicFacetCollectionFilterModel } from './types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';

export class ExampleDynamicFacetCollectionRepository
	extends UmbRepositoryBase
	implements UmbCollectionRepository<ExampleProductModel, ExampleDynamicFacetCollectionFilterModel>
{
	#dataSource = new ExampleDynamicFacetCollectionDataSource();

	async requestCollection(args: ExampleDynamicFacetCollectionFilterModel) {
		return this.#dataSource.getCollection(args);
	}
}

export { ExampleDynamicFacetCollectionRepository as api };
