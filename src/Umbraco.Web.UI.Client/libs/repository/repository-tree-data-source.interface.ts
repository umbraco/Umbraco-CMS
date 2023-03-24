import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbTreeDataSource<PagedItemsType, ItemsType> {
	getRootItems(): Promise<DataSourceResponse<PagedItemsType>>;
	getChildrenOf(parentUnique: string): Promise<DataSourceResponse<PagedItemsType>>;
	getItems(unique: Array<string>): Promise<DataSourceResponse<Array<ItemsType>>>;
}
