import { ExampleProductCollectionDataSource } from './collection.data-source.js';
import type { ExampleProductCollectionFilterModel, ExampleProductCollectionItemModel } from './types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';

export class ExampleProductCollectionRepository
	extends UmbRepositoryBase
	implements UmbCollectionRepository<ExampleProductCollectionItemModel, ExampleProductCollectionFilterModel>
{
	#dataSource = new ExampleProductCollectionDataSource();

	async requestCollection(args: ExampleProductCollectionFilterModel) {
		return this.#dataSource.getCollection(args);
	}
}

export { ExampleProductCollectionRepository as api };
