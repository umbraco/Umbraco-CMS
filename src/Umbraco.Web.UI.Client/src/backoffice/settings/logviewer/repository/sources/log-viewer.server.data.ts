import { LogMessagesDataSource, LogSearchDataSource } from '.';
import { DirectionModel, LogLevelModel, LogViewerResource, SavedLogSearchModel } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

/**
 * A data source for the log saved searches
 * @export
 * @class UmbLogSearchesServerDataSource
 * @implements {TemplateDetailDataSource}
 */
export class UmbLogSearchesServerDataSource implements LogSearchDataSource {
	#host: UmbControllerHostInterface;

	/**
	 * Creates an instance of UmbLogSearchesServerDataSource.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbLogSearchesServerDataSource
	 */
	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
	}

	/**
	 * Grabs all the log viewer saved searches from the server
	 *
	 * @param {{ skip?: number; take?: number }} { skip = 0, take = 100 }
	 * @return {*}
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async getAllSavedSearches({ skip = 0, take = 100 }: { skip?: number; take?: number }) {
		return await tryExecuteAndNotify(this.#host, LogViewerResource.getLogViewerSavedSearch({ skip, take }));
	}
	/**
	 * Get a log viewer saved search by name from the server
	 *
	 * @param {{ name: string }} { name }
	 * @return {*}
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async getSavedSearchByName({ name }: { name: string }) {
		return await tryExecuteAndNotify(this.#host, LogViewerResource.getLogViewerSavedSearchByName({ name }));
	}

	/**
	 *	Post a new log viewer saved search to the server
	 *
	 * @param {{ requestBody?: SavedLogSearch }} { requestBody }
	 * @return {*}
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async postLogViewerSavedSearch({ requestBody }: { requestBody?: SavedLogSearchModel }) {
		return await tryExecuteAndNotify(this.#host, LogViewerResource.postLogViewerSavedSearch({ requestBody }));
	}
	/**
	 * Remove a log viewer saved search by name from the server
	 *
	 * @param {{ name: string }} { name }
	 * @return {*}
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async deleteSavedSearchByName({ name }: { name: string }) {
		return await tryExecuteAndNotify(this.#host, LogViewerResource.deleteLogViewerSavedSearchByName({ name }));
	}
}
	/**
	 * A data source for the log messages and levels 
	 *
	 * @export
	 * @class UmbLogMessagesServerDataSource
	 * @implements {LogMessagesDataSource}
	 */
	export class UmbLogMessagesServerDataSource implements LogMessagesDataSource {
		#host: UmbControllerHostInterface;

		/**
		 * Creates an instance of UmbLogMessagesServerDataSource.
		 * @param {UmbControllerHostInterface} host
		 * @memberof UmbLogMessagesServerDataSource
		 */
		constructor(host: UmbControllerHostInterface) {
			this.#host = host;
		}

		/**
		 * Grabs all the loggers from the server
		 *
		 * @param {{ skip?: number; take?: number }} { skip = 0, take = 100 }
		 * @return {*}
		 * @memberof UmbLogMessagesServerDataSource
		 */
		async getLogViewerLevel({ skip = 0, take = 100 }: { skip?: number; take?: number }) {
			return await tryExecuteAndNotify(this.#host, LogViewerResource.getLogViewerLevel({ skip, take }));
		}

		/**
		 * Grabs all the number of different log messages from the server
		 *
		 * @param {{ skip?: number; take?: number }} { skip = 0, take = 100 }
		 * @return {*}
		 * @memberof UmbLogMessagesServerDataSource
		 */
		async getLogViewerLevelCount({ startDate, endDate }: { startDate?: string; endDate?: string }) {
			return await tryExecuteAndNotify(
				this.#host,
				LogViewerResource.getLogViewerLevelCount({
					startDate,
					endDate,
				})
			);
		}
		/**
		 *	Grabs all the log messages from the server
		 *
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
		 * @return {*}
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
				LogViewerResource.getLogViewerLog({
					skip,
					take,
					orderDirection,
					filterExpression,
					logLevel,
					startDate,
					endDate,
				})
			);
		}
		/**
		 * Grabs all the log message templates from the server
		 *
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
		 * @return {*}
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
				LogViewerResource.getLogViewerMessageTemplate({
					skip,
					take,
					startDate,
					endDate,
				})
			);
		}

		async getLogViewerValidateLogsSize({ startDate, endDate }: { startDate?: string; endDate?: string }) {
			return await tryExecuteAndNotify(
				this.#host,
				LogViewerResource.getLogViewerValidateLogsSize({
					startDate,
					endDate,
				})
			);
		}
	}
