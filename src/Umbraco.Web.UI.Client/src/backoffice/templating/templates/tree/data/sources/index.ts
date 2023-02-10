import type { DataSourceResponse } from '@umbraco-cms/models';
import type { EntityTreeItemModel, PagedEntityTreeItemModel } from '@umbraco-cms/backend-api';

export interface TemplateTreeDataSource {
	getRootItems(): Promise<DataSourceResponse<PagedEntityTreeItemModel>>;
	getChildrenOf(parentKey: string): Promise<DataSourceResponse<PagedEntityTreeItemModel>>;
	getItems(key: Array<string>): Promise<DataSourceResponse<EntityTreeItemModel[]>>;
}
