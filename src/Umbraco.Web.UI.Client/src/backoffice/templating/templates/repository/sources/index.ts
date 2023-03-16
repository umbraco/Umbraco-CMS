import type { DataSourceResponse } from '@umbraco-cms/models';
import type { EntityTreeItemResponseModel, PagedEntityTreeItemResponseModel } from '@umbraco-cms/backend-api';

export interface TemplateTreeDataSource {
	getRootItems(): Promise<DataSourceResponse<PagedEntityTreeItemResponseModel>>;
	getChildrenOf(parentKey: string): Promise<DataSourceResponse<PagedEntityTreeItemResponseModel>>;
	getItems(key: Array<string>): Promise<DataSourceResponse<EntityTreeItemResponseModel[]>>;
}
