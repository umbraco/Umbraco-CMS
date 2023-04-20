import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { ItemResponseModelBaseModel, PagedEntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbRelationTypeTreeDataSource {
	getRootItems(): Promise<DataSourceResponse<PagedEntityTreeItemResponseModel>>;
	getItems(ids: Array<string>): Promise<DataSourceResponse<ItemResponseModelBaseModel[]>>;
}
