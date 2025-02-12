import type { UmbDataSourceResponse } from '../../repository/index.js';
import type { UmbPagedModel } from '../../repository/types.js';
import type { UmbCollectionFilterModel } from '../collection-filter-model.interface.js';

export interface UmbCollectionDataSource<
	CollectionItemType extends { entityType: string; unique: string } = any,
	FilterType extends UmbCollectionFilterModel = UmbCollectionFilterModel,
> {
	getCollection(filter: FilterType): Promise<UmbDataSourceResponse<UmbPagedModel<CollectionItemType>>>;
}
