import type {
	DirectionModel,
	LogLevelCountsReponseModel,
	LogLevelModel,
	PagedLoggerResponseModel,
	PagedLogMessageResponseModel,
	PagedLogTemplateResponseModel,
	PagedSavedLogSearchResponseModel,
	SavedLogSearchResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface LogSearchDataSource {
	getAllSavedSearches({
		skip,
		take,
	}: {
		skip?: number;
		take?: number;
	}): Promise<UmbDataSourceResponse<PagedSavedLogSearchResponseModel>>;
	getSavedSearchByName({ name }: { name: string }): Promise<UmbDataSourceResponse<SavedLogSearchResponseModel>>;
	deleteSavedSearchByName({ name }: { name: string }): Promise<UmbDataSourceResponse<unknown>>;
	postLogViewerSavedSearch({ name, query }: SavedLogSearchResponseModel): Promise<UmbDataSourceResponse<unknown>>;
}

export interface LogMessagesDataSource {
	getLogViewerLevel({
		skip,
		take,
	}: {
		skip?: number;
		take?: number;
	}): Promise<UmbDataSourceResponse<PagedLoggerResponseModel>>;
	getLogViewerLevelCount({
		startDate,
		endDate,
	}: {
		startDate?: string;
		endDate?: string;
	}): Promise<UmbDataSourceResponse<LogLevelCountsReponseModel>>;
	getLogViewerLogs({
		skip,
		take,
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
	}): Promise<UmbDataSourceResponse<PagedLogMessageResponseModel>>;
	getLogViewerMessageTemplate({
		skip,
		take,
		startDate,
		endDate,
	}: {
		skip?: number;
		take?: number;
		startDate?: string;
		endDate?: string;
	}): Promise<UmbDataSourceResponse<PagedLogTemplateResponseModel>>;
}
