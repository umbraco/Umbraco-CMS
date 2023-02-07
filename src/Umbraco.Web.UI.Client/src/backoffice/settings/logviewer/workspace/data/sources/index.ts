import { PagedSavedLogSearch, Template } from '@umbraco-cms/backend-api';
import type { DataSourceResponse } from '@umbraco-cms/models';

interface skipNtake {
	skip?: number;
	take?: number;
}

export interface LogSearchDataSource {
	getLogViewerSavedSearch(skipNtake: skipNtake): Promise<DataSourceResponse<PagedSavedLogSearch>>;
}
