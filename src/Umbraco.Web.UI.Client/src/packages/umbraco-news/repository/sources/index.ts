import type {
	NewsDashboardResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbNewsDataSource {
	getNewsItems(): Promise<UmbDataSourceResponse<NewsDashboardResponseModel>>;
}

