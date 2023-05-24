import type { UmbPagedData } from '../tree-repository.interface.js';
import type { DataSourceResponse } from '../index.js';

export interface UmbCollectionDataSource<ItemType = any, PagedItemType = UmbPagedData<ItemType>> {
	getCollection(): Promise<DataSourceResponse<PagedItemType>>;
	filterCollection(filter: any): Promise<DataSourceResponse<PagedItemType>>;
}
