import type { DataSourceResponse, UmbPagedData } from '@umbraco-cms/backoffice/repository';
import type { ItemResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel } from '@umbraco-cms/backoffice/tree';

export interface TemplateTreeDataSource {
	getRootItems(): Promise<DataSourceResponse<UmbPagedData<UmbEntityTreeItemModel>>>;
	getChildrenOf(parentId: string): Promise<DataSourceResponse<UmbPagedData<UmbEntityTreeItemModel>>>;
	getItems(ids: Array<string>): Promise<DataSourceResponse<ItemResponseModelBaseModel[]>>;
}
