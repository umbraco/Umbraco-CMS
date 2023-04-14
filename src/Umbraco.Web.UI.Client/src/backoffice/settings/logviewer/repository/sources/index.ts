import type {
	DirectionModel,
	LogLevelCountsReponseModel,
	LogLevelModel,
	PagedLoggerResponseModel,
	PagedLogMessageResponseModel,
	PagedLogTemplateResponseModel,
	PagedSavedLogSearchResponseModel,
	SavedLogSearchResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface LogSearchDataSource {
	getAllSavedSearches({
		skip,
		take,
	}: {
		skip?: number;
		take?: number;
	}): Promise<DataSourceResponse<PagedSavedLogSearchResponseModel>>;
	getSavedSearchByName({ name }: { name: string }): Promise<DataSourceResponse<SavedLogSearchResponseModel>>;
	deleteSavedSearchByName({ name }: { name: string }): Promise<DataSourceResponse<unknown>>;
	postLogViewerSavedSearch({ name, query }: SavedLogSearchResponseModel): Promise<DataSourceResponse<unknown>>;
}

export interface LogMessagesDataSource {
	getLogViewerLevel({
		skip,
		take,
	}: {
		skip?: number;
		take?: number;
	}): Promise<DataSourceResponse<PagedLoggerResponseModel>>;
	getLogViewerLevelCount({
		startDate,
		endDate,
	}: {
		startDate?: string;
		endDate?: string;
	}): Promise<DataSourceResponse<LogLevelCountsReponseModel>>;
	getLogViewerLogs({
		skip,
		take = 100,
		orderDirection,
		filterExpression,
		logLevel,
		startDate,
		endDate,
	}: {
		skip?: number;
		take?: number;
		orderDirection?: DirectionModel;
		filterExpression?: string;
		logLevel?: Array<LogLevelModel>;
		startDate?: string;
		endDate?: string;
	}): Promise<DataSourceResponse<PagedLogMessageResponseModel>>;
	getLogViewerMessageTemplate({
		skip,
		take = 100,
		startDate,
		endDate,
	}: {
		skip?: number;
		take?: number;
		startDate?: string;
		endDate?: string;
	}): Promise<DataSourceResponse<PagedLogTemplateResponseModel>>;
}
