import type { DataSourceResponse } from '@umbraco-cms/backoffice/models';
import { EntityTreeItemResponseModel, PagedEntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export interface RelationTypeTreeDataSource {
	getRootItems(): Promise<DataSourceResponse<PagedEntityTreeItemResponseModel>>;
	getItems(key: Array<string>): Promise<DataSourceResponse<EntityTreeItemResponseModel[]>>;
}
