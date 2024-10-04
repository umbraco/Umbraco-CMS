import type { LogMessagesDataSource, LogSearchDataSource } from './index.js';
import type {
	DirectionModel,
	LogLevelModel,
	SavedLogSearchResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { LogViewerService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the log saved searches
 * @class UmbLogSearchesServerDataSource
 * @implements {TemplateDetailDataSource}
 */
export class UmbLogSearchesServerDataSource implements LogSearchDataSource {
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
	 * @param {{ skip?: number; take?: number }} { skip = 0, take = 100 }
	 * @returns {*}
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async getAllSavedSearches({ skip = 0, take = 100 }: { skip?: number; take?: number }) {
		return await tryExecuteAndNotify(this.#host, LogViewerService.getLogViewerSavedSearch({ skip, take }));
	}
	/**
	 * Get a log viewer saved search by name from the server
	 * @param {{ name: string }} { name }
	 * @returns {*}
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async getSavedSearchByName({ name }: { name: string }) {
		return await tryExecuteAndNotify(this.#host, LogViewerService.getLogViewerSavedSearchByName({ name }));
	}

	/**
	 *	Post a new log viewer saved search to the server
	 * @param {{ requestBody?: SavedLogSearch }} { requestBody }
	 * @returns {*}
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async postLogViewerSavedSearch({ name, query }: SavedLogSearchResponseModel) {
		return await tryExecuteAndNotify(
			this.#host,
			LogViewerService.postLogViewerSavedSearch({ requestBody: { name, query } }),
		);
	}
	/**
	 * Remove a log viewer saved search by name from the server
	 * @param {{ name: string }} { name }
	 * @returns {*}
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async deleteSavedSearchByName({ name }: { name: string }) {
		return await tryExecuteAndNotify(this.#host, LogViewerService.deleteLogViewerSavedSearchByName({ name }));
	}
}
/**
 * A data source for the log messages and levels
 * @class UmbLogMessagesServerDataSource
 * @implements {LogMessagesDataSource}
 */
export class UmbLogMessagesServerDataSource implements LogMessagesDataSource {
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
	 * @param {{ skip?: number; take?: number }} { skip = 0, take = 100 }
	 * @returns {*}
	 * @memberof UmbLogMessagesServerDataSource
	 */
	async getLogViewerLevel({ skip = 0, take = 100 }: { skip?: number; take?: number }) {
		return await tryExecuteAndNotify(this.#host, LogViewerService.getLogViewerLevel({ skip, take }));
	}

	/**
	 * Grabs all the number of different log messages from the server
	 * @param {{ skip?: number; take?: number }} { skip = 0, take = 100 }
	 * @returns {*}
	 * @memberof UmbLogMessagesServerDataSource
	 */
	async getLogViewerLevelCount({ startDate, endDate }: { startDate?: string; endDate?: string }) {
		return await tryExecuteAndNotify(
			this.#host,
			LogViewerService.getLogViewerLevelCount({
				startDate,
				endDate,
			}),
		);
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
	 * 	}} {
	 * 		skip = 0,
	 * 		take = 100,
	 * 		orderDirection,
	 * 		filterExpression,
	 * 		logLevel,
	 * 		startDate,
	 * 		endDate,
	 * 	}
	 * @returns {*}
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
	}) {
		return await tryExecuteAndNotify(
			this.#host,
			LogViewerService.getLogViewerLog({
				skip,
				take,
				orderDirection,
				filterExpression,
				logLevel,
				startDate,
				endDate,
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
	 * 	}} {
	 * 		skip,
	 * 		take = 100,
	 * 		startDate,
	 * 		endDate,
	 * 	}
	 * @returns {*}
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
	}) {
		return await tryExecuteAndNotify(
			this.#host,
			LogViewerService.getLogViewerMessageTemplate({
				skip,
				take,
				startDate,
				endDate,
			}),
		);
	}

	async getLogViewerValidateLogsSize({ startDate, endDate }: { startDate?: string; endDate?: string }) {
		return await tryExecuteAndNotify(
			this.#host,
			LogViewerService.getLogViewerValidateLogsSize({
				startDate,
				endDate,
			}),
		);
	}
}
