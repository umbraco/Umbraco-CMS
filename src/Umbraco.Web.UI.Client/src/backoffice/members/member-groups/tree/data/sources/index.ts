import type { DataSourceResponse } from '@umbraco-cms/models';
import { EntityTreeItemModel, PagedEntityTreeItemModel } from '@umbraco-cms/backend-api';

export interface MemberGroupTreeDataSource {
	getRootItems(): Promise<DataSourceResponse<PagedEntityTreeItemModel>>;
	getItems(key: Array<string>): Promise<DataSourceResponse<EntityTreeItemModel[]>>;
}
