import type { UmbLogLevelCounts } from '../../types.js';
import type { UmbLogMessagesDataSource, UmbLogSearchDataSource } from './index.js';
import {
	LogLevelModel,
	type DirectionModel,
	type PagedLoggerResponseModel,
	type PagedLogMessageResponseModel,
	type PagedLogTemplateResponseModel,
	type PagedSavedLogSearchResponseModel,
	type SavedLogSearchResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { LogViewerService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceErrorResponse, UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for the log saved searches
 * @class UmbLogSearchesServerDataSource
 * @implements {UmbLogSearchDataSource}
 */
export class UmbLogSearchesServerDataSource implements UmbLogSearchDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbLogSearchesServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbLogSearchesServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Grabs all the log viewer saved searches from the server
	 * @param {{ skip?: number; take?: number }} query - Pagination options.
	 * @returns {Promise<UmbDataSourceResponse<PagedSavedLogSearchResponseModel>>} The saved searches.
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async getAllSavedSearches({
		skip = 0,
		take = 100,
	}: {
		skip?: number;
		take?: number;
	}): Promise<UmbDataSourceResponse<PagedSavedLogSearchResponseModel>> {
		return await tryExecute(this.#host, LogViewerService.getLogViewerSavedSearch({ query: { skip, take } }));
	}
	/**
	 * Get a log viewer saved search by name from the server
	 * @param {{ name: string }} path - The name of the saved search.
	 * @returns {Promise<UmbDataSourceResponse<SavedLogSearchResponseModel>>} The saved search.
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async getSavedSearchByName({ name }: { name: string }) {
		return await tryExecute(this.#host, LogViewerService.getLogViewerSavedSearchByName({ path: { name } }));
	}

	/**
	 *	Post a new log viewer saved search to the server
	 * @param {SavedLogSearchResponseModel} request - The saved search to create.
	 * @returns {Promise<UmbDataSourceErrorResponse>} The created saved search.
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async postLogViewerSavedSearch({ name, query }: SavedLogSearchResponseModel): Promise<UmbDataSourceErrorResponse> {
		return await tryExecute(this.#host, LogViewerService.postLogViewerSavedSearch({ body: { name, query } }));
	}
	/**
	 * Remove a log viewer saved search by name from the server
	 * @param {{ name: string }} path - The name of the saved search to remove.
	 * @returns {Promise<UmbDataSourceErrorResponse>} The result of the delete operation.
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async deleteSavedSearchByName({ name }: { name: string }) {
		return await tryExecute(this.#host, LogViewerService.deleteLogViewerSavedSearchByName({ path: { name } }));
	}
}
/**
 * A data source for the log messages and levels
 * @class UmbLogMessagesServerDataSource
 * @implements {UmbLogMessagesDataSource}
 */
export class UmbLogMessagesServerDataSource implements UmbLogMessagesDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbLogMessagesServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbLogMessagesServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Grabs all the loggers from the server
	 * @param {{ skip?: number; take?: number }} query - Pagination options.
	 * @returns {Promise<UmbDataSourceResponse<PagedLoggerResponseModel>>} The loggers.
	 * @memberof UmbLogMessagesServerDataSource
	 */
	async getLogViewerLevel({
		skip = 0,
		take = 100,
	}: {
		skip?: number;
		take?: number;
	}): Promise<UmbDataSourceResponse<PagedLoggerResponseModel>> {
		return tryExecute(this.#host, LogViewerService.getLogViewerLevel({ query: { skip, take } }));
	}

	/**
	 * Grabs all the number of different log messages from the server
	 * @param {{ startDate?: string; endDate?: string }} query - The date range to count log messages for.
	 * @returns {Promise<UmbDataSourceResponse<UmbLogLevelCounts>>} The log level counts.
	 * @memberof UmbLogMessagesServerDataSource
	 */
	async getLogViewerLevelCount({
		startDate,
		endDate,
	}: {
		startDate?: string;
		endDate?: string;
	}): Promise<UmbDataSourceResponse<UmbLogLevelCounts>> {
		const data = await tryExecute(
			this.#host,
			LogViewerService.getLogViewerLevelCount({
				query: { startDate, endDate },
			}),
		);

		if (data?.data) {
			const normalizedData: UmbLogLevelCounts = {
				[LogLevelModel.VERBOSE]: 0,
				[LogLevelModel.DEBUG]: 0,
				[LogLevelModel.INFORMATION]: 0,
				[LogLevelModel.WARNING]: 0,
				[LogLevelModel.ERROR]: 0,
				[LogLevelModel.FATAL]: 0,
			};

			// Helper to normalize log level keys to PascalCase
			const normalizeLogLevel = (level: string): LogLevelModel => {
				const normalized = level.charAt(0).toUpperCase() + level.slice(1).toLowerCase();
				return normalized as LogLevelModel;
			};

			// Normalize keys to match LogLevelModel
			for (const [level, count] of Object.entries(data.data)) {
				normalizedData[normalizeLogLevel(level)] = count;
			}

			return { data: normalizedData };
		}

		return {};
	}
	/**
	 *	Grabs all the log messages from the server
	 * @param {{
	 * 		skip?: number;
	 * 		take?: number;
	 * 		orderDirection?: DirectionModel;
	 * 		filterExpression?: string;
	 * 		logLevel?: Array<LogLevelModel>;
	 * 		startDate?: string;
	 * 		endDate?: string;
	 * 	}} query - Filtering and pagination options.
	 * @returns {Promise<UmbDataSourceResponse<PagedLogMessageResponseModel>>} The log messages.
	 * @memberof UmbLogMessagesServerDataSource
	 */
	async getLogViewerLogs({
		skip = 0,
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
	}): Promise<UmbDataSourceResponse<PagedLogMessageResponseModel>> {
		return tryExecute(
			this.#host,
			LogViewerService.getLogViewerLog({
				query: {
					skip,
					take,
					orderDirection,
					filterExpression,
					logLevel: logLevel?.length ? logLevel : undefined,
					startDate,
					endDate,
				},
			}),
		);
	}
	/**
	 * Grabs all the log message templates from the server
	 * @param {{
	 * 		skip?: number;
	 * 		take?: number;
	 * 		startDate?: string;
	 * 		endDate?: string;
	 * 	}} query - Filtering and pagination options.
	 * @returns {Promise<UmbDataSourceResponse<PagedLogTemplateResponseModel>>} The log message templates.
	 * @memberof UmbLogMessagesServerDataSource
	 */
	async getLogViewerMessageTemplate({
		skip,
		take = 100,
		startDate,
		endDate,
	}: {
		skip?: number;
		take?: number;
		startDate?: string;
		endDate?: string;
	}): Promise<UmbDataSourceResponse<PagedLogTemplateResponseModel>> {
		return tryExecute(
			this.#host,
			LogViewerService.getLogViewerMessageTemplate({
				query: { skip, take, startDate, endDate },
			}),
		);
	}

	async getLogViewerValidateLogsSize({ startDate, endDate }: { startDate?: string; endDate?: string }) {
		return tryExecute(
			this.#host,
			LogViewerService.getLogViewerValidateLogsSize({
				query: { startDate, endDate },
			}),
		);
	}
}
