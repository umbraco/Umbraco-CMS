import type { DataSourceResponse } from '../../repository/index.js';
import type { UmbPagedModel } from '../../repository/types.js';

export interface UmbCollectionDataSource<CollectionItemType, FilterType = unknown> {
	getCollection(filter: FilterType): Promise<DataSourceResponse<UmbPagedModel<CollectionItemType>>>;
}
