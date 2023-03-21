import { EntityTreeItemResponseModel, PagedEntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface RepositoryTreeDataSource {
	getRootItems(): Promise<DataSourceResponse<PagedEntityTreeItemResponseModel>>;
	getChildrenOf(parentKey: string): Promise<DataSourceResponse<PagedEntityTreeItemResponseModel>>;
	getItems(key: Array<string>): Promise<DataSourceResponse<EntityTreeItemResponseModel[]>>;
}
