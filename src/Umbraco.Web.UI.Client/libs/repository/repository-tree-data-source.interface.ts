import { EntityTreeItemResponseModel, PagedEntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { DataSourceResponse } from '@umbraco-cms/backoffice/models';

export interface RepositoryTreeDataSource {
	getRootItems(): Promise<DataSourceResponse<PagedEntityTreeItemResponseModel>>;
	getChildrenOf(parentKey: string): Promise<DataSourceResponse<PagedEntityTreeItemResponseModel>>;
	getItems(key: Array<string>): Promise<DataSourceResponse<EntityTreeItemResponseModel[]>>;
}
