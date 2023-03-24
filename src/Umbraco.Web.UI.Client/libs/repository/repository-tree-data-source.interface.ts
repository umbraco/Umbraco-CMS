import { EntityTreeItemResponseModel, PagedEntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface RepositoryTreeDataSource<T = PagedEntityTreeItemResponseModel> {
	getRootItems(): Promise<DataSourceResponse<T>>;
	getChildrenOf(parentKey: string): Promise<DataSourceResponse<T>>;
	getItems(key: Array<string>): Promise<DataSourceResponse<EntityTreeItemResponseModel[]>>;
}
