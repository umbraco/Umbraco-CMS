import type { UmbDataSourceResponse } from '../../repository/index.js';
import type { UmbPagedModel } from '../../repository/types.js';

export interface UmbCollectionDataSource<CollectionItemType, FilterType = unknown> {
	getCollection(filter: FilterType): Promise<UmbDataSourceResponse<UmbPagedModel<CollectionItemType>>>;
}
