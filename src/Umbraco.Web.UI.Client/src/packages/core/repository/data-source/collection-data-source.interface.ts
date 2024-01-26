import type { DataSourceResponse } from '../index.js';
import type { UmbPagedModel } from './types.js';

export interface UmbCollectionDataSource<ItemType = any, FilterType = unknown> {
	getCollection(filter: FilterType): Promise<DataSourceResponse<UmbPagedModel<ItemType>>>;
}
