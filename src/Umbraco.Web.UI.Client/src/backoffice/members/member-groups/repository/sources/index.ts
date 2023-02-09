import type { DataSourceResponse } from '@umbraco-cms/models';
import { EntityTreeItem, PagedEntityTreeItem } from '@umbraco-cms/backend-api';

export interface MemberGroupTreeDataSource {
	getRootItems(): Promise<DataSourceResponse<PagedEntityTreeItem>>;
	getItems(key: Array<string>): Promise<DataSourceResponse<EntityTreeItem[]>>;
}
