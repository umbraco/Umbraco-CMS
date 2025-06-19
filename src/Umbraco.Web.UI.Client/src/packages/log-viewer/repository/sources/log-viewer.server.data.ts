import type { LogMessagesDataSource, LogSearchDataSource } from './index.js';
import type {
	DirectionModel,
	LogLevelModel,
	SavedLogSearchResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { LogViewerService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

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
		return await tryExecute(this.#host, LogViewerService.getLogViewerSavedSearch({ query: { skip, take } }));
	}
	/**
	 * Get a log viewer saved search by name from the server
	 * @param {{ name: string }} { name }
	 * @returns {*}
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async getSavedSearchByName({ name }: { name: string }) {
		return await tryExecute(this.#host, LogViewerService.getLogViewerSavedSearchByName({ path: { name } }));
	}

	/**
	 *	Post a new log viewer saved search to the server
	 * @param {{ body?: SavedLogSearch }} { body }
	 * @returns {*}
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async postLogViewerSavedSearch({ name, query }: SavedLogSearchResponseModel) {
		return await tryExecute(this.#host, LogViewerService.postLogViewerSavedSearch({ body: { name, query } }));
	}
	/**
	 * Remove a log viewer saved search by name from the server
	 * @param {{ name: string }} { name }
	 * @returns {*}
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async deleteSavedSearchByName({ name }: { name: string }) {
		return await tryExecute(this.#host, LogViewerService.deleteLogViewerSavedSearchByName({ path: { name } }));
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
		return await tryExecute(this.#host, LogViewerService.getLogViewerLevel({ query: { skip, take } }));
	}

	/**
	 * Grabs all the number of different log messages from the server
	 * @param {{ skip?: number; take?: number }} { skip = 0, take = 100 }
	 * @returns {*}
	 * @memberof UmbLogMessagesServerDataSource
	 */
	async getLogViewerLevelCount({ startDate, endDate }: { startDate?: string; endDate?: string }) {
		return await tryExecute(
			this.#host,
			LogViewerService.getLogViewerLevelCount({
				query: { startDate, endDate },
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
		return await tryExecute(
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
		return await tryExecute(
			this.#host,
			LogViewerService.getLogViewerMessageTemplate({
				query: { skip, take, startDate, endDate },
			}),
		);
	}

	async getLogViewerValidateLogsSize({ startDate, endDate }: { startDate?: string; endDate?: string }) {
		return await tryExecute(
			this.#host,
			LogViewerService.getLogViewerValidateLogsSize({
				query: { startDate, endDate },
			}),
		);
	}
}
