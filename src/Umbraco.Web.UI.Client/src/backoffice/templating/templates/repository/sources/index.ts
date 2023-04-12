import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';
import type { ItemResponseModelBaseModel, PagedEntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export interface TemplateTreeDataSource {
	getRootItems(): Promise<DataSourceResponse<PagedEntityTreeItemResponseModel>>;
	getChildrenOf(parentId: string): Promise<DataSourceResponse<PagedEntityTreeItemResponseModel>>;
	getItems(ids: Array<string>): Promise<DataSourceResponse<ItemResponseModelBaseModel[]>>;
}
