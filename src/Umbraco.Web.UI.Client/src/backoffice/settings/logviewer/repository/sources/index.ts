import {
	DirectionModel,
	LogLevelCountsModel,
	LogLevelModel,
	PagedLoggerModel,
	PagedLogMessageModel,
	PagedLogTemplateModel,
	PagedSavedLogSearchModel,
	SavedLogSearchModel,
} from '@umbraco-cms/backend-api';
import type { DataSourceResponse } from '@umbraco-cms/models';



export interface LogSearchDataSource {
	getAllSavedSearches({
		skip,
		take,
	}: {
		skip?: number;
		take?: number;
	}): Promise<DataSourceResponse<PagedSavedLogSearchModel>>;
	getSavedSearchByName({ name }: { name: string }): Promise<DataSourceResponse<SavedLogSearchModel>>;
	deleteSavedSearchByName({ name }: { name: string }): Promise<DataSourceResponse<unknown>>;
	postLogViewerSavedSearch({
		requestBody,
	}: {
		requestBody?: SavedLogSearchModel;
	}): Promise<DataSourceResponse<unknown>>;
}

export interface LogMessagesDataSource {
	getLogViewerLevel({ skip, take }: { skip?: number; take?: number }): Promise<DataSourceResponse<PagedLoggerModel>>;
	getLogViewerLevelCount({
		startDate,
		endDate,
	}: {
		startDate?: string;
		endDate?: string;
	}): Promise<DataSourceResponse<LogLevelCountsModel>>;
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
	}): Promise<DataSourceResponse<PagedLogMessageModel>>;
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
	}): Promise<DataSourceResponse<PagedLogTemplateModel>>;
}
