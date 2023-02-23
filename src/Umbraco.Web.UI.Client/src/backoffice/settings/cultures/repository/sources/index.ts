import { PagedCultureModel } from '@umbraco-cms/backend-api';
import type { DataSourceResponse } from '@umbraco-cms/models';

// TODO: This is a temporary solution until we have a proper paging interface
type paging = {
	skip: number;
	take: number;
};

export interface UmbCultureDataSource {
	getCollection(paging: paging): Promise<DataSourceResponse<PagedCultureModel>>;
}
