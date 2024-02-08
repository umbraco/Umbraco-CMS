import type { DataSourceResponse } from '../index.js';
import type { UmbPagedModel } from './types.js';

export interface UmbCollectionDataSource<CollectionItemType, FilterType = unknown> {
	getCollection(filter: FilterType): Promise<DataSourceResponse<UmbPagedModel<CollectionItemType>>>;
}
