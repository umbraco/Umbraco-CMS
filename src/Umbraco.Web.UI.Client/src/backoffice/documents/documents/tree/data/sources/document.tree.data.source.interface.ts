import { EntityTreeItem, PagedEntityTreeItem } from "@umbraco-cms/backend-api";
import type { DataSourceResponse } from "@umbraco-cms/models";

export interface DocumentTreeDataSource {
	getRootItems(): Promise<DataSourceResponse<PagedEntityTreeItem>>;
	getChildrenOf(parentKey: string): Promise<DataSourceResponse<PagedEntityTreeItem>>;
	getItems(key: Array<string>): Promise<DataSourceResponse<EntityTreeItem[]>>;
}
