import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';
import type {
	EntityTreeItemResponseModel,
	PagedEntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export interface MemberTreeDataSource {
	getRootItems(): Promise<DataSourceResponse<PagedEntityTreeItemResponseModel>>;
	getItems(id: Array<string>): Promise<DataSourceResponse<EntityTreeItemResponseModel[]>>;
}
