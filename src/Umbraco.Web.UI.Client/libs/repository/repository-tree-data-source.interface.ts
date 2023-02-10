import { EntityTreeItemModel, PagedEntityTreeItemModel } from '@umbraco-cms/backend-api';
import type { DataSourceResponse } from '@umbraco-cms/models';

export interface RepositoryTreeDataSource {
	getRootItems(): Promise<DataSourceResponse<PagedEntityTreeItemModel>>;
	getChildrenOf(parentKey: string): Promise<DataSourceResponse<PagedEntityTreeItemModel>>;
	getItems(key: Array<string>): Promise<DataSourceResponse<EntityTreeItemModel[]>>;
}
