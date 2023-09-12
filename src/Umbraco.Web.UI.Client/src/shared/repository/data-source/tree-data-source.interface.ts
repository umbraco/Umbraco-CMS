import type { UmbPagedData } from '../tree-repository.interface.js';
import type { DataSourceResponse } from './data-source-response.interface.js';

export interface UmbTreeDataSource<ItemType = any, PagedItemType = UmbPagedData<ItemType>> {
	getRootItems(): Promise<DataSourceResponse<PagedItemType>>;
	getChildrenOf(parentUnique: string | null): Promise<DataSourceResponse<PagedItemType>>;
	// TODO: remove this when all repositories are migrated to the new items interface
	getItems?(unique: Array<string>): Promise<DataSourceResponse<Array<any>>>;
}
