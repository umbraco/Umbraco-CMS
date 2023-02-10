import { PagedSavedLogSearch, SavedLogSearch } from '@umbraco-cms/backend-api';
import type { DataSourceResponse } from '@umbraco-cms/models';

export interface LogSearchDataSource {
	getAllSavedSearches({
		skip,
		take,
	}: {
		skip?: number;
		take?: number;
	}): Promise<DataSourceResponse<PagedSavedLogSearch>>;
	getSavedSearchByName({ name }: { name: string }): Promise<DataSourceResponse<SavedLogSearch>>;
	deleteSavedSearchByName({ name }: { name: string }): Promise<DataSourceResponse<unknown>>;
	postLogViewerSavedSearch({ requestBody }: { requestBody?: SavedLogSearch }): Promise<DataSourceResponse<unknown>>;
}
