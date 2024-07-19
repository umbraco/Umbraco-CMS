import { UmbLogMessagesServerDataSource, UmbLogSearchesServerDataSource } from './sources/log-viewer.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	DirectionModel,
	LogLevelModel,
	SavedLogSearchResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export class UmbLogViewerRepository {
	#host: UmbControllerHost;
	#searchDataSource: UmbLogSearchesServerDataSource;
	#messagesDataSource: UmbLogMessagesServerDataSource;

	constructor(host: UmbControllerHost) {
		this.#host = host;
		this.#searchDataSource = new UmbLogSearchesServerDataSource(this.#host);
		this.#messagesDataSource = new UmbLogMessagesServerDataSource(this.#host);
	}

	async getSavedSearches({ skip, take }: { skip: number; take: number }) {
		return this.#searchDataSource.getAllSavedSearches({ skip, take });
	}

	async saveSearch({ name, query }: SavedLogSearchResponseModel) {
		return this.#searchDataSource.postLogViewerSavedSearch({ name, query });
	}

	async removeSearch({ name }: { name: string }) {
		return this.#searchDataSource.deleteSavedSearchByName({ name });
	}

	async getMessageTemplates({
		skip,
		take,
		startDate,
		endDate,
	}: {
		skip: number;
		take: number;
		startDate?: string;
		endDate?: string;
	}) {
		return this.#messagesDataSource.getLogViewerMessageTemplate({ skip, take, startDate, endDate });
	}

	async getLogCount({ startDate, endDate }: { startDate?: string; endDate?: string }) {
		return this.#messagesDataSource.getLogViewerLevelCount({ startDate, endDate });
	}

	async getLogs({
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
		return this.#messagesDataSource.getLogViewerLogs({
			skip,
			take,
			orderDirection,
			filterExpression,
			logLevel,
			startDate,
			endDate,
		});
	}

	async getLogLevels({ skip = 0, take = 100 }: { skip: number; take: number }) {
		return this.#messagesDataSource.getLogViewerLevel({ skip, take });
	}

	async getLogViewerValidateLogsSize({ startDate, endDate }: { startDate?: string; endDate?: string }) {
		return this.#messagesDataSource.getLogViewerValidateLogsSize({ startDate, endDate });
	}
}
