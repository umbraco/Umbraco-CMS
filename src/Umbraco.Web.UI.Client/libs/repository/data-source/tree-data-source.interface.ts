import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbTreeDataSource<PagedItemsType = any, ItemsType = any> {
	getRootItems(): Promise<DataSourceResponse<PagedItemsType>>;
	getChildrenOf(parentUnique: string): Promise<DataSourceResponse<PagedItemsType>>;

	// TODO: remove this when all repositories are migrated to the new items interface
	getItems(unique: Array<string>): Promise<DataSourceResponse<Array<ItemsType>>>;
}
