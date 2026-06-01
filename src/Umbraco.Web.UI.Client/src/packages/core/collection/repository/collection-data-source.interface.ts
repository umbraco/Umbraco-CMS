import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionFilterModel } from '../collection-filter-model.interface.js';

export interface UmbCollectionDataSource<
	CollectionItemType extends { entityType: string; unique: string } = any,
	FilterType extends UmbCollectionFilterModel = UmbCollectionFilterModel,
> {
	getCollection(filter: FilterType): Promise<UmbDataSourceResponse<UmbPagedModel<CollectionItemType>>>;
}
